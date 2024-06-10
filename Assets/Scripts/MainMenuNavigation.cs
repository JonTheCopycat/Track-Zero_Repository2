using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Replays;

public class MainMenuNavigation : MonoBehaviour
{
    [SerializeField]
    private GameObject mainSButton, mainTTButton, mainPButton, mainVSButton, mainCPUSButton, mainOButton, mainRButton, trackFirstButton, carFirstButton, settingsFirstSelection, gamesettingsFirstSelection, onlineMenuFirstSelection, onlineLobbyFirstSelection;

    private GameObject mainLastButton, trackLastButton, settingsLastSelection, gamesettingsLastSelection, onlineLastSelection;

    public CarSelect carSelect;
    public ReplayUI replayUI;

    public GameObject SettingsScreen;
    public GameObject GameSettingsScreen;

    private enum CurrentMenu
    {
        MAIN,
        SETTINGS,
        GAMESETTINGS,
        CARSELECT,
        TRACKSELECT,
        REPLAYSELECT,
        ONLINE,
        ONLINELOBBY
    }

    private CurrentMenu currentMenu;

    private GameObject lastButton;

    public static MainMenuNavigation current;

    private void OnEnable()
    {
        current = this;
    }

    public void OnDestroy()
    {
        if (current == this)
        current = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        OpenMainMenu();
        Application.targetFrameRate = 60;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            lastButton = EventSystem.current.currentSelectedGameObject;
            if (currentMenu == CurrentMenu.MAIN)
            {
                mainLastButton = lastButton;
            }
        }
        else
        {
            //clear selected object
            EventSystem.current.SetSelectedGameObject(null);
            //set a new selected object
            EventSystem.current.SetSelectedGameObject(lastButton);
        }

