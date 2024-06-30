using Unity.Netcode;
using UnityEngine;
using System;

namespace Architecture
{

    public class Player : NetworkBehaviour
    {
        public event Action<Vector3> OnPositionChanged;
        private Vector3 lastPosition;

        private void Update()
        {
            if (IsOwner)
            {
                Vector3 currentPosition = transform.position;
                if (currentPosition != lastPosition)
                {
                    lastPosition = currentPosition;
                    OnPositionChanged?.Invoke(currentPosition);
                }
            }
        }
    }
}
