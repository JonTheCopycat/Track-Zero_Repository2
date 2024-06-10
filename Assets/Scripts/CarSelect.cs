using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cars;
using UISystems;

public class CarSelect : MonoBehaviour
{
    public GameObject highlight;
    public GameObject[] CarButtons;
    ScreenManager screenManager;
    public CarStats carStatsDisplay;
    
    // Start is called before the first frame update
    void Start()
    {
        int carIndex = CarCollection.localCarIndex;
        highlight.transform.position = CarButtons[carIndex].transform.position;

        screenManager = ScreenManager.current;

        Car currentCar = CarCollection.FindCarByIndex(carIndex);
        carStatsDisplay.SetStats(
                currentCar.GetMaxSpeed(),
                currentCar.GetAcceleration(),
                currentCar.GetDriftAcceleration(),
                currentCar.GetHandling(),
                currentCar.GetDHandling(),
                currentCar.GetBoostStrength(),
                currentCar.GetTraction()
                );
    }

    private void OnDestroy()
    {
    }

    private void OnApplicationQuit()
    {
        OnDestroy();
    }


    public void PickLocalCar(string name)
    {
        if (CarCollection.localCarIndex == CarCollection.GetIndex(name))
        {
            GoToNext();
        }
        else
        {
            CarCollection.localCarIndex = CarCollection.GetIndex(name);

            highlight.transform.position = CarButtons[CarCollection.localCarIndex].transform.position;

            Car currentCar = CarCollection.FindCarByName(name);
            carStatsDisplay.SetStats(
                currentCar.GetMaxSpeed(),
                currentCar.GetAcceleration(),
                currentCar.GetDriftAcceleration(),
                currentCar.GetHandling(),
                currentCar.GetDHandling(),
                currentCar.GetBoostStrength(),
                currentCar.GetTraction()
                );
        }
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
            screenManager.AnimateScreenOut(transform.gameObject);
            screenManager.AnimateScreenOut(menu);
            MainMenuNavigation.current.OpenOnline("lobby");
        }
        else
        {
            screenManager.AnimateScreenIn(GameObject.Find("Track Select"));
            screenManager.AnimateScreenOut(transform.gameObject);
            MainMenuNavigation.current.OpenTrackSelect();
        }
    }

    public GameObject GetCurrentCarButton()
    {
        return CarButtons[CarCollection.localCarIndex];
    }
}
