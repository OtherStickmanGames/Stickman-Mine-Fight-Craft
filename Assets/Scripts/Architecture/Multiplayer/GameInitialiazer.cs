using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameInitialiazer : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject housePrefab;
    public GameObject worldManagerPrefab;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            var worldManager = Instantiate(worldManagerPrefab);
            NetworkObject networkObject = worldManager.GetComponent<NetworkObject>();
            networkObject.Spawn();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        GenerateVillageForClient(clientId);
    }

    private void GenerateVillageForClient(ulong clientId)
    {
        List<Vector3> spawnPoints = new List<Vector3>();

        // ѕример генерации деревушки с домами и добавление точек спавна
        for (int i = 0; i < 5; i++) // —оздадим 5 домов
        {
            Vector3 housePosition = new Vector3((int)clientId * 100 + i * 10, 0, 0); // –асположим дома с промежутком в 10 единиц
            Instantiate(housePrefab, housePosition, Quaternion.identity);
            spawnPoints.Add(housePosition + new Vector3(2, 0, 0)); // ƒобавл€ем точку спавна р€дом с каждым домом
        }

        SpawnPlayer(clientId, spawnPoints);
    }

    private void SpawnPlayer(ulong clientId, List<Vector3> spawnPoints)
    {
        Vector3 spawnPosition = GetRandomSpawnPoint(spawnPoints);
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = player.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId);
    }

    private Vector3 GetRandomSpawnPoint(List<Vector3> spawnPoints)
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points available in the village.");
            return Vector3.zero;
        }

        int randomIndex = Random.Range(0, spawnPoints.Count);
        return spawnPoints[randomIndex];
    }
}
