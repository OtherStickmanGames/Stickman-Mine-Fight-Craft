using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Architecture
{
    public class GameManager : MonoBehaviour
    {
        public GameObject playerPrefab;
        public GameObject worldManagerPrefab;
        public GameObject villagePrefab;

        private Dictionary<string, Vector3> playerStartingPositions = new Dictionary<string, Vector3>();
        [SerializeField]
        private Vector3 nextVillagePosition = Vector3.zero;
        private float villageOffset = 20.0f;  // Смещение для каждой новой деревни
        int seedStartPos;

        private void Awake()
        {
            seedStartPos = Random.Range(-300, 300);
        }

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
            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                string clientIdString = clientId.ToString();
                if (!playerStartingPositions.ContainsKey(clientIdString))
                {
                    Vector3 villagePosition = GenerateVillageForNewPlayer();
                    playerStartingPositions[clientIdString] = villagePosition;
                }

                var pos = GetRandomStartPos(clientId);
                //SpawnPlayer(clientId, playerStartingPositions[clientIdString]);
                SpawnPlayer(clientId, pos);
            }
        }

        Vector2 GetRandomStartPos(ulong clientId)
        {
            Random.InitState(seedStartPos + (int)clientId);
            var posX = Random.Range(-300, 300);
            var posY = 1300;
            bool foundY = false;
            while (!foundY)
            {
                var blockId = ProceduralGenerator.GetBlockID(posX, posY, 0);
                if (blockId > 0)
                {
                    foundY = true;
                }
                else
                {
                    posY--;
                }
            }

            return new Vector2(posX, posY + 2);
        }


        private Vector3 GenerateVillageForNewPlayer()
        {
            GameObject village = Instantiate(villagePrefab, nextVillagePosition, Quaternion.identity);
            Vector3 villagePosition = village.transform.position;
            nextVillagePosition += new Vector3(villageOffset, 0, 0);  // Смещение по оси X для каждой новой деревни
            return villagePosition;
        }

        public Vector3 GetStartingPosition(string clientId)
        {
            if (playerStartingPositions.ContainsKey(clientId))
            {
                return playerStartingPositions[clientId];
            }
            return Vector3.zero;
        }


        private void SpawnPlayer(ulong clientId, Vector3 position)
        {
            GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }

        private void OnDestroy()
        {
            if (!NetworkManager.Singleton)
                return;

            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
        }
    }
}
