using UnityEngine;
using Unity.Netcode;
using Architecture;

public class PlayerController : NetworkBehaviour
{
    private BlockManager blockManager;

    public int Layer { get; set; }


    private void Start()
    {
        blockManager = FindObjectOfType<BlockManager>();

        EventsHolder.onBtnNextLayer.AddListener(LayerNext_Click);
        EventsHolder.onBtnPrevLayer.AddListener(LayerPrev_Click);

        EventsHolder.onPlayerLayerChanged?.Invoke(Layer);

    }

    private void Update()
    {
        if (IsOwner && Input.GetMouseButtonDown(0) && !UI.IsHited())
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Vector2Int blockPosition = GetBlockPositionFromMouse();
            int layerIndex = 0; // Пример, используйте правильный индекс слоя
            blockManager.SetBlock(layerIndex, mousePosition, 0);
        }

        if (IsOwner && Input.GetMouseButtonDown(1) && !UI.IsHited())
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int layerIndex = 0; // Пример, используйте правильный индекс слоя
            blockManager.SetBlock(layerIndex, mousePosition, 2);
        }

        CheckAvailableLayerSwitchs();
    }

    bool availableNextLayer;
    bool availablePrevLayer;
    void CheckAvailableLayerSwitchs()
    {
        availableNextLayer = blockManager.CheckAvailableMoveToLayer(Layer + 1, transform.position);
        availablePrevLayer = blockManager.CheckAvailableMoveToLayer(Layer - 1, transform.position);

        EventsHolder.onAvailableSwitchLayer?.Invoke(availableNextLayer, LayerSwitchDir.Next);
        EventsHolder.onAvailableSwitchLayer?.Invoke(availablePrevLayer, LayerSwitchDir.Prev);
    }

    private void LayerPrev_Click()
    {
        if (availablePrevLayer)
        {
            Layer--;
            CheckValidateLayerValue();
            EventsHolder.onPlayerLayerChanged?.Invoke(Layer);
        }
    }

    private void LayerNext_Click()
    {
        if (availableNextLayer)
        {
            Layer++;
            CheckValidateLayerValue();
            gameObject.layer = LayerMask.NameToLayer($"LAYER_{Layer}");
            blockManager.SwitchLayer(Layer, LayerSwitchDir.Next);
            EventsHolder.onPlayerLayerChanged?.Invoke(Layer);
        }
    }

    void CheckValidateLayerValue()
    {
        Layer = Mathf.Clamp(Layer, 0, WorldManager.Instance.numLayers);
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
