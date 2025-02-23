using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;

public class PlayerGameOverUI : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerKillEntryPrefab;
    [SerializeField] private Transform gameOverPanel;
    public TimerManager timerManager;

    private HostGameManager hostGameManager;
    private ClientGameManager clientGameManager;
    private List<TankPlayerData> allTankPlayers = new List<TankPlayerData>();

    private void Start()
    {
        Debug.Log("PlayerGameOverUI Start called");

        if (!NetworkManager.Singleton.IsClient)
        {
            Debug.LogError("This script should only run on a client.");
            return;
        }

        if (NetworkManager.Singleton.LocalClient?.PlayerObject == null)
        {
            Debug.LogError("Local player object is null!");
            return;
        }

        TankPlayer tankPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<TankPlayer>();
        if (tankPlayer == null)
        {
            Debug.LogError("TankPlayer component not found!");
            return;
        }

        allTankPlayers = timerManager.AllTankPlayerData;
        PopulateGameOverPanel();
    }

    private void PopulateGameOverPanel()
    {
        Debug.Log("Populating GameOver panel...");

        foreach (Transform child in gameOverPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (TankPlayerData tk in allTankPlayers)
        {
            GameObject playerKillEntry = Instantiate(playerKillEntryPrefab, gameOverPanel);
            TextMeshProUGUI[] texts = playerKillEntry.GetComponentsInChildren<TextMeshProUGUI>();

            string playerName = tk.PName;
            int playerKills = tk.PKills;

            texts[0].text = playerName;
            texts[1].text = $"Assassinations: {playerKills}";

            Debug.Log($"Added entry: {playerName} - {playerKills} kills");
        }
    }

}
