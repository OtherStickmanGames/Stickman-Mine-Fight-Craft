using UnityEngine;
using Unity.Netcode;
using Architecture;

public class PlayerController : NetworkBehaviour
{
    private BlockManager blockManager;

    private void Start()
    {
        blockManager = FindObjectOfType<BlockManager>();
    }

    private void Update()
    {
        if (IsOwner && Input.GetMouseButtonDown(0))
        {
            Vector2Int blockPosition = GetBlockPositionFromMouse();
            int layerIndex = 0; // Пример, используйте правильный индекс слоя
            blockManager.SetBlock(layerIndex, blockPosition, true);
        }

        if (IsOwner && Input.GetMouseButtonDown(1))
        {
            Vector2Int blockPosition = GetBlockPositionFromMouse();
            int layerIndex = 0; // Пример, используйте правильный индекс слоя
            blockManager.SetBlock(layerIndex, blockPosition, false);
        }
    }

    private Vector2Int GetBlockPositionFromMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2Int(Mathf.FloorToInt(mousePosition.x), Mathf.FloorToInt(mousePosition.y));
    }
}
