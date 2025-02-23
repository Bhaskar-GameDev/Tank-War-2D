using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbiesList : MonoBehaviour
{
    public static LobbiesList Instance { get; private set; }
    [SerializeField] private Transform lobbyItemParent;
    [SerializeField] private LobbyItem lobbyItemPrefab;


    private bool isJoining;
    private bool isRefreshing;
    private Lobby joinedLobby;
    private void OnEnable()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        if (isRefreshing) { return; }

        isRefreshing = true;

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"),
                new QueryFilter(
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0")
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

            foreach(Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }

            foreach(Lobby lobby in lobbies.Results)
            {
                LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyItem.Initialise(this, lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        isRefreshing = false;
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (isJoining) { return; }

        isJoining = true;
        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        try
        {
            JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions();
            joinOptions.Player = new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {
                        "PlayerData", new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Member,
                            value: Encoding.UTF8.GetString(payloadBytes))
                    }
                }
            };
            Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id,joinOptions);
            string joinCode = joiningLobby.Data["JoinCode"].Value;
            


            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode,lobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        isJoining = false;
    }
}