        if (((InputSystemUIInputModule)EventSystem.current.currentInputModule).cancel.action.WasPressedThisFrame() && InputManager.current.isEnabled)
        {
            GoBackScreen();
        }
    }

    public void OpenMainMenu()
    {
        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);

        //set a new selected object
        if (CurrentMenu.SETTINGS == currentMenu)
        {
            EventSystem.current.SetSelectedGameObject(mainSButton);
        }
        else
        {
            switch (GameSettings.gamemode)
            {
                case GameSettings.GameMode.TIMETRIAL:
                    EventSystem.current.SetSelectedGameObject(mainTTButton);
                    break;
                case GameSettings.GameMode.PRACTICE:
                    EventSystem.current.SetSelectedGameObject(mainPButton);
                    break;
                case GameSettings.GameMode.VS:
                    EventSystem.current.SetSelectedGameObject(mainVSButton);
                    break;
                case GameSettings.GameMode.CPUVS:
                    EventSystem.current.SetSelectedGameObject(mainCPUSButton);
                    break;
                case GameSettings.GameMode.ONLINE:
                    EventSystem.current.SetSelectedGameObject(mainOButton);
                    break;
                case GameSettings.GameMode.REPLAY:
                    EventSystem.current.SetSelectedGameObject(mainRButton);
                    break;
                default:
                    EventSystem.current.SetSelectedGameObject(mainVSButton);
                    break;
            }
        }
        

        currentMenu = CurrentMenu.MAIN;
    }

    public void OpenCarSelect()
    {
        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        //set a new selected object
        EventSystem.current.SetSelectedGameObject(carFirstButton);

        currentMenu = CurrentMenu.CARSELECT;
    }

    public void OpenTrackSelect()
    {
        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        //set a new selected object
        EventSystem.current.SetSelectedGameObject(trackFirstButton);

        currentMenu = CurrentMenu.TRACKSELECT;
    }

    public void OpenSettings()
    {
        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        //set a new selected object
        EventSystem.current.SetSelectedGameObject(settingsFirstSelection);

        currentMenu = CurrentMenu.SETTINGS;
    }

    public void OpenGameSettings()
    {
        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        //set a new selected object
        EventSystem.current.SetSelectedGameObject(gamesettingsFirstSelection);

        currentMenu = CurrentMenu.GAMESETTINGS;
    }

    public void OpenOnline(string screen)
    {
        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        if (screen != null && screen.Equals("lobby"))
        {
            //set a new selected object
            EventSystem.current.SetSelectedGameObject(onlineLobbyFirstSelection);
        }
        else
        {
            //set a new selected object
            EventSystem.current.SetSelectedGameObject(onlineMenuFirstSelection);
        }
        

        currentMenu = CurrentMenu.ONLINE;
    }

    public void OpenReplays()
    {
        if (ReplayStorage.allReplays.Count == 0)
            ReplayStorage.LoadAllReplays();

        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        replayUI.SelectFirstItem();

        currentMenu = CurrentMenu.REPLAYSELECT;
    }

    public void SelectTrack(string scene)
    {
        ReplayStorage.selectedTrack = scene;

        if (GameSettings.gamemode == GameSettings.GameMode.REPLAY)
        {
            ScreenManager.current.AnimateScreenOut(trackFirstButton.transform.parent.gameObject);

            ScreenManager.current.AnimateScreenIn(transform.Find("Replay Select").gameObject);

            OpenReplays();
        } 
        else
        {
            ScreenManager.current.GoToScene(scene);
        }
    }

    public void GoBackScreen()
    {
        if (currentMenu == CurrentMenu.CARSELECT)
        {
            ScreenManager.current.AnimateScreenOut(carSelect.gameObject);

            ScreenManager.current.AnimateScreenIn(mainTTButton.transform.parent.gameObject);
            OpenMainMenu();
        }
        else if (currentMenu == CurrentMenu.SETTINGS)
        {
            ScreenManager.current.AnimateScreenOut(SettingsScreen);

            ScreenManager.current.AnimateScreenIn(mainTTButton.transform.parent.gameObject);
            OpenMainMenu();
        }
        else if (currentMenu == CurrentMenu.GAMESETTINGS)
        {
            ScreenManager.current.AnimateScreenOut(GameSettingsScreen);

            ScreenManager.current.AnimateScreenIn(mainTTButton.transform.parent.gameObject);
            OpenMainMenu();
        }
        else if (currentMenu == CurrentMenu.TRACKSELECT)
        {
            ScreenManager.current.AnimateScreenOut(trackFirstButton.transform.parent.gameObject);

            if (GameSettings.gamemode == GameSettings.GameMode.REPLAY)
            {
                ScreenManager.current.AnimateScreenIn(mainTTButton.transform.parent.gameObject);
                OpenMainMenu();
            }
            else
            {
                ScreenManager.current.AnimateScreenIn(carSelect.gameObject);
                OpenCarSelect();
            }
        } else if (currentMenu == CurrentMenu.REPLAYSELECT)
        {
            ScreenManager.current.AnimateScreenOut(transform.Find("Replay Select").gameObject); 

            ScreenManager.current.AnimateScreenIn(trackFirstButton.transform.parent.gameObject);
            OpenTrackSelect();
        }
    }

    public void SetGamemode(GameSettings.GameMode gamemode)
    {
        GameSettings.gamemode = gamemode;
    }

    public void SetModeTimetrial()
    {
        SetGamemode(GameSettings.GameMode.TIMETRIAL);
    }

    public void SetModePractice()
    {
        SetGamemode(GameSettings.GameMode.PRACTICE);
    }

    public void SetModeVS()
    {
        SetGamemode(GameSettings.GameMode.VS);
    }

    public void SetModeCPUVS()
    {
        SetGamemode(GameSettings.GameMode.CPUVS);
    }

    public void SetModeONLINE()
    {
        SetGamemode(GameSettings.GameMode.ONLINE);
    }

    public void SetModeREPLAY()
    {
        SetGamemode(GameSettings.GameMode.REPLAY);
    }
}
