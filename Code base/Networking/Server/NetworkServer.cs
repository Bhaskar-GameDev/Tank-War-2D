using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;

    public Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();
    public Dictionary<ulong, int> PlayerKills = new Dictionary<ulong, int>();


    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;



        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log($"ApprovalCheck Trying :");
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonConvert.DeserializeObject<UserData>(payload);
        Debug.Log($"ApprovalCheck Trying : {userData.userName}");

        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        authIdToUserData[userData.userAuthId] = userData;
        PlayerKills[request.ClientNetworkId] = 0;

        response.Approved = true;
        response.Position = SpawnPoint.Instance.GetRandomSpawnPos();
        response.Rotation = Quaternion.identity;
        response.CreatePlayerObject = true;
        Debug.Log($"ApprovalCheck Done For:  {userData.userName}");
    }

    private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            clientIdToAuth.Remove(clientId);
            authIdToUserData.Remove(authId);
        }
    }

    public UserData GetUserDataByClientId(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            if (authIdToUserData.TryGetValue(authId, out UserData data))
            {
                return data;
            }

            return null;
        }

        return null;
    }

    public string GetPlayerNameById(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            if (authIdToUserData.TryGetValue(authId, out UserData data))
            {
                return data.userName;
            }
            else
            {
                return "Unknown";
            }
        }
        else
        {
            return "Unknown";
        }
    }
    public List<ulong> GetConnectedClientIds()
    {
        return clientIdToAuth.Keys.ToList();
    }




    public void Dispose()
    {
        if (networkManager == null) { return; }

        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        networkManager.OnServerStarted -= OnNetworkReady;

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
}
