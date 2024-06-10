using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UISystems;
using Cars;
using CarBehaviour;
using Replays;
using AI;


public class Spawner : MonoBehaviour
{
    public IngameUI ingameUI;
    public CheckpointList CheckpointList;

    public GameObject PlayerCar;
    public GameObject GhostCar;
    public GameObject CPUCar;
    public GameObject OnlineCar;
    public CarMeshScriptable carMeshCollection;
    public bool normalGravity;
    public bool overrideLaps;
    public int overrideLapCount;
    public bool overrideGamemode;
    public GameSettings.GameMode gamemode;

    private CameraControl cameraControl;

    private GameObject currentObject;

    private List<int> allAvailableCarIDs;

    private void RepopulateAvailableCars()
    {
        int tier = CarCollection.FindCarByIndex(CarCollection.localCarIndex).GetTier();
        //only pick cars of the same tier as the car the player picks
        
        allAvailableCarIDs = new List<int>();
        for (int i = 0; i < CarCollection.allTiers[tier - 1].Count; i++)
        {
            allAvailableCarIDs.Add(CarCollection.allTiers[tier - 1][i]);
        }
    }

    private int PopRandomCarId()
    {
        //edge case, list empty
        if (allAvailableCarIDs.Count == 0)
        {
            RepopulateAvailableCars();
        }

        int index = Random.Range(0, allAvailableCarIDs.Count - 1);
        int realId = allAvailableCarIDs[index];
        allAvailableCarIDs.RemoveAt(index);
        return realId;
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraControl>();
        CarCollection.InitializeTiers();
        Time.timeScale = 0;
        RepopulateAvailableCars();

        if (overrideGamemode)
        GameSettings.gamemode = gamemode;
        
        CarAI.aiPath = PathDataManager.LoadAIPath(ScreenManager.current.GetSceneName());
        if (CarAI.aiPath.GetSize() > 0)
            CarAI.aiPath.CalculateMaxDepth();
        CarControl.normalGravity = normalGravity;
        
        int carsInARow;
        if (GameSettings.gamemode == GameSettings.GameMode.PRACTICE)
        {
            carsInARow = 1;
            CarControl.SetCarsInARow(carsInARow);

            currentObject = Instantiate(PlayerCar);
            currentObject.name = "Player";
            currentObject.SetActive(false);

            //player info
            CarGetter carGetter = currentObject.GetComponent<CarGetter>();
            carGetter.carMeshCollection = carMeshCollection;

            CarInputHandler inputHandler = currentObject.GetComponent<CarInputHandler>();
            inputHandler.playerNum = 0;
            inputHandler.inputType = CarInputHandler.InputType.PLAYER;

            CarAI carAI = currentObject.GetComponent<CarAI>();
            

            CarControl carControl = currentObject.GetComponent<CarControl>();

            CarEffects carEffects = currentObject.GetComponent<CarEffects>();

            PlayerInfo playerInfo = currentObject.GetComponent<PlayerInfo>();
            playerInfo.main = true;
            playerInfo.totalLaps = 0;
            playerInfo.CheckpointList = CheckpointList;

            ReplayManager replayManager = currentObject.GetComponent<ReplayManager>();
            replayManager.observing = false;
            replayManager.replaying = false;

            currentObject.SetActive(true);

            //manually start everything
            carGetter.CustomStart();
            inputHandler.CustomStart();
            carAI.CustomStart();
            carControl.CustomStart();
            carEffects.CustomStart();
            playerInfo.CustomStart();
            replayManager.CustomStart();

            cameraControl.target = currentObject.GetComponent<Rigidbody>();
        }
        else if (GameSettings.gamemode == GameSettings.GameMode.TIMETRIAL)
        {
            currentObject = Instantiate(PlayerCar);
            currentObject.name = "Player";
            currentObject.SetActive(false);
            CarControl.SetCarsInARow(1);


            //player info
            CarGetter carGetter = currentObject.GetComponent<CarGetter>();
            carGetter.carMeshCollection = carMeshCollection;

            CarInputHandler inputHandler = currentObject.GetComponent<CarInputHandler>();
            inputHandler.playerNum = 0;
            inputHandler.inputType = CarInputHandler.InputType.PLAYER;

            CarAI carAI = currentObject.GetComponent<CarAI>();

            CarControl carControl = currentObject.GetComponent<CarControl>();

            CarEffects carEffects = currentObject.GetComponent<CarEffects>();

            PlayerInfo playerInfo = currentObject.GetComponent<PlayerInfo>();
            playerInfo.main = true;
            if (overrideLaps)
                playerInfo.totalLaps = overrideLapCount;
            else
                playerInfo.totalLaps = 2;

            playerInfo.CheckpointList = CheckpointList;

            ReplayManager replayManager = currentObject.GetComponent<ReplayManager>();
            replayManager.observing = true;
            replayManager.replaying = false;

            currentObject.SetActive(true);

            //manually start everything
            carGetter.CustomStart();
            inputHandler.CustomStart();
            carAI.CustomStart();
            carControl.CustomStart();
            carEffects.CustomStart();
            playerInfo.CustomStart();
            replayManager.CustomStart();

            cameraControl.target = currentObject.GetComponent<Rigidbody>();

            //spawn the ghost of last replay
            if (ReplayStorage.lastReplay != null && ReplayStorage.lastReplay.GetTrack() == ScreenManager.current.GetSceneName())
            {
                currentObject = Instantiate(GhostCar);
                currentObject.name = "LastReplayGhost";
                currentObject.SetActive(false);
                CarControl.SetCarsInARow(1);

                //player info
                carGetter = currentObject.GetComponent<CarGetter>();
                carGetter.carMeshCollection = carMeshCollection;
                carGetter.providedCar = ReplayStorage.lastReplay.GetCar();
                carGetter.usingProvidedCar = true;

                inputHandler = currentObject.GetComponent<CarInputHandler>();
                inputHandler.playerNum = 1;
                inputHandler.inputType = CarInputHandler.InputType.REPLAY;

                carAI = currentObject.GetComponent<CarAI>();

                carControl = currentObject.GetComponent<CarControl>();

                carEffects = currentObject.GetComponent<CarEffects>();

                playerInfo = currentObject.GetComponent<PlayerInfo>();
                playerInfo.main = false;
                if (overrideLaps)
                    playerInfo.totalLaps = overrideLapCount;
                else
                    playerInfo.totalLaps = 2;

                playerInfo.CheckpointList = CheckpointList;

                replayManager = currentObject.GetComponent<ReplayManager>();
                replayManager.currentReplay = ReplayStorage.lastReplay;
                replayManager.observing = false;
                replayManager.replaying = true;

                currentObject.SetActive(true);

                //manually start everything
                carGetter.CustomStart();
                inputHandler.CustomStart();
                carAI.CustomStart();
                carControl.CustomStart();
                carEffects.CustomStart();
                playerInfo.CustomStart();
                replayManager.CustomStart();

            }

            Replay loadedReplay = ReplayStorage.LoadReplay(Application.version + "-" + ScreenManager.current.GetSceneName() + "-" + "tier" + carGetter.GetCar().GetTier() + "-bestTime");
            if (loadedReplay != null && loadedReplay != ReplayStorage.lastReplay && PlayerPrefs.HasKey(Application.version + "-" + ScreenManager.current.GetSceneName() + "-" + "tier" + carGetter.GetCar().GetTier() + "-bestTime"))
            {
                currentObject = Instantiate(GhostCar);
                currentObject.name = "BestTimeGhost";
                currentObject.SetActive(false);

                //player info
                carGetter = currentObject.GetComponent<CarGetter>();
                carGetter.carMeshCollection = carMeshCollection;
                carGetter.providedCar = loadedReplay.GetCar();
                carGetter.usingProvidedCar = true;

                inputHandler = currentObject.GetComponent<CarInputHandler>();
                inputHandler.playerNum = 2;
                inputHandler.inputType = CarInputHandler.InputType.REPLAY;

                carAI = currentObject.GetComponent<CarAI>();

                carControl = currentObject.GetComponent<CarControl>();

                carEffects = currentObject.GetComponent<CarEffects>();

                playerInfo = currentObject.GetComponent<PlayerInfo>();
                playerInfo.main = false;
                if (overrideLaps)
                    playerInfo.totalLaps = overrideLapCount;
                else
                    playerInfo.totalLaps = 2;

                playerInfo.CheckpointList = CheckpointList;

                replayManager = currentObject.GetComponent<ReplayManager>();
                replayManager.currentReplay = loadedReplay;
                replayManager.observing = false;
                replayManager.replaying = true;

                currentObject.SetActive(true);

                //manually start everything
                carGetter.CustomStart();
                inputHandler.CustomStart();
                carAI.CustomStart();
                carControl.CustomStart();
                carEffects.CustomStart();
                playerInfo.CustomStart();
                replayManager.CustomStart();
            }
        }
        else if (GameSettings.gamemode == GameSettings.GameMode.VS)
        {
            carsInARow = GameSettings.cpuCount + 1;
            if (carsInARow > 6)
                carsInARow = 6;
            CarControl.SetCarsInARow(carsInARow);

            currentObject = Instantiate(PlayerCar);
            currentObject.name = "Player";
            currentObject.SetActive(false);

            //player info
            CarGetter carGetter = currentObject.GetComponent<CarGetter>();
            carGetter.carMeshCollection = carMeshCollection;

            CarInputHandler inputHandler = currentObject.GetComponent<CarInputHandler>();
            inputHandler.playerNum = 0;
            inputHandler.inputType = CarInputHandler.InputType.PLAYER;

            CarAI carAI = currentObject.GetComponent<CarAI>();

            CarControl carControl = currentObject.GetComponent<CarControl>();

            CarEffects carEffects = currentObject.GetComponent<CarEffects>();

            PlayerInfo playerInfo = currentObject.GetComponent<PlayerInfo>();
            playerInfo.main = true;
            if (overrideLaps)
                playerInfo.totalLaps = Mathf.Max((int)(overrideLapCount * GameSettings.laps / 2), 1);
            else
                playerInfo.totalLaps = GameSettings.laps;
            playerInfo.CheckpointList = CheckpointList;

            ReplayManager replayManager = currentObject.GetComponent<ReplayManager>();
            replayManager.observing = false;
            replayManager.replaying = false;

            currentObject.SetActive(true);

            //manually start everything
            carGetter.CustomStart();
            inputHandler.CustomStart();
            carAI.CustomStart();
            carControl.CustomStart();
            carEffects.CustomStart();
            playerInfo.CustomStart();
            replayManager.CustomStart();

            cameraControl.target = currentObject.GetComponent<Rigidbody>();

            for (int i = 1; i <= GameSettings.cpuCount; i++)
            {
                currentObject = Instantiate(CPUCar);
                currentObject.name = "CPU" + i;
                currentObject.SetActive(false);

                carGetter = currentObject.GetComponent<CarGetter>();
                carGetter.carMeshCollection = carMeshCollection;
                carGetter.overrideCar = true;
                carGetter.carId = PopRandomCarId();

                inputHandler = currentObject.GetComponent<CarInputHandler>();
                inputHandler.playerNum = i;
                inputHandler.inputType = CarInputHandler.InputType.COMPUTER;

                carAI = currentObject.GetComponent<CarAI>();

                carControl = currentObject.GetComponent<CarControl>();

                carEffects = currentObject.GetComponent<CarEffects>();

                playerInfo = currentObject.GetComponent<PlayerInfo>();
                if (overrideLaps)
                    playerInfo.totalLaps = Mathf.Max((int)(overrideLapCount * GameSettings.laps / 2), 1);
                else
                    playerInfo.totalLaps = GameSettings.laps;
                playerInfo.main = false;
                playerInfo.CheckpointList = CheckpointList;

                replayManager = currentObject.GetComponent<ReplayManager>();
                replayManager.observing = false;
                replayManager.replaying = false;

                currentObject.SetActive(true);

                //manually start everything
                carGetter.CustomStart();
                inputHandler.CustomStart();
                carAI.CustomStart();
                carControl.CustomStart();
                carEffects.CustomStart();
                playerInfo.CustomStart();
                replayManager.CustomStart();
            }
        }
        else if (GameSettings.gamemode == GameSettings.GameMode.CPUVS)
        {
            carsInARow = GameSettings.cpuCount;
            if (carsInARow > 6)
                carsInARow = 6;
            CarControl.SetCarsInARow(carsInARow);

            //player info
            CarGetter carGetter;

            CarInputHandler inputHandler;

            CarAI carAI;

            CarControl carControl;

            CarEffects carEffects;

            PlayerInfo playerInfo;

            ReplayManager replayManager;

            for (int i = 1; i <= GameSettings.cpuCount; i++)
            {
                currentObject = Instantiate(CPUCar);
                currentObject.name = "CPU" + i;
                currentObject.SetActive(false);

                carGetter = currentObject.GetComponent<CarGetter>();
                carGetter.carMeshCollection = carMeshCollection;
                if (i == 1)
                {
                    carGetter.overrideCar = false;
                }
                else
                {
                    carGetter.overrideCar = true;
                    carGetter.carId = PopRandomCarId();
                }
                

                inputHandler = currentObject.GetComponent<CarInputHandler>();
                inputHandler.playerNum = i - 1;
                inputHandler.inputType = CarInputHandler.InputType.COMPUTER;

                carAI = currentObject.GetComponent<CarAI>();

                carControl = currentObject.GetComponent<CarControl>();

                carEffects = currentObject.GetComponent<CarEffects>();

                playerInfo = currentObject.GetComponent<PlayerInfo>();
                if (overrideLaps)
                    playerInfo.totalLaps = Mathf.Max((int)(overrideLapCount * GameSettings.laps / 3), 1);
                else
                    playerInfo.totalLaps = GameSettings.laps;
                playerInfo.CheckpointList = CheckpointList;

                replayManager = currentObject.GetComponent<ReplayManager>();
                replayManager.observing = false;
                replayManager.replaying = false;

                currentObject.SetActive(true);

                if (i == 1)
                {
                    cameraControl.target = currentObject.GetComponent<Rigidbody>();
                    playerInfo.main = true;
                }
                else
                {
                    playerInfo.main = false;
                }

                //manually start everything
                carGetter.CustomStart();
                inputHandler.CustomStart();
                carAI.CustomStart();
                carControl.CustomStart();
                carEffects.CustomStart();
                playerInfo.CustomStart();
                replayManager.CustomStart();
                
            }
        }
        else if (GameSettings.gamemode == GameSettings.GameMode.REPLAY)
        {
            Replay loadedReplay = ReplayStorage.LoadReplay(ScreenManager.current.GetSceneName() + "BestTime");
            if (loadedReplay == null)
            {
                ScreenManager.current.GoToScene("Main Menu");
                Debug.LogWarning("Ouch! That's seriously weak dude");
            }

            currentObject = Instantiate(GhostCar);
            currentObject.name = "BestTimeGhost";
            currentObject.SetActive(false);

            //player info
            CarGetter carGetter = currentObject.GetComponent<CarGetter>();
            carGetter.carMeshCollection = carMeshCollection;
            carGetter.providedCar = loadedReplay.GetCar();
            carGetter.usingProvidedCar = true;

            CarInputHandler inputHandler = currentObject.GetComponent<CarInputHandler>();
            inputHandler.playerNum = 0;
            inputHandler.inputType = CarInputHandler.InputType.REPLAY;

            CarAI carAI = currentObject.GetComponent<CarAI>();

            CarControl carControl = currentObject.GetComponent<CarControl>();

            CarEffects carEffects = currentObject.GetComponent<CarEffects>();

            PlayerInfo playerInfo = currentObject.GetComponent<PlayerInfo>();
            playerInfo.main = true;
            if (overrideLaps)
                playerInfo.totalLaps = overrideLapCount;
            else
                playerInfo.totalLaps = 3;

            playerInfo.CheckpointList = CheckpointList;

            ReplayManager replayManager = currentObject.GetComponent<ReplayManager>();
            replayManager.currentReplay = loadedReplay;
            replayManager.observing = false;
            replayManager.replaying = true;

            currentObject.SetActive(true);

            //manually start everything
            carGetter.CustomStart();
            inputHandler.CustomStart();
            carAI.CustomStart();
            carControl.CustomStart();
            carEffects.CustomStart();
            playerInfo.CustomStart();
            replayManager.CustomStart();

            cameraControl.target = currentObject.GetComponent<Rigidbody>();
        }
        else if (GameSettings.gamemode == GameSettings.GameMode.NULL)
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i].GetComponent<CarGetter>() != null)
                {
                    if (gameObjects[i].GetComponent<CarGetter>().isActiveAndEnabled)
                        gameObjects[i].GetComponent<CarGetter>().CustomStart();

                    if (gameObjects[i].GetComponent<CarInputHandler>().isActiveAndEnabled)
                        gameObjects[i].GetComponent<CarInputHandler>().CustomStart();

                    if (gameObjects[i].GetComponent<CarAI>().isActiveAndEnabled)
                        gameObjects[i].GetComponent<CarAI>().CustomStart();

                    if (gameObjects[i].GetComponent<CarControl>().isActiveAndEnabled)
                        gameObjects[i].GetComponent<CarControl>().CustomStart();

                    if (gameObjects[i].GetComponent<CarEffects>().isActiveAndEnabled)
                        gameObjects[i].GetComponent<CarEffects>().CustomStart();

                    if (gameObjects[i].GetComponent<PlayerInfo>().isActiveAndEnabled)
                        gameObjects[i].GetComponent<PlayerInfo>().CustomStart();

                    if (gameObjects[i].GetComponent<ReplayManager>().isActiveAndEnabled)
                        gameObjects[i].GetComponent<ReplayManager>().CustomStart();
                }
            }
        }

        GameObject[] objectList = GameObject.FindGameObjectsWithTag("GameUI");
        foreach (GameObject obj in objectList)
        {
            obj.BroadcastMessage("ActualStart");
        }
                
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
