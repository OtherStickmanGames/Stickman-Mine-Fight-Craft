using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    Player player;
    HealthComponent hp;

    private void Start()
    {
        player = GetComponent<Player>();
        hp = GetComponent<HealthComponent>();

        //hp.valueChanged += HP_Changed;

        EventsHolder.playerSpawnedMine?.Invoke(player);

        EventsHolder.leftJoystickMoved.AddListener(Move);
        EventsHolder.rightJoystickMoved.AddListener(Attack);
        EventsHolder.rightJoystickUp.AddListener(RightJoystick_Uped);
        EventsHolder.jumpClicked.AddListener(() => player.Jump());
        EventsHolder.onLeftJoystickUp.AddListener(() => player.Idle());
        EventsHolder.onPunchClicked.AddListener(Punch);
#if UNITY_WEBGL
        YG.YandexGame.GetDataEvent += LoadParameters;
#endif
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return null;

            LoadParameters();
        }
    }

    private void HP_Changed(HealthComponent _)
    {
        if (hp.Value <= 0)
        {
            EventsHolder.onMissionDefeat?.Invoke();
            player.Ragdoll.IsDestroyed = true;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            EventsHolder.leftJoystickMoved?.Invoke(new Vector2(1, 0));
        }

        if (Input.GetKey(KeyCode.A))
        {
            EventsHolder.leftJoystickMoved?.Invoke(new Vector2(-1, 0));
        }

        if (Input.GetKey(KeyCode.W))
        {
            EventsHolder.leftJoystickMoved?.Invoke(new Vector2(0, 1));
        }

        if (Input.GetKey(KeyCode.S))
        {
            EventsHolder.leftJoystickMoved?.Invoke(new Vector2(0, -1));
        }

        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A))
        {
            player.Idle();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Punch();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            EventsHolder.jumpClicked?.Invoke();
        }

        if (Input.GetKey(KeyCode.Q))
        {
            player.Mine();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            //player.Mine();
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            player.Idle();
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            EventsHolder.onBtnNextLayer?.Invoke();
        }

        if (Input.GetKeyUp(KeyCode.T))
        {
            EventsHolder.onBtnPrevLayer?.Invoke();
        }
    }


    void Punch()
    {
        player.Attack();
        player.ImmediateThrowItems();

        Vector2 origin = player.Hip.position;

        var lands = Physics2D.CircleCastAll(origin, 0.1f, Vector2.zero);
        foreach (var item in lands)
        {
            if (item.collider.GetComponentInParent<Chunck>())
            {
                EffectManager.Punch(origin);
                return;
            }
        }
    }

    public void Move(Vector2 dir)
    {
        player.Move(dir);
    }

    private void Attack(Vector2 dir)
    {
        player.Attack(dir);
    }

    public void RightJoystick_Uped()
    {
        player.Idle();
    }

    void LoadParameters()
    {
    //    print(User.Data.currentHP);
    //    hp.Init(User.Data.maxHP);
    //    if (GameManager.Wave > 1)
    //    {
    //        hp.Value = User.Data.currentHP;
    //    }

    //    player.critPower = User.Data.critPower;
    //    player.punchPower = User.Data.punchPower;
    //    player.Ragdoll.stunThresold = User.Data.stunThresold;
    //    player.GetComponent<AnimationEvents>().jumpPower = User.Data.jumpPower;
    }

    void SaveParameters()
    {
        if (GameManager.Wave > 1)
        {
            //User.Data.currentHP = (int)hp.Value;
            //User.Data.ConvertToYG();
        }
    }

    private void OnDestroy()
    {
        SaveParameters();
    }

}

public enum JumpSet
{
    Up,
    Forward,
    Back
}
