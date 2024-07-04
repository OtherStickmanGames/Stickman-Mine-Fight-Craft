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
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Vector2Int blockPosition = GetBlockPositionFromMouse();
            int layerIndex = 0; // Пример, используйте правильный индекс слоя
            blockManager.SetBlock(layerIndex, mousePosition, 0);
        }

        if (IsOwner && Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int layerIndex = 0; // Пример, используйте правильный индекс слоя
            blockManager.SetBlock(layerIndex, mousePosition, 2);
        }
    }

    private Vector2Int GetBlockPositionFromMouse()
    {
        int chunkSize = WorldManager.Instance.chunkSize;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int chunckPosition = new(Mathf.FloorToInt(mousePosition.x / chunkSize), Mathf.FloorToInt(mousePosition.y / chunkSize));
        chunckPosition *= chunkSize;
        Vector2Int blockPosition = new(Mathf.FloorToInt(mousePosition.x - chunckPosition.x), Mathf.FloorToInt(mousePosition.y - chunckPosition.y));
        //print($"{chunckPosition} -=-=- {blockPosition}");
        return blockPosition;
    }
}
