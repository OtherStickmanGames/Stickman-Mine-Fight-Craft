using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioPrefab;

    public AudioClip[] screems;
    public AudioClip[] explosions;
    public AudioClip[] rockStrickes;
    public AudioClip[] popadanieVSkaly;
    public AudioClip[] noobSpawn;
    public AudioClip[] noobVShoke;
    public AudioClip[] zombies;

    public static AudioManager Instance;

    static bool mute;
    public static bool Mute 
    {
        get => mute;
        set
        {
            if (!value)
            {
                var sounds = FindObjectsOfType<AudioSource>();
                foreach (var item in sounds)
                {
                    item.Stop();
                }
            }

            mute = value;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public void Screem(Transform parent)
    {
        if (Mute)
            return;

        var clip = screems[Random.Range(0, screems.Length)];
        var audio = Instantiate(audioPrefab, parent);
        audio.pitch = 1.1f;
        //audio.volume = 0.5f;
        audio.clip = clip;
        audio.Play();
    }

    public void Explosion(Transform source)
    {
        if (Mute)
            return;

        var clip = explosions[Random.Range(0, explosions.Length)];
        AudioSource.PlayClipAtPoint(clip, source.position);
    }

    public void RockStrinke(Transform source)
    {
        if (Mute)
            return;

        var clip = rockStrickes[Random.Range(0, rockStrickes.Length)];
        AudioSource.PlayClipAtPoint(clip, source.position);

        clip = popadanieVSkaly[Random.Range(0, popadanieVSkaly.Length)];
        AudioSource.PlayClipAtPoint(clip, source.position);
    }

    public void NoobSpawn(Transform parent)
    {
        print(mute);
        if (Mute)
            return;

        var audio = Instantiate(audioPrefab, parent);
        audio.pitch = 1.3f;
        audio.clip = noobSpawn[Random.Range(0, noobSpawn.Length)];
        audio.Play();
    }

    public void ZombieSpawn(Transform parent)
    {
        if (Mute)
            return;

        var audio = Instantiate(audioPrefab, parent);
        audio.pitch = 1.1f;
        audio.volume = 0.5f;
        audio.clip = zombies[Random.Range(0, zombies.Length)];
        audio.Play();
    }

    internal void NoobVShoke(Transform parent)
    {
        if (Mute)
            return;

        var audio = Instantiate(audioPrefab, parent);
        audio.pitch = Random.Range(0.7f, 1.3f);
        audio.spatialBlend = 0;
        audio.clip = noobVShoke[0];
        audio.Play();
    }
}
