using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UISystems;
using Cars;

public class CarSelect_New : SelectableOption
{
    private int previousCar;
    private int currentCar;
    private int nextCar;
    private int tierIndex;
    private int carIndex;

    public static List<int>[] allTiers;

    [SerializeField]
    private Text text;
    //[SerializeField]
    //private Text extraText;

    [SerializeField]
    private Image LeftImage;
    private Color leftColor;
    Coroutine leftPulse;
    [SerializeField]
    private Image RightImage;
    private Color rightColor;
    Coroutine rightPulse;
    
    [SerializeField]
    private CarStats carStatsDisplay;
    [SerializeField]
    private TierDisplay tierDisplay;

    ScreenManager screenManager;

    [SerializeField]
    private GameObject CarSelectScreen;

    [SerializeField]
    private Text PreviousCarText;
    [SerializeField]
    private Text NextCarText;

    protected override void PostStart()
    {
        leftColor = LeftImage.color;
        rightColor = RightImage.color;
        currentCar = CarCollection.localCarIndex;
        text.text = CarCollection.FindCarByIndex(currentCar).GetName();

        screenManager = ScreenManager.current;

        //update car stats
        UpdateCarStats();

        //Selected += ShowDescriptiveText;
        //Deselected += ShowNoText;
    }

    protected override void OnEnable()
    {
        if (Application.isPlaying)
        {
            CarCollection.InitializeTiers();
            allTiers = CarCollection.allTiers;

            currentCar = CarCollection.localCarIndex;
            tierIndex = CarCollection.FindCarByIndex(currentCar).GetTier() - 1;
            carIndex = 0;
            for (int i = 0; i < allTiers[tierIndex].Count; i++)
            {
                if (allTiers[tierIndex][i] == currentCar)
                {
                    carIndex = i;

                    break;
                }
            }

            UpdateSideText();
        }
    }

    protected override void OnDisable()
    {
        //Selected -= ShowDescriptiveText;
        //Deselected -= ShowNoText;
    }

    protected override void PressLeft()
    {

        CycleCar(-1);
        UpdateSideText();


        if (leftPulse != null)
        {
            StopCoroutine(leftPulse);
            LeftImage.color = leftColor;
        }
        leftPulse = StartCoroutine(ColorPulse(LeftImage, Color.green, 0.5f));

        text.text = CarCollection.FindCarByIndex(currentCar).GetName();
        //update car stats
        UpdateCarStats();
    }

    protected override void PressRight()
    {
        CycleCar(1);
        UpdateSideText();

        if (rightPulse != null)
        {
            StopCoroutine(rightPulse);
            RightImage.color = rightColor;
        }
        rightPulse = StartCoroutine(ColorPulse(RightImage, Color.green, 0.5f));

        text.text = CarCollection.FindCarByIndex(currentCar).GetName();
        //update car stats
        UpdateCarStats();
    }

    protected override void PressUp()
    {
        CycleTier(1);
        UpdateSideText();

        text.text = CarCollection.FindCarByIndex(currentCar).GetName();
        //update car stats
        UpdateCarStats();
    }

    protected override void PressDown()
    {
        CycleTier(-1);
        UpdateSideText();

        text.text = CarCollection.FindCarByIndex(currentCar).GetName();
        //update car stats
        UpdateCarStats();
    }

    protected override void PressSubmit()
    {
        CarCollection.localCarIndex = currentCar;

        GoToNext();
    }

    protected override void PressClick(Vector2 position)
    {
        Debug.Log("Position clicked: " + new Vector2(position.x / Screen.width, position.y / Screen.height));

        if (position.y / Screen.height < 0.25f)
        {
            if (position.x / Screen.width < 0.42f)
            {
                PressLeft();
            }
            else if (position.x / Screen.width > 0.58f)
            {
                PressRight();
            }
            else
            {
                PressSubmit();
            }


        }
    }

    //private void ShowDescriptiveText()
    //{
    //    if (extraText != null)
    //        extraText.text = "Press Left or Right to get access more menu options";
    //}

    //private void ShowNoText()
    //{
    //    if (extraText != null)
    //        extraText.text = string.Empty;
    //}

