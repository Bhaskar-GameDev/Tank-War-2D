using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class TimerManager : NetworkBehaviour
{
    public static TimerManager Instance { get; private set; }
    [SerializeField] private GameObject GOPSpawner;
    [SerializeField] private float gameTime = 300f;
    private float timer;
    public TMP_Text timerText;
    public event Action<bool> OnGameEnd;

    public List<TankPlayerData> AllTankPlayerData = new List<TankPlayerData>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            timer = gameTime;
            UpdateTimerDisplayOnClientsClientRpc(timer);
            OnGameEnd?.Invoke(false);
        }
        GOPSpawner.SetActive(false);
    }

    private void Update()
    {
        if (!IsServer) return;

        if (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimerDisplayOnClientsClientRpc(timer);

            if (timer <= 0)
            {
                EndGameServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndGameServerRpc()
    {

        EndGameClientRpc();
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        GetAllTankPlayersDataServerRpc();
        Time.timeScale = 0f;
        Debug.Log("Game Over!");
        OnGameEnd?.Invoke(true);
        GOPSpawner.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetAllTankPlayersDataServerRpc()
    {
        List<TankPlayerData> tankPlayersList = new List<TankPlayerData>();
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            TankPlayer tankPlayer = client.PlayerObject?.GetComponent<TankPlayer>();
            if (tankPlayer != null)
            {
                TankPlayerData td = new TankPlayerData(tankPlayer.PlayerName.Value.ToString(), tankPlayer.Kills.Value);
                tankPlayersList.Add(td);
            }
        }
        GetAllTankPlayersDataClientRpc(tankPlayersList.ToArray());
    }

    [ClientRpc]
    private void GetAllTankPlayersDataClientRpc(TankPlayerData[] tankPlayers)
    {
        AllTankPlayerData = tankPlayers.ToList();
    }


    public void ReturnToMainMenu()
    {


        SceneManager.LoadScene("Menu");

    }

    [ClientRpc]
    private void UpdateTimerDisplayOnClientsClientRpc(float remainingTime)
    {
        timerText.text = $"Time Left: {Mathf.Ceil(remainingTime)}s";
    }


}
