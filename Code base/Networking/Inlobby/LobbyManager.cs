using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Core;
using System.Collections.Generic;
using Unity.Netcode;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    [Header("UI Components")]
    [SerializeField] private Transform playerListParent;
    [SerializeField] private GameObject playerInfoPrefab;

    private string currentLobbyId;
    private HostGameManager hostGameManager;
    private ClientGameManager clientGameManager;
    private int lengthOfPlayers;
    private Lobby currentLobby;
    public Button StartGameButton;
    private bool isLobbyHost = false;

    private float lobbyPollTimer;
    private bool isGameStarted = false;
    public event EventHandler<LobbyEventArgs> OnLobbyGameModeChanged;

    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    private void Awake()
    {

        StartGameButton.onClick.AddListener(StartGame);
        StartGameButton.gameObject.SetActive(isLobbyHost);

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeGameManagers();
    }



    private async void InitializeGameManagers()
    {
        hostGameManager = HostSingleton.Instance?.GameManager;
        clientGameManager = ClientSingleton.Instance?.GameManager;

        if (hostGameManager != null && !string.IsNullOrEmpty(hostGameManager.lobbyId))
        {
            Debug.Log("HostGameManager found");
            isLobbyHost = true;
            StartGameButton.gameObject.SetActive(isLobbyHost);

            await FetchAndDisplayPlayersAsync(hostGameManager.lobbyId);
        }
        else if (clientGameManager != null && !string.IsNullOrEmpty(clientGameManager.lobbyId))
        {
            Debug.Log("ClientGameManager found");
            await FetchAndDisplayPlayersAsync(clientGameManager.lobbyId);
        }
        else
        {
            Debug.LogError("No valid Host or Client GameManager found.");
        }

    }

    public async Task FetchAndDisplayPlayersAsync(string lobbyId)
    {
        if (string.IsNullOrWhiteSpace(lobbyId))
        {
            Debug.LogError("FetchAndDisplayPlayersAsync received an invalid Lobby ID.");
            return;
        }

        if (playerListParent == null || playerInfoPrefab == null)
        {
            Debug.LogError("UI components are not assigned in the inspector.");
            return;
        }

        currentLobbyId = lobbyId;

        try
        {
            ClearPlayerListUI();

            Lobby lobby = await GetLobbyAsyncMethod(currentLobbyId);
            if (lobby == null) return;
            currentLobby = lobby;
            lengthOfPlayers = lobby.Players.Count;

            UpdateUI(lobby.Players);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error fetching players from lobby {lobbyId}: {e}");
        }
    }

    private void UpdateUI(List<Player> players)
    {
        ClearPlayerListUI();
        foreach (var player in players)
        {
            if (player.Data != null && player.Data.ContainsKey("PlayerData"))
            {
                string playerDataJson = player.Data["PlayerData"].Value;
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(playerDataJson);
                AddPlayerToUI(playerData.userName);
            }
            else
            {
                AddPlayerToUI("Unknown");
            }
        }
    }

    private void ClearPlayerListUI()
    {
        foreach (Transform child in playerListParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void AddPlayerToUI(string playerName)
    {
        GameObject playerInfo = Instantiate(playerInfoPrefab, playerListParent);
        TextMeshProUGUI playerNameText = playerInfo.GetComponentInChildren<TextMeshProUGUI>();

        if (playerNameText != null)
        {
            playerNameText.text = $"{playerName}";
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found on PlayerInfoPrefab.");
        }
    }

    private async Task<Lobby> GetLobbyAsyncMethod(string lobbyId)
    {
        if (string.IsNullOrWhiteSpace(lobbyId))
        {
            Debug.LogError("GetLobbyAsync received an invalid Lobby ID.");
            return null;
        }

        try
        {
            return await Lobbies.Instance.GetLobbyAsync(lobbyId);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to get lobby: {e}");
            return null;
        }
    }
    public async void RefreshPlayerList()
    {
        await FetchAndDisplayPlayersAsync(currentLobbyId);
    }


    public void StartGame()
    {
        if (isLobbyHost && !isGameStarted)
        {
            UpdateLobbyGameStarted(true);
            StartHostGame();
        }
    }
    public async void UpdateLobbyGameStarted(bool isStarted)
    {
        try
        {
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                    { "GameStarted", new DataObject(DataObject.VisibilityOptions.Public, isStarted.ToString()) }
                }
            });

            currentLobby = lobby;
            OnLobbyGameModeChanged?.Invoke(this, new LobbyEventArgs { lobby = currentLobby });
            isGameStarted = true;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to update game started: " + e.Message);
        }
    }
    private async void HandleLobbyPolling()
    {
        if (isGameStarted == true) { return; }

        if (currentLobby == null) return;
        lobbyPollTimer -= Time.deltaTime;
        if (lobbyPollTimer < 0f)
        {
            lobbyPollTimer = 3f;

            try
            {
                if (isGameStarted) { return; }
                Lobby lobby = await GetLobbyAsyncMethod(currentLobby.Id);
                if (lobby == null) return;
                currentLobby = lobby;
                UpdateUI(currentLobby.Players);
                if (isLobbyHost) { return; }

                if (currentLobby.Data.ContainsKey("GameStarted") &&
                    currentLobby.Data["GameStarted"].Value == "True" && isGameStarted == false)
                {
                    StartClientGame();
                    isGameStarted = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error fetching lobby {currentLobby.Id}: {e}");
            }
        }
    }
    private void StartHostGame()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);

    }
    private void StartClientGame()
    {
        clientGameManager.StartGame();

    }
    private void Update()
    {
        HandleLobbyPolling();
    }
}

[System.Serializable]
public class PlayerData
{
    public string userAuthId;
    public string userName;

}