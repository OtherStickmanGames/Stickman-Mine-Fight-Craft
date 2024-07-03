using Unity.Netcode;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace Architecture
{
    public class Player : NetworkBehaviour
    {
        public static UnityEvent<Player> onOwnerSpawn = new UnityEvent<Player>();
        public event Action<Vector3> OnPositionChanged;

        private Vector3 lastPosition;

        private GameManager gameManager;

        public override void OnNetworkSpawn()
        {
            gameManager = FindObjectOfType<GameManager>();

            if (IsOwner)
            {
                if (ClientIdentifier.HasSavedPosition())
                {
                    Vector3 savedPosition = ClientIdentifier.LoadPlayerPosition();
                    transform.position = savedPosition;
                }
                else
                {
                    //Vector3 startingPosition = gameManager.GetStartingPosition(NetworkManager.Singleton.LocalClientId.ToString());
                    //transform.position = startingPosition;
                    //ClientIdentifier.SavePlayerPosition(startingPosition);
                }

                // Add event listener for position changes
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

                onOwnerSpawn?.Invoke(this);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (IsOwner)
            {
                // Save the player's position when they disconnect
                ClientIdentifier.SavePlayerPosition(transform.position);
            }
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                ClientIdentifier.SavePlayerPosition(transform.position);
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                HandleMovement();
                CheckPositionChange();
            }
        }

        private void HandleMovement()
        {
            float moveSpeed = 5f;
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            //print(moveHorizontal);
            Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0) * moveSpeed * Time.deltaTime;
            transform.Translate(movement);
        }

        private void CheckPositionChange()
        {
            Vector3 currentPosition = transform.position;
            if (currentPosition != lastPosition)
            {
                lastPosition = currentPosition;
                OnPositionChanged?.Invoke(currentPosition);
                Debug.Log($"Position changed: {currentPosition}");
                NotifyServerOfPositionChangeServerRpc(currentPosition);
            }
        }

        [ServerRpc]
        private void NotifyServerOfPositionChangeServerRpc(Vector3 position)
        {
            OnPositionChanged?.Invoke(position);
        }
    }
}
