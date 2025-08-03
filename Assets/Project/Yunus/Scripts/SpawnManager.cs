using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    [Header("Spawn Settings")]
    public Transform[] spawnPoints;

    private static int currentSpawnIndex = 0;

    void Start()
    {
        // Sahne �zerindeki spawn noktalar�n� bul
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

        Debug.Log($"Bulunan spawn noktas� say�s�: {spawnPoints.Length}");
    }

    public Vector3 GetNextSpawnPosition()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("Spawn point bulunamad�! Varsay�lan pozisyon kullan�l�yor.");
            return Vector3.zero;
        }

        Vector3 spawnPos = spawnPoints[currentSpawnIndex].position;
        currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Length;

        return spawnPos;
    }
}