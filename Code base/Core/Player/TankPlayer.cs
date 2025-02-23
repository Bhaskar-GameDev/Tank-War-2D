using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Cinemachine.CinemachineVirtualCamera virtualCamera;
    [field: SerializeField] public Health Health { get; private set; }

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<int> Kills = new NetworkVariable<int>();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;

    private List<ulong> ClientIds = new List<ulong>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.userName;
            Kills.Value = 0;

            OnPlayerSpawned?.Invoke(this);
        }

        if (IsOwner)
        {
            virtualCamera.Priority = ownerPriority;
            Kills.OnValueChanged += (oldValue, newValue) =>
            {
                Debug.Log($"Kill count updated to {newValue}");
                AddKillToServer_ServerRpc(OwnerClientId);
            };
        }

        RequestClientIdsServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestClientIdsServerRpc()
    {
        List<ulong> serverClientIds = HostSingleton.Instance.GameManager.NetworkServer.GetConnectedClientIds();
        UpdateClientIdsClientRpc(serverClientIds.ToArray());
    }

    [ClientRpc]
    private void UpdateClientIdsClientRpc(ulong[] clientIds)
    {
        Debug.Log("Updating client IDs...");
        ClientIds = clientIds.ToList();
        Debug.Log($"Client IDs updated: {string.Join(", ", ClientIds)}");
    }

    public List<ulong> GetPlayerIds()
    {
        return ClientIds;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddKillToServer_ServerRpc(ulong clientId)
    {
        if (!HostSingleton.Instance.GameManager.NetworkServer.PlayerKills.ContainsKey(clientId))
        {
            HostSingleton.Instance.GameManager.NetworkServer.PlayerKills[clientId] = 0;
        }
        HostSingleton.Instance.GameManager.NetworkServer.PlayerKills[clientId] = Kills.Value;
    }

    public string GetPlayerNameById(ulong clientId)
    {
        Debug.Log($"Getting player name for Client ID: {clientId}");
        return HostSingleton.Instance.GameManager.NetworkServer.GetPlayerNameById(clientId);
    }

    public int GetPlayerKillsById()
    {
        Debug.Log($"Getting kills for player {OwnerClientId}: {Kills.Value}");
        return Kills.Value;
    }
}