using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System;
using System.Collections.Generic;

public class PlayerKillUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI killText;

    private TankPlayer player;

    private void Start()
    {
        player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<TankPlayer>();

        if (player != null)
        {
            player.Kills.OnValueChanged += UpdateKillUI;
            UpdateKillUI(player.Kills.Value, player.Kills.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (player != null)
        {
            player.Kills.OnValueChanged -= UpdateKillUI;
        }

        base.OnNetworkDespawn();
    }

    private void UpdateKillUI(int oldKills, int newKills)
    {
        if (!IsOwner) return;
        killText.text = $"Assassinations: {newKills}";
    }
    
}
