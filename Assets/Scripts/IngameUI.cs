using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UISystems;
using Cars;
using CarBehaviour;
using AI;

public class IngameUI : MonoBehaviour
{
    public GameObject GameScreen;
    public GameObject FinishScreen;
    public GameObject PauseScreen;

    private ScreenManager screenManager;

    private GameObject currentScreen;

    public GameObject Finish_FirstSelect;
    public GameObject Pause_FirstSelect;
    public GameObject lastButton;

    public Text timeText;
    public Text velocityText;
    public TextMeshProUGUI wrongWayText;
    public BoostBar boostBar;

    public Text finishText;
    public Scrollbar scrollbar;

    private List<float> previousLapTimes = new List<float>();
    private float bestLapTime;
    private float bestCarTime;
    private float totalTime;
    private float bestTime;
    private float fullStamp;
    private float lapStamp;
    private List<int> lapCount = new List<int>();

    List<(GameObject, int)> racingPlayers; /* the object and it's place in the race*/
    List<(GameObject, float)> finishedPlayers; /* the object and its total time*/
    GameObject mainPlayer;
    Rigidbody mainRigidbody;
    PlayerInfo playerInfo;
    CarControl mainCarControl;
    string mainCarName;
    string sceneName;
    int mainID;

    bool started = false;
    bool paused = false;

    // Start is called before the first frame update
    public void ActualStart()
    {
        screenManager = ScreenManager.current;
        sceneName = ScreenManager.current.GetSceneName();
        currentScreen = GameScreen;
        wrongWayText.alpha = 0;

        racingPlayers = new List<(GameObject, int)>();
        finishedPlayers = new List<(GameObject, float)>();

        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i].tag.Equals("Player"))
            {
                racingPlayers.Add((gameObjects[i], racingPlayers.Count));
                lapCount.Add(0);

                if (racingPlayers[racingPlayers.Count - 1].Item1.GetComponent<PlayerInfo>().main == true)
                {
                    mainPlayer = racingPlayers[racingPlayers.Count - 1].Item1;
                    mainRigidbody = mainPlayer.GetComponent<Rigidbody>();
                    playerInfo = mainPlayer.GetComponent<PlayerInfo>();
                    mainCarControl = mainPlayer.GetComponent<CarControl>();
                    mainID = racingPlayers.Count - 1;
                    mainCarName = mainPlayer.GetComponent<CarGetter>().GetCar().GetName();

                    boostBar.SetMaxBoost(CarCollection.FindCarByName(mainCarName).GetBoostStrength());
                }
            }
        }

        if (GameSettings.gamemode == GameSettings.GameMode.TIMETRIAL)
        {
            if (ScreenManager.current != null)
            {
                if (PlayerPrefs.HasKey(Application.version + "-" + sceneName + "-" + "tier" + CarCollection.FindCarByName(mainCarName).GetTier() + "-bestTime"))
                {
                    bestTime = PlayerPrefs.GetFloat(Application.version + "-" + sceneName + "-" + "tier" + CarCollection.FindCarByName(mainCarName).GetTier() + "-bestTime", -1);
                    bestCarTime = PlayerPrefs.GetFloat(Application.version + "-" + ScreenManager.current.GetSceneName() + mainPlayer.GetComponent<CarGetter>().GetCar().GetName() + "-bestCarTime", -1); ;
                    bestLapTime = PlayerPrefs.GetFloat(Application.version + "-" + sceneName + "-" + "tier" + CarCollection.FindCarByName(mainCarName).GetTier() + "-bestLapTime");

                    //for testing purposes only
                    //PlayerPrefs.DeleteKey("1.test-" + ScreenManager.current.GetSceneName() + "-bestTime");
                    //PlayerPrefs.DeleteKey("1.test-" + ScreenManager.current.GetSceneName() + "-bestFirstLapTime");

                    //bestTime = -1;
                    //bestFirstLapTime = -1;

                    Debug.Log("Best times loaded");
                }
                else
                {
                    bestLapTime = -1;
                    bestTime = -1;

                    Debug.Log("No records found");
                }
            }
            else
            {
                bestLapTime = -1;
            }
        }

        PlayerInfo.FinishedLap += SendLapData;
        PlayerInfo.FinishedRace += Finish;

        started = true;
        Debug.Log("IngameUI started");
    }

    private void OnDestroy()
    {
        if (GameSettings.gamemode == GameSettings.GameMode.TIMETRIAL)
        {
            //Debug.Log("Scene Name: " + sceneName);
            
            PlayerPrefs.SetFloat(Application.version + "-" + sceneName + "-" + "tier" + CarCollection.FindCarByName(mainCarName).GetTier() + "-bestTime", bestTime);
            PlayerPrefs.SetFloat(Application.version + "-" + sceneName + mainCarName + "-bestCarTime", bestCarTime);
            PlayerPrefs.SetFloat(Application.version + "-" + sceneName + "-" + "tier" + CarCollection.FindCarByName(mainCarName).GetTier() + "-bestLapTime", bestLapTime);

            PlayerPrefs.Save();
        }

        PlayerInfo.FinishedLap -= SendLapData;
        PlayerInfo.FinishedRace -= Finish;
    }

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            if (currentScreen != GameScreen)
            {
                InputManager.inputActions.UI.Enable();
                if (EventSystem.current.currentSelectedGameObject != null)
                    lastButton = EventSystem.current.currentSelectedGameObject;
                else
                {
                    //clear selected object
                    EventSystem.current.SetSelectedGameObject(null);
                    //set a new selected object
                    EventSystem.current.SetSelectedGameObject(lastButton);
                }
            }
            else
            {
                if (mainPlayer != null)
                {
                    InputManager.inputActions.UI.Disable();
                    fullStamp = playerInfo.GetFullStamp();
                    lapStamp = playerInfo.GetLapStamp();

                    if (Time.frameCount % 2 == 0)
                        UpdatePlacements();


                    //time
                    if (playerInfo.totalLaps > 0)
                    {
                        timeText.text = "Time: \t" + FormatTime(Mathf.Floor((Time.time - fullStamp) * 100) / 100) + "\n" +
                                "Lap Time: \t" + FormatTime(Mathf.Floor((Time.time - lapStamp) * 100) / 100) + "\n" +
                                "Lap " + (lapCount[mainID] + 1) + " / " + playerInfo.totalLaps + "\n\n";
                    }
                    else
                    {
                        timeText.text = "Time: \t" + FormatTime(Mathf.Floor((Time.time - fullStamp) * 100) / 100) + "\n" +
                                "Lap Time: \t" + FormatTime(Mathf.Floor((Time.time - lapStamp) * 100) / 100) + "\n" +
                                "Lap " + (lapCount[mainID] + 1) + "\n\n";
                    }

                    //placement
                    string placeEnding = "";
                    if ((racingPlayers[mainID].Item2 + finishedPlayers.Count) >= 11 && (racingPlayers[mainID].Item2 + finishedPlayers.Count) <= 13)
                    {
                        placeEnding = "th";
                    }
                    else if ((racingPlayers[mainID].Item2 + finishedPlayers.Count) % 10 == 1)
                    {
                        placeEnding = "st";
                    }
                    else if ((racingPlayers[mainID].Item2 + finishedPlayers.Count) % 10 == 2)
                    {
                        placeEnding = "nd";
                    }
                    else if ((racingPlayers[mainID].Item2 + finishedPlayers.Count) % 10 == 3)
                    {
                        placeEnding = "rd";
                    }
                    else
                    {
                        placeEnding = "th";
                    }
                    //combine them into the string
                    timeText.text += (racingPlayers[mainID].Item2 + finishedPlayers.Count) + placeEnding + " / " + (racingPlayers.Count + finishedPlayers.Count) + "\n\n";

                    //print all of the lap times
                    for (int i = Mathf.Clamp(previousLapTimes.Count - 10, 0, int.MaxValue); i < previousLapTimes.Count; i++)
                    {
                        timeText.text += "Lap " + (i + 1) + ": " + FormatTime(previousLapTimes[i]) + "\n";
                    }

                    //velocity
                    velocityText.text = Mathf.Floor(mainRigidbody.velocity.magnitude * 0.85f / CarControl.speedFactor).ToString();
                    velocityText.color = Color.HSVToRGB(
                        Mathf.Lerp(0.5f, 0, Mathf.Floor(mainRigidbody.velocity.magnitude) / (400 * CarControl.speedFactor)),
                        Mathf.Lerp(0, 1, Mathf.Floor(mainRigidbody.velocity.magnitude) / (350 * CarControl.speedFactor)),
                        1
                        );

                    //boostBar
                    boostBar.SetBoostMeter(mainCarControl.GetBoostMeter(), mainCarControl.GetDoubleBoostMeter());

                    if (playerInfo.isGoingWrongWay())
                    {
                        wrongWayText.alpha = 1;
                    }
                    else
                    {
                        wrongWayText.alpha = 0;
                    }
                }
                else
                {
                    Debug.LogError("There is no main player");
                }

                //pausing
                if (mainPlayer.GetComponent<CarInputHandler>().GetStartDown())
                {
                    //TogglePause();
                    Pause();
                }
            }


            if (currentScreen == FinishScreen)
            {
                //scrolling
                float scroll = 0;
                if (Keyboard.current[Key.W].isPressed || Keyboard.current[Key.UpArrow].isPressed)
                {
                    scroll += 1;
                }
                if (Keyboard.current[Key.S].isPressed || Keyboard.current[Key.DownArrow].isPressed)
                {
                    scroll -= 1;
                }
                if (Mathf.Abs(InputManager.inputActions.UI.Navigate.ReadValue<Vector2>().y) > 0.1f)
                {
                    scroll += InputManager.inputActions.UI.Navigate.ReadValue<Vector2>().y;
                }
                if (scroll != 0)
                {
                    scrollbar.value = Mathf.Clamp01(scrollbar.value + scroll * Time.deltaTime);
                }

                if (GameSettings.gamemode == GameSettings.GameMode.VS ||
                GameSettings.gamemode == GameSettings.GameMode.CPUVS ||
                GameSettings.gamemode == GameSettings.GameMode.ONLINE)
                {
                    if (Time.frameCount % 30 == 0)
                    {
                        UpdateFinishText();
                    }
                }
                
            }
        }
    }

    private void UpdatePlayerList()
    {
        racingPlayers = new List<(GameObject, int)>();

        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i].tag.Equals("Player"))
            {
                racingPlayers.Add((gameObjects[i], racingPlayers.Count));
                lapCount.Add(0);

                if (racingPlayers[racingPlayers.Count - 1].Item1.GetComponent<PlayerInfo>().main == true)
                {
                    mainPlayer = racingPlayers[racingPlayers.Count - 1].Item1;
                    mainRigidbody = racingPlayers[racingPlayers.Count - 1].Item1.GetComponent<Rigidbody>();
                    playerInfo = racingPlayers[racingPlayers.Count - 1].Item1.GetComponent<PlayerInfo>();
                    mainID = racingPlayers.Count - 1;
                }
            }
        }
    }

    public void Finish(GameObject who, float _totalTime, float[] allLapTimes)
    {
        bool mainFinish = false;

        for (int i = 0; i <= racingPlayers.Count; i++)
        {
            if (racingPlayers[i].Item1 == who)
            {
                if (racingPlayers[i].Item1 == mainPlayer)
                {
                    mainFinish = true;
                    mainID = -1;
                }
                else if ((GameSettings.gamemode == GameSettings.GameMode.VS ||
                GameSettings.gamemode == GameSettings.GameMode.CPUVS ||
                GameSettings.gamemode == GameSettings.GameMode.ONLINE) && mainID == -1)
                {
                    //the main player has already finished, but in these game modes, other people finishing after
                    //the player should cause the FinishScreen to update;
                    UpdateFinishText();
                }
                else if (i < mainID)
                {
                    mainID--;
                    Debug.Log("MainId now points to " + racingPlayers[i].Item1.name);
                }

                finishedPlayers.Add((racingPlayers[i].Item1, _totalTime));
                racingPlayers.RemoveAt(i);

                break;
            }
        }
        
        if (mainFinish)
        {
            if (currentScreen != FinishScreen)
            {
                screenManager.AnimateScreenIn(FinishScreen);
                screenManager.AnimateScreenOut(currentScreen);

                //clear selected object
                EventSystem.current.SetSelectedGameObject(null);
                //set a new selected object
                EventSystem.current.SetSelectedGameObject(Finish_FirstSelect);

                currentScreen = FinishScreen;

                //print the finish text
                totalTime = 0;
                for (int i = 0; i < previousLapTimes.Count; i++)
                {
                    totalTime += previousLapTimes[i];
                }
            }

            

            if (GameSettings.gamemode == GameSettings.GameMode.VS ||
                GameSettings.gamemode == GameSettings.GameMode.CPUVS ||
                GameSettings.gamemode == GameSettings.GameMode.ONLINE)
            {
                UpdateFinishText();
            }
            else if (GameSettings.gamemode == GameSettings.GameMode.TIMETRIAL)
            {
                bool newBest = false;
                bool newCarBest = false;
                bool newLapBest = false;

                float fastestLap = 0;

                //calculate the best times
                if ((bestTime <= 0.1f || _totalTime < bestTime) && !(finishedPlayers[0].Item1.GetComponent<CarGetter>().GetCar().GetName() == "random"))
                {
                    bestTime = _totalTime;
                    newBest = true;
                }

                if (bestCarTime <= 0.1f || _totalTime < bestCarTime)
                {
                    bestCarTime = _totalTime;
                    newCarBest = true;
                }

                for (int i = 0; i < previousLapTimes.Count; i++)
                {
                    if ((bestLapTime <= 0.1f || previousLapTimes[i] < bestLapTime) && !(finishedPlayers[0].Item1.GetComponent<CarGetter>().GetCar().GetName() == "random"))
                    {
                        bestLapTime = allLapTimes[i];
                        newLapBest = true;
                    }

                    if ((fastestLap <= 0.1f || allLapTimes[i] < fastestLap))
                    {
                        fastestLap = allLapTimes[i];
                    }
                }

                //print the times
                finishText.text = "Total Time: " + FormatTime(_totalTime) + "\n";
                finishText.text += "Fastest lap: " + FormatTime(fastestLap) + "\n\n";

                finishText.text += "Tier " + CarCollection.FindCarByName(mainCarName).GetTier() + "\n";
                finishText.text += "Best Time Record: " + FormatTime(bestTime) + "\n";
                if (newBest)
                    finishText.text += " (New Best!)";
                finishText.text += "\n";

                finishText.text += "Car Time Record: " + FormatTime(bestCarTime);
                if (newCarBest)
                    finishText.text += " (New Best!)";
                finishText.text += "\n";

                finishText.text += "Fastest Lap Record: " + FormatTime(bestLapTime);
                if (newLapBest)
                    finishText.text += " (New Best!)";
                
            }
            else if (GameSettings.gamemode == GameSettings.GameMode.REPLAY)
            {

                float fastestLap = 0;

                //calculate the best times
                if ((bestTime <= 0.1f || _totalTime < bestTime) && !(finishedPlayers[0].Item1.GetComponent<CarGetter>().GetCar().GetName() == "swerve"))
                {
                    bestTime = _totalTime;
                }

                if (bestCarTime <= 0.1f || _totalTime < bestCarTime)
                {
                    bestCarTime = _totalTime;
                }

                for (int i = 0; i < allLapTimes.Length; i++)
                {
                    if ((bestLapTime <= 0.1f || allLapTimes[i] < bestLapTime) && !(finishedPlayers[0].Item1.GetComponent<CarGetter>().GetCar().GetName() == "random"))
                    {
                        bestLapTime = allLapTimes[i];
                    }

                    if ((fastestLap <= 0.1f || allLapTimes[i] < fastestLap))
                    {
                        fastestLap = allLapTimes[i];
                    }
                }

                //print the times
                finishText.text = "Total Time: " + FormatTime(_totalTime) + "\n";
                finishText.text += "Fastest lap: " + FormatTime(fastestLap) + "\n\n";

                finishText.text += "Best Time Record: " + FormatTime(bestTime) + "\n";
                finishText.text += "\n";

                finishText.text += "Car Time Record: " + FormatTime(bestCarTime);
                finishText.text += "\n";

                finishText.text += "Fastest Lap Record: " + FormatTime(bestLapTime);

            }
        }
    }

    public void Pause()
    {
        if (currentScreen == GameScreen && GameSettings.gamemode != GameSettings.GameMode.ONLINE)
        {
            screenManager.AnimateScreenIn(PauseScreen);
            screenManager.AnimateScreenOut(currentScreen);

            //clear selected object
            EventSystem.current.SetSelectedGameObject(null);
            //set a new selected object
            EventSystem.current.SetSelectedGameObject(Pause_FirstSelect);

            currentScreen = PauseScreen;

            Time.timeScale = 0;
        }
        
    }

    public void Unpause()
    {
        if (currentScreen == PauseScreen)
        {
            screenManager.AnimateScreenIn(GameScreen);
            screenManager.AnimateScreenOut(currentScreen);

            //clear selected object
            EventSystem.current.SetSelectedGameObject(null);

            currentScreen = GameScreen;

            Time.timeScale = 1;
        }

    }

    public void TogglePause()
    {
        if (paused)
        {
            paused = false;
            Unpause();
        }
        else
        {
            paused = true;
            Pause();
        }
    }

    public void SendLapData(GameObject car, float time)
    {
        if (car == mainPlayer)
        {
            previousLapTimes.Add(time);
        }
        for (int i = 0; i < racingPlayers.Count; i++)
        {
            if (racingPlayers[i].Item1.Equals(car))
            {
                lapCount[i]++;
                return;
            }
        }
    }

    public void Retry()
    {
        screenManager.RestartScene();
    }

    public void Reset()
    {
        previousLapTimes = new List<float>();
        for (int i = 0; i < lapCount.Count; i++)
        {
            lapCount[i] = 0;
        }


    }

    private string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60);
        float secondsAfterMinutes = Mathf.Floor((seconds - minutes * 60) * 100) / 100;

        //built the formatted string
        string secondsFormat = "";
        if ((int)secondsAfterMinutes / 10 == 0)
        {
            secondsFormat = "0" + secondsAfterMinutes;
        }
        else
        {
            secondsFormat = secondsAfterMinutes.ToString();
        }

        return minutes + ":" + secondsFormat;
    }

    private List<(GameObject, int)> SortPlayersList()
    {
        List<(GameObject, int)> sortedPlayers = new List<(GameObject, int)>();
        //item 2 is the original id
        for (int i = 0; i < racingPlayers.Count; i++)
        {
            var currentIndex = i;
            while (currentIndex > 0)
            {
                if (lapCount[sortedPlayers[currentIndex - 1].Item2] < lapCount[i])
                {
                    currentIndex--;
                    continue;
                }
                else if (lapCount[sortedPlayers[currentIndex - 1].Item2] == lapCount[i])
                {
                    if (sortedPlayers[currentIndex - 1].Item1.GetComponent<CarAI>().GetTrackCompletion() < racingPlayers[i].Item1.GetComponent<CarAI>().GetTrackCompletion())
                    {
                        currentIndex--;
                        continue;
                    }
                }
                break;

            }
            sortedPlayers.Insert(currentIndex, (racingPlayers[i].Item1, i));
        }

        return sortedPlayers;
    }

    private void UpdatePlacements()
    {
        List<(GameObject, int)> sortedPlayers = SortPlayersList();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            racingPlayers[sortedPlayers[i].Item2] = (racingPlayers[sortedPlayers[i].Item2].Item1, i + 1);
        }

        for (int i = 0; i < racingPlayers.Count; i++)
        {

            float difficultyRubberband = 0;
            switch(GameSettings.difficulty)
            {
                case 0:
                    difficultyRubberband = -20;
                    break;
                case 1:
                    difficultyRubberband = 0;
                    break;
                case 2:
                    difficultyRubberband = 0;
                    break;
                case 3:
                    difficultyRubberband = 10;
                    break;
                case 4:
                    difficultyRubberband = 30;
                    break;
                default:
                    difficultyRubberband = 0;
                    break;

            }
            
            if (sortedPlayers.Count > 1)
            {
                if (racingPlayers[i].Item2 - racingPlayers[mainID].Item2 > 0)
                {
                    //when behind
                    racingPlayers[i].Item1.GetComponent<CarInputHandler>().SetRubberband(difficultyRubberband + (racingPlayers[i].Item1.transform.position - racingPlayers[mainID].Item1.transform.position).magnitude / 10);
                }
                else if (racingPlayers[i].Item2 - racingPlayers[mainID].Item2 < 0)
                {
                    //when ahead
                    racingPlayers[i].Item1.GetComponent<CarInputHandler>().SetRubberband(difficultyRubberband - (racingPlayers[i].Item1.transform.position - racingPlayers[mainID].Item1.transform.position).magnitude / 20);
                }
                else
                {
                    racingPlayers[i].Item1.GetComponent<CarInputHandler>().SetRubberband(difficultyRubberband);
                }
            }
            else
            {
                racingPlayers[i].Item1.GetComponent<CarInputHandler>().SetRubberband(0);
            }
        }
    }

    private void UpdateFinishText()
    {
        float scrollbarValue = scrollbar.value;

        string newText = "Total Time: " + FormatTime(totalTime) + "\n\n";

        List<(GameObject, int)> sortedPlayers = SortPlayersList();

        for (int i = 0; i < finishedPlayers.Count; i++)
        {
            newText += string.Format("{0,6}:\t{1,12}\t{2,15}\t{3,12}\n", (i + 1), finishedPlayers[i].Item1.name, finishedPlayers[i].Item1.GetComponent<CarGetter>().GetCar().GetName(), FormatTime(finishedPlayers[i].Item2));
        }

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            newText += string.Format("{0,6}:\t{1,12}\t{2,15}\t{3,8}\n", (i + 1 + finishedPlayers.Count), sortedPlayers[i].Item1.name, sortedPlayers[i].Item1.GetComponent<CarGetter>().GetCar().GetName(), "--");
        }

        finishText.text = newText;

        scrollbar.value = scrollbarValue;
    }
}
