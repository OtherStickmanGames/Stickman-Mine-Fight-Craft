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
        new Rigidbody2D rigidbody2D;
        private GameManager gameManager;

        public override void OnNetworkSpawn()
        {
            gameManager = FindObjectOfType<GameManager>();
            rigidbody2D = GetComponent<Rigidbody2D>();

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

        private void LateUpdate()
        {
            
        }

        private void HandleMovement()
        {
            float moveSpeed = 15f;
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical") * 3;
            //print(moveHorizontal);
            var movement = moveSpeed * Time.deltaTime * new Vector3(moveHorizontal, moveVertical, 0);
            //transform.Translate(movement);
            //rigidbody2D.MovePosition(transform.position + movement);
            rigidbody2D.AddForce(movement, ForceMode2D.Impulse);
        }

        private void CheckPositionChange()
        {
            Vector3 currentPosition = transform.position;
            if (currentPosition != lastPosition)
            {
                lastPosition = currentPosition;
                OnPositionChanged?.Invoke(currentPosition);
                //Debug.Log($"Position changed: {currentPosition}");
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