    void CycleCar(int timesToCycle)
    {
        carIndex += timesToCycle;
        int infiniteCyclePrevention = 0;
        do
        {
            if (carIndex >= allTiers[tierIndex].Count)
            {
                carIndex -= allTiers[tierIndex].Count;
                tierIndex = mod(tierIndex + 1, 4);
            }
            if (carIndex < 0)
            {
                tierIndex = mod(tierIndex - 1, 4);
                carIndex += allTiers[tierIndex].Count;
            }

            infiniteCyclePrevention++;
            if (infiniteCyclePrevention > 100)
            {
                Debug.LogError("Infinite Loop Detected!");
                break;
            }
        } while (carIndex >= allTiers[tierIndex].Count || carIndex < 0);

        currentCar = allTiers[tierIndex][carIndex];
    }

    void CycleTier(int timesToCycle)
    {
        tierIndex = mod(tierIndex + timesToCycle, 4);
        carIndex = 0;

        currentCar = allTiers[tierIndex][carIndex];
    }

    void UpdateSideText()
    {
        if (carIndex - 1 >= 0)
            previousCar = allTiers[tierIndex][carIndex - 1];
        else
            previousCar = -tierIndex - 1;

        if (carIndex + 1 < allTiers[tierIndex].Count)
            nextCar = allTiers[tierIndex][carIndex + 1];
        else
            nextCar = -tierIndex - 1;

        if (previousCar >= 0)
            PreviousCarText.text = CarCollection.FindCarByIndex(previousCar).GetName();
        else
        {
            switch(previousCar)
            {
                case -1:
                    PreviousCarText.text = "Special Tier";
                    break;
                case -2:
                    PreviousCarText.text = "Tier 1";
                    break;
                case -3:
                    PreviousCarText.text = "Tier 2";
                    break;
                case -4:
                    PreviousCarText.text = "Tier 3";
                    break;
                default:
                    PreviousCarText.text = "";
                    break;
            }
        }

        if (nextCar >= 0)
            NextCarText.text = CarCollection.FindCarByIndex(nextCar).GetName();
        else
        {
            switch (nextCar)
            {
                case -1:
                    NextCarText.text = "Tier 2";
                    break;
                case -2:
                    NextCarText.text = "Tier 3";
                    break;
                case -3:
                    NextCarText.text = "Special Tier";
                    break;
                case -4:
                    NextCarText.text = "Tier 1";
                    break;
                default:
                    NextCarText.text = "";
                    break;
            }
        }
    }

    void UpdateCarStats()
    {
        Car c = CarCollection.FindCarByIndex(currentCar);
        carStatsDisplay.SetStats(
                c.GetMaxSpeed(),
                c.GetAcceleration(),
                c.GetDriftAcceleration(),
                c.GetHandling(),
                c.GetOversteer(),
                c.GetBoostStrength(),
                c.GetTraction()
                );

        tierDisplay.DisplayTier(c.GetTier());
    }

    public void GoToNext()
    {
        if (GameSettings.gamemode == GameSettings.GameMode.ONLINE)
        {
            GameObject online = GameObject.Find("Online");
            GameObject lobby = online.transform.Find("Lobby").gameObject;
            GameObject menu = online.transform.Find("Menu").gameObject;
            screenManager.AnimateScreenIn(online);
            screenManager.AnimateScreenIn(lobby);
            screenManager.AnimateScreenOut(CarSelectScreen);
            screenManager.AnimateScreenOut(menu);
            MainMenuNavigation.current.OpenOnline("lobby");
        }
        else
        {
            screenManager.AnimateScreenIn(GameObject.Find("Track Select"));
            screenManager.AnimateScreenOut(CarSelectScreen);
            MainMenuNavigation.current.OpenTrackSelect();
        }
    }

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public IEnumerator ColorPulse(Image image, Color color, float fadeTime)
    {
        Color baseColor = image.color;
        float startTime = Time.time;
        float timestamp = Time.time + fadeTime;
        while (Time.time < timestamp)
        {
            image.color = Color.Lerp(color, baseColor, (Time.time - startTime) / fadeTime);
            yield return null;
        }
    }
}
