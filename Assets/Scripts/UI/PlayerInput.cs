using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] Joystick leftJoystick;
    [SerializeField] TestoBaton baton;

    Player player;

    bool batonDowned;
    bool moveReseted;

    public void Init(Player player)
    {
        this.player = player;

        baton.onDown.AddListener(Baton_Downed);
        baton.onUp.AddListener(Baton_Upped);

        EventsHolder.onLeftControl.AddListener(LeftControl_Clicked);
    }

    private void LeftControl_Clicked()
    {
        Miner.Instance.backMine = !Miner.Instance.backMine;
        EventsHolder.onMineModeSwitch?.Invoke(Miner.Instance.backMine);
    }

    private void Baton_Upped()
    {
        batonDowned = false;
        player.Idle();
    }

    private void Baton_Downed()
    {
        batonDowned = true;
    }

    private void Update()
    {
        if (leftJoystick.Direction != Vector2.zero)
        {
            var joyDir = leftJoystick.Direction;
            var isHorizontal = Mathf.Abs(joyDir.x) - Mathf.Abs(joyDir.y) > 0.5f;

            if (isHorizontal)
            {
                if (leftJoystick.Direction.x > 0)
                {
                    player.Move(Vector2.right);
                }
                else
                    if (leftJoystick.Direction.x < 0)
                {
                    player.Move(Vector2.left);
                }
            }
            else
            {
                if(joyDir.y > 0)
                {
                    player.Move(Vector2.up);
                }
                else
                    if(joyDir.y < 0)
                {
                    player.Move(Vector2.down);
                }
            }

            moveReseted = false;
        }
        else if (!moveReseted)
        {
            moveReseted = true;
            player.Idle();
        }

        if (batonDowned)
        {
            player.Mine();
        }
    }
}
