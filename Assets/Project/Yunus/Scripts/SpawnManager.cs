using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    [Header("Spawn Settings")]
    public Transform[] spawnPoints;

    private static int currentSpawnIndex = 0;

    void Start()
    {
        // Sahne üzerindeki spawn noktalarýný bul
        FindSpawnPoints();
    }

    void FindSpawnPoints()
    {
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        spawnPoints = new Transform[spawnPointObjects.Length];

        for (int i = 0; i < spawnPointObjects.Length; i++)
        {
            spawnPoints[i] = spawnPointObjects[i].transform;
        }

        Debug.Log($"Bulunan spawn noktasý sayýsý: {spawnPoints.Length}");
    }

    public Vector3 GetNextSpawnPosition()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("Spawn point bulunamadý! Varsayýlan pozisyon kullanýlýyor.");
            return Vector3.zero;
        }

        Vector3 spawnPos = spawnPoints[currentSpawnIndex].position;
        currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Length;

        return spawnPos;
    }
}