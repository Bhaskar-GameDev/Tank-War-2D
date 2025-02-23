using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static SpawnPoint Instance { get; private set; }

    public List<Vector3> spawnPoints = new List<Vector3>();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  
        }
    }

    public void AddSpawnPoint(Vector3 newSpawnPoint)
    {
        spawnPoints.Add(newSpawnPoint);
    }

    public Vector3 GetRandomSpawnPos()
    {
        if (spawnPoints.Count == 0)
        {
            return Vector3.zero;
        }

        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }
}
