using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cars;

public class NetworkPlayer : NetworkBehaviour
{
    public static List<ulong> allPlayerIds = new List<ulong>();
    public static List<NetworkPlayer>allPlayerObjects = new List<NetworkPlayer>();
    public static Dictionary<ulong, NetworkPlayer> allPlayers = new Dictionary<ulong, NetworkPlayer>();
    public static ulong LocalCLientId = ulong.MaxValue;
    
    NetworkObject networkObject;
    //clientId, carID, readyStatus
    NetworkVariable<ulong> playerId = new NetworkVariable<ulong>();
    NetworkVariable<int> chosenCar = new NetworkVariable<int>();
    NetworkVariable<bool> playerReady = new NetworkVariable<bool>(false);

    private bool connected = false;

    //NetworkVariable<Vector3> position;

    public static int PlayersInGame
    {
        get { return allPlayers.Count; }
    }
    public ulong PlayerId
    {
        get { return playerId.Value; }
    }
    public int ChosenCar
    {
        get { return chosenCar.Value; }
    }
    public bool PlayerReady
    {
        get { return playerReady.Value; }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        networkObject = GetComponent<NetworkObject>();
        DontDestroyOnLoad(gameObject);

        

        if (IsLocalPlayer)
        {
            RequestPlayerDataServerRpc(PlayerId);
            AddClientServerRpc(NetworkManager.LocalClientId);
            UpdateChosenCarServerRpc(CarCollection.localCarIndex);
            LocalCLientId = NetworkManager.LocalClientId;
        }
    }

    private void OnDestroy()
    {
        allPlayerIds.Remove(PlayerId);
        allPlayerObjects.Remove(this);
        allPlayers.Remove(PlayerId);
    }

    // Update is called once per frame
    void Update()
    {
        if (networkObject != null)
        {
            if (IsLocalPlayer)
            {
                if (CarCollection.localCarIndex != ChosenCar)
                {
                    //chosenCar.Value = CarCollection.localCarIndex;
                    UpdateChosenCarServerRpc(CarCollection.localCarIndex);
                }
                if (LocalCLientId == ulong.MaxValue)
                {
                    LocalCLientId = NetworkManager.LocalClientId;
                }
            }
        }
        else
        {
            Debug.LogWarning($"No networkObject for {gameObject.name}");
        }
    }

    [ServerRpc]
    void UpdateChosenCarServerRpc(int carId)
    {
        chosenCar.Value = carId;
    }

    [ServerRpc]
    public void UpdateReadyStatusServerRpc(bool newValue)
    {
        playerReady.Value = newValue;
    }

    [ServerRpc]
    public void ToggleReadyStatusServerRpc()
    {
        playerReady.Value = playerReady.Value ? false : true;
    }

    [ServerRpc]
    private void AddClientServerRpc(ulong cliendId)
    {
        AddClientClientRpc(cliendId);
    }

    [ClientRpc]
    private void AddClientClientRpc(ulong  cliendId)
    {
        allPlayerIds.Add(cliendId);
        allPlayerObjects.Add(this);
        allPlayers.Add(cliendId, this);
    }


    [ServerRpc]
    private void RemoveClientServerRpc(ulong cliendId)
    {
        RemoveClientClientRpc(cliendId);
    }

    [ClientRpc]
    private void RemoveClientClientRpc(ulong cliendId)
    {
        allPlayerIds.Remove(cliendId);
        allPlayerObjects.Remove(allPlayers[cliendId]);
        allPlayers.Remove(cliendId);
    }

    [ServerRpc]
    private void RequestPlayerDataServerRpc(ulong receiverId)
    {
        //todo have this send playerIds and playerObjects to ProcessPlayerDataClientRpc
        ulong[] requestedIds = allPlayerIds.ToArray();
        ProcessPlayerDataClientRpc(receiverId, requestedIds);
    }

    [ClientRpc]
    private void ProcessPlayerDataClientRpc(ulong receiverId, ulong[] idList)
    {
        //todo have this receive playerIds and playerObjects arrays and turn them in to the appropriate list objects.

        if (receiverId == NetworkManager.Singleton.LocalClientId)
        {
            allPlayerIds = new List<ulong>(idList);
            NetworkPlayer[] tempNetPlayers = Transform.FindObjectsOfType<NetworkPlayer>(true);
            for (int i = 0; i < allPlayerIds.Count; i++)
            {
                for (int e = 0; true; e++)
                {
                    if (e < tempNetPlayers.Length)
                    {
                        allPlayerObjects.Add(null);
                        break;
                    }
                    if (tempNetPlayers[e].PlayerId == allPlayerIds[i])
                    {
                        allPlayerObjects.Add(tempNetPlayers[e]);
                        break;
                    }
                }
            }
        }
    }

    public static NetworkPlayer GetNetworkPlayer(ulong id)
    {
        NetworkPlayer result = null;
        if (allPlayers.TryGetValue(id, out result))
        {
            return result;
        }
        else
        {
            Debug.LogWarning($"id {LocalCLientId} not found");
            return null;
        }
    }

    public static NetworkPlayer GetLocalNetworkPlayer()
    {
        NetworkPlayer result = null;
        if (allPlayers.TryGetValue(LocalCLientId, out result))
        {
            return result;
        }
        else
        {
            Debug.LogWarning($"id {LocalCLientId} not found");
            return null;
        }
    }

}
