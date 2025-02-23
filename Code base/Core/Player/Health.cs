using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public static Health Instance { get; private set; }
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;

    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    private bool isDead;

    public Action<Health> OnDie;

    private ulong lastDamagerClientId;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        CurrentHealth.Value = MaxHealth;
    }

    public void TakeDamage(int damageValue, ulong damagerClientId)
    {
        lastDamagerClientId = damagerClientId;
        ModifyHealth(-damageValue);
    }

    public void RestoreHealth(int healValue)
    {
        ModifyHealth(healValue);
    }

    private void ModifyHealth(int value)
    {
        if (isDead) return;

        int newHealth = CurrentHealth.Value + value;
        CurrentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);

        if (CurrentHealth.Value == 0)
        {
            HandleDeath();
            isDead = true;
            OnDie?.Invoke(this);
        }
    }

    private void HandleDeath()
    {
        if (!IsServer) return;

        Debug.Log("Health reached zero. Checking killer...");
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(lastDamagerClientId, out var client))
        {
            if (client.PlayerObject.TryGetComponent<TankPlayer>(out TankPlayer ownerPlayer))
            {
                Debug.Log($"Kill awarded to {ownerPlayer.PlayerName.Value}");
                ownerPlayer.Kills.Value++;
            }
            else
            {
                Debug.LogError("Owner player not found!");
            }
        }
        else
        {
            Debug.LogError("Owner client not found!");
        }
    }
}
