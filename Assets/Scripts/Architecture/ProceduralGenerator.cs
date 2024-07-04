using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BLOCKS;

public class ProceduralGenerator : MonoBehaviour
{
    [SerializeField] public BlockData[] blocksData;
    [SerializeField] float thresoldMain = 0.35f;
    [SerializeField] bool useRandomSeed;
    [Space]
    [SerializeField] float[] globalZoom;
    [SerializeField] float globalCutout = 488;
    [Space]
    public int size = 10;
    public float[] zoom;

    public static ProceduralGenerator Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public static int GetBlockID(int posX, int posY, int layer)
    {
        Random.InitState(888);

        int idBlock = EMPTY;

        var globalZoom = Instance.globalZoom;
        var globalCutout = Instance.globalCutout;
        var zoom = Instance.zoom;
        var thresoldMain = Instance.thresoldMain;

        var globalNoise = Mathf.PerlinNoise(posX / globalZoom[layer], posY / globalZoom[layer]);
        globalNoise /= posY / globalCutout;

        if (globalNoise > 0.5f)
        {
            var noise = Mathf.PerlinNoise(posX / zoom[layer], posY / zoom[layer]);

            if (noise > thresoldMain)
            {
                idBlock = DIRT;

                if (noise > thresoldMain + 0.1f)
                {
                    idBlock = STONE;
                }
                if (idBlock == DIRT)// Проверка на верхний блок земли
                {
                    if (GetBlockID(posX, posY + 1, layer) == 0)
                    {
                        idBlock = GROUND;
                    }
                }
            }
        }

        return idBlock;
    }

    [System.Serializable]
    public class BlockData
    {
        public string name;
        public Sprite sprite;
    }
}
