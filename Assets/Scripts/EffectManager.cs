using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] GameObject[] punchEffectPrefabs;
    [SerializeField] TNT TNTasPunchPrefab;
    [SerializeField] GameObject spawnEffectZombiePrefab;
    [SerializeField] GameObject mineEffectPrefab;

    static EffectManager Inst;

    private void Awake()
    {
        Inst = this;
    }

    public static void PunchEffect(Vector3 pos)
    {
        foreach (var item in Inst.punchEffectPrefabs)
        {
            Instantiate(item, pos, Quaternion.identity);
        }
    }

    public static TNT Punch(Vector3 pos)
    {
        return Instantiate(Inst.TNTasPunchPrefab, pos, Quaternion.identity);
    }

    public static void SpawnEffectZombie(Vector3 pos)
    {
        //Instantiate(Inst.spawnEffectZombiePrefab, pos, Quaternion.identity);
    }

    public static void SpawnMineEffect(Mineable mineable, Vector3 pos, float angle)
    {
        var effect = Instantiate(Inst.mineEffectPrefab, pos, Quaternion.Euler(0, 0, angle));
        var particle = effect.GetComponentInChildren<ParticleSystem>();
        var renderer = particle.GetComponent<ParticleSystemRenderer>();
        var mat = new Material(renderer.material);
        var uv = mineable.tile.sprite.uv;
        var newTexture = new Texture2D(64, 64);

        int startX = (int)(uv[0].x * 1024);
        int endX = (int)(uv[1].x * 1024);
        int startY = (int)(uv[3].y * 1024);
        int endY = (int)(uv[0].y * 1024);

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                var pixel = mineable.tile.sprite.texture.GetPixel(x, y);
                newTexture.SetPixel(x - startX, y - startY, pixel);
                
            }
        }
        newTexture.Apply();
        mat.mainTexture = newTexture;

        renderer.material = mat;
    }
}
