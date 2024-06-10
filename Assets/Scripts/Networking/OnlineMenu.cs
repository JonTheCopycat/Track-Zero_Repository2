using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Cars;

public class OnlineMenu : MonoBehaviour
{
    [SerializeField]
    private Button startServerButton;

    [SerializeField]
    private Button startHostButton;

    [SerializeField]
    private Button startClientButton;

    [SerializeField]
    private Button disconnectButton;

    [SerializeField]
    private Button ChangeTrackButton;

    [SerializeField]
    private GameObject menuScreen;

    [SerializeField]
    private Button ReadyButton;

    [SerializeField]
    private GameObject lobbyScreen;

    [SerializeField]
    private Text LobbyDisplay;

    private bool connected = false;
    private NetworkPlayer localPlayerObject;

    // Start is called before the first frame update
    void Start()
    {

        startHostButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started...");
                ScreenManager.current.AnimateScreenIn(lobbyScreen);
                ScreenManager.current.AnimateScreenOut(menuScreen);
                MainMenuNavigation.current.OpenOnline("lobby");
                localPlayerObject = NetworkPlayer.GetLocalNetworkPlayer();
                connected = true;
                

                //myIndex = playerIDs.Count;
                ChangeTrackButton.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("Host could not be started");
            }
        });

        startClientButton.onClick.AddListener(() =>
        {
            
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started...");
                ScreenManager.current.AnimateScreenIn(lobbyScreen);
                ScreenManager.current.AnimateScreenOut(menuScreen);
                MainMenuNavigation.current.OpenOnline("lobby");
                localPlayerObject = NetworkPlayer.GetLocalNetworkPlayer();
                connected = true;

                //playerIDs.Add(NetworkManager.Singleton.LocalClientId);
                //playerChosenCars.Add(CarCollection.localCarIndex);
                //playersReady.Add(false);
                //myIndex = playerIDs.Count - 1;

                ChangeTrackButton.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("Client could not be started");
            }
        });

        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started...");
                connected = true;
            }
            else
            {
                Debug.Log("Server could not be started");
            }
        });

        disconnectButton.onClick.AddListener(() =>
        {
            ScreenManager.current.AnimateScreenIn(menuScreen);
            ScreenManager.current.AnimateScreenOut(lobbyScreen);
            MainMenuNavigation.current.OpenOnline("menu");
            connected = false; 
            NetworkManager.Singleton.Shutdown();

            NetworkPlayer.allPlayerIds.Clear();
            NetworkPlayer.allPlayerObjects.Clear();
            NetworkPlayer.allPlayers.Clear();
        });

        ReadyButton.onClick.AddListener(() =>
        {
            if (localPlayerObject != null)
                localPlayerObject.ToggleReadyStatusServerRpc();
        });

    }

    // Update is called once per frame
    void Update()
    {
        if (connected)
        {
            if (localPlayerObject == null)
            {
                localPlayerObject = NetworkPlayer.GetLocalNetworkPlayer();
            }
            if (localPlayerObject != null)
            {
                LobbyDisplay.text = $"{NetworkPlayer.PlayersInGame} Players In Lobby\n\n";
                //if (NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>() != null)
                //{
                //    int carID = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>().ChosenCar;
                //    string carName = CarCollection.FindCarByIndex(carID).GetName();

                //    LobbyDisplay.text += $"{NetworkManager.Singleton.LocalClientId}\n" +
                //        $"{carName}";
                //}
                //else
                //{
                //    Debug.LogError("bruh\nhurry up and connect bitch");
                //}

                for (int i = 0; i < NetworkPlayer.allPlayerIds.Count; i++)
                {
                    if (localPlayerObject.PlayerId == NetworkPlayer.allPlayerIds[i])
                    {
                        LobbyDisplay.text += $"Player {NetworkPlayer.allPlayerIds[i]} (You)\n" +
                        $"{CarCollection.FindCarByIndex(NetworkPlayer.allPlayerObjects[i].ChosenCar).GetName()}\n" +
                        $"Ready: {NetworkPlayer.allPlayerObjects[i].PlayerReady}\n\n";
                    }
                    else
                    {
                        LobbyDisplay.text += $"Player {NetworkPlayer.allPlayerIds[i]}\n" +
                        $"{CarCollection.FindCarByIndex(NetworkPlayer.allPlayerObjects[i].ChosenCar).GetName()}\n" +
                        $"Ready: {NetworkPlayer.allPlayerObjects[i].PlayerReady}\n\n";
                    }
                    
                }
            }
            else
            {
                LobbyDisplay.text = "Connecting...";
            }
        }
    }

    //int carID = NetworkManager.Singleton.ConnectedClientsList[i].PlayerObject.GetComponent<NetworkPlayer>().ChosenCar;
    //string carName = CarCollection.FindCarByIndex(carID).GetName();
}
