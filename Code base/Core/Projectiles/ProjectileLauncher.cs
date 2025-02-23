using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private AudioSource bulletFireSound;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;

    public LookAt lookAtInstance;

    private bool isFiring = false;
    private float previousFireTime;
    private float muzzleFlashTimer;


    private void Update()
    {
        isFiring = lookAtInstance.FiringCheck();
        if (muzzleFlashTimer > 0f)
        {
            muzzleFlashTimer -= Time.deltaTime;

            if (muzzleFlashTimer <= 0f)
            {
                muzzleFlash.SetActive(false);
            }
        }

        if (!IsOwner) return;

        if (playerCollider == null || !playerCollider.gameObject.activeInHierarchy)
        {
            Debug.Log("Player inactive or destroyed - stopping fire.");
            ForceStopFiring();
            return;
        }

        if (!isFiring)
        {
            StopFiring();
            return;
        }
        if (Time.time - previousFireTime < 1 / fireRate) return;

        FireProjectile();
        previousFireTime = Time.time;
    }

    private void HandlePrimaryFire(bool isFiring)
    {
        Debug.Log($"HandlePrimaryFire called: {isFiring}");
        this.isFiring = isFiring;

        if (!isFiring)
        {
            StopFiring();
        }
    }

    private void FireProjectile()
    {
        Vector3 spawnPos = projectileSpawnPoint.position;
        Vector3 direction = projectileSpawnPoint.up;

        PrimaryFireServerRpc(spawnPos, direction);

        SpawnDummyProjectile(spawnPos, direction);
    }

    private void StopFiring()
    {
        isFiring = false;
        muzzleFlash.SetActive(false);
    }

    public void ForceStopFiring()
    {
        Debug.Log("Force stopping fire.");
        isFiring = false;
        muzzleFlash.SetActive(false);
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        GameObject projectileInstance = Instantiate(serverProjectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = direction;

        if (projectileInstance.TryGetComponent<Collider2D>(out Collider2D projectileCollider))
        {
            Physics2D.IgnoreCollision(playerCollider, projectileCollider);
        }

        if (projectileInstance.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDamage))
        {
            dealDamage.SetOwner(OwnerClientId);
        }

        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }

        SpawnDummyProjectileClientRpc(spawnPos, direction);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (IsOwner) return;

        SpawnDummyProjectile(spawnPos, direction);
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;

        GameObject projectileInstance = Instantiate(clientProjectilePrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = direction;
        bulletFireSound.Play();

        if (projectileInstance.TryGetComponent<Collider2D>(out Collider2D projectileCollider))
        {
            Physics2D.IgnoreCollision(playerCollider, projectileCollider);
        }

        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }
    }
}
