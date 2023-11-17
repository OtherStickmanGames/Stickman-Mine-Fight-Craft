using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Устойчивость
// Сила прыжка
// Исцеление
// Плюс хп
// Сила удара
// Сила Крит удара
// Шанс крит удара
// +TNT

public class GameManager : MonoBehaviour
{
    public bool startSimulated = false;
    [SerializeField]
    bool ebalaTesto;
    [SerializeField] public List<PerkData> perks;

    public static bool DESTROY_BULLET;

    [SerializeField]
    Vector3 spawnPos;
    [SerializeField]
    private Player playerAIPrefab;

    [SerializeField]
    List<GameObject> spawneds = new();

    [Space]

    public int totalDamage;
    public int countParts;

    public List<Player> allPlayers;
    public int countDestroyed;

    public static GameManager Instance;
    public Player AiPrefab => playerAIPrefab;
    public static int Wave { get; set; } = 1;
    public int CountEnemies => Wave > 15 ? 15 : Wave;


    static float? defaultTimeScale;
    static float? defaultFixedTime;

    private void Awake()
    {
        if(defaultTimeScale == null)
        {
            defaultTimeScale = Time.timeScale;
            defaultFixedTime = Time.fixedDeltaTime;
            //print($"{defaultTimeScale.Value} =======================");
        }

        Instance = this;
#if UNITY_WEBGL
        //User.Data.ConvertToData();
#endif
        EventsHolder.onObjectSpawned.AddListener(Object_Spawned);
        EventsHolder.playerSpawnedAny.AddListener(AnyPlayer_Spawned);
        EventsHolder.onCritPunch.AddListener(Crit_Punched);
        EventsHolder.onStickmanDestroyed.AddListener(Enemy_Destroyed);

        Application.targetFrameRate = 60;
#if UNITY_WEBGL
        YG.YandexGame.GetDataEvent += Data_Loaded;
        YG.YandexGame.CloseFullAdEvent += Resume;
        YG.YandexGame.OpenFullAdEvent += Pause;
#endif

#if UNITY_ANDROID
        LoadData();
#endif
    }

    void LoadData()
    {
        if (PlayerPrefs.HasKey("Data"))
        {
            var str = PlayerPrefs.GetString("Data");
            User.Data = Json.Deserialize<User>(str);
        }
    }

    private void Data_Loaded()
    {
        //User.Data.ConvertToData();
        print("шо за ботва");
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private void Enemy_Destroyed(Player enemy)
    {
        countDestroyed++;
        if(countDestroyed >= CountEnemies)
        {
            EventsHolder.onMissionComplete?.Invoke();
        }
    }

    private void Crit_Punched(Player player)
    {
        Time.timeScale /= 10f; 
        Time.fixedDeltaTime /= 10;

        LeanTween.delayedCall(1f, RevertTimescale);

        void RevertTimescale()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    private void AnyPlayer_Spawned(Player player)
    {
        allPlayers.Add(player);
    }

   
    private void Object_Spawned(GameObject spawned)
    {
        spawneds.Add(spawned);
    }

    private IEnumerator Start()
    {
        //Wave = User.Data.Wave;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        DESTROY_BULLET = ebalaTesto;

        yield return new WaitForSeconds(3f);

        StartCoroutine(Spawner());

    }

    IEnumerator Spawner()
    {
        //print($"{CountEnemies} - Количество врагов");
        int count = CountEnemies;
        for (int i = 0; i < count; i++)
        {
            Spawn();

            yield return new WaitForSeconds(Random.Range(1, 5));
        }
    }

    void Spawn()
    {
        if (!playerAIPrefab)
            return;

        var player = allPlayers.Find(p => p.GetComponent<PlayerBehaviour>());
        var center = player.Hip.position;
        int maxRadiusSpawn = 8;

        var randomDir = Random.insideUnitCircle;
        randomDir.y = Mathf.Abs(randomDir.y);
        Vector3 pos = center + (Vector3)(randomDir * Random.Range(3, maxRadiusSpawn));

        int countTryed = 0;
        //while (Physics2D.OverlapBoxAll(pos, new(2, 3), 0).Length > 0)
        while (Physics2D.CircleCastAll(pos, 3, Vector2.zero).Length > 0)
        {
            randomDir = Random.insideUnitCircle;
            randomDir.y = Mathf.Abs(randomDir.y);
            pos = center + (Vector3)(randomDir * Random.Range(3, maxRadiusSpawn));
            countTryed++;
            if (countTryed > 30)
                maxRadiusSpawn++;
        }

        LeanTween.delayedCall(0.3f, SpawnEnemy);

        void SpawnEnemy()
        {
            var enemy = Instantiate(playerAIPrefab, pos, Quaternion.identity);
            enemy.Ragdoll.startHP = 10 * (1 + (Wave / 8));

            AudioManager.Instance.ZombieSpawn(enemy.Hip);
        }

        EffectManager.SpawnEffectZombie(pos);
    }

    public void Pause()
    {
        print("-=-=-=-=- PAUSE ==-=-=-=-=-=-");
        Time.timeScale = 0;
        Time.fixedDeltaTime = 0;
    }

    public void Resume()
    {
        print($" Resume {defaultTimeScale.Value} -==-=-=-=-=-=-=-");
        Time.timeScale = defaultTimeScale.Value;
        Time.fixedDeltaTime = defaultFixedTime.Value;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.B))
        {
            Spawn();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Resume();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            
        }
#endif
    }

    private void OnDestroy()
    {
#if UNITY_WEBGL
        YG.YandexGame.GetDataEvent -= Data_Loaded;
        YG.YandexGame.CloseFullAdEvent -= Resume;
        YG.YandexGame.OpenFullAdEvent -= Pause;
#endif
    }
}

[Serializable]
public class PerkData
{
    public string title;
    [TextArea(5, 8)]
    public string description;
    public int id;
    public Sprite icon;
    public Color backColor;
}
