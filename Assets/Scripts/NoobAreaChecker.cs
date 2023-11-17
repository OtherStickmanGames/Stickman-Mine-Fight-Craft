using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoobAreaChecker : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] TriggerComponent leftLow;
    [SerializeField] TriggerComponent leftTop;
    [SerializeField] TriggerComponent rightLow;
    [SerializeField] TriggerComponent rightTop;

    private void Awake()
    {
        EventsHolder.weaponPicked.AddListener(Weapon_Picked);
    }

    public void IgnoreTriggers(Collider2D collider)
    {
        Physics2D.IgnoreCollision(collider, leftLow.MineCollider);
        Physics2D.IgnoreCollision(collider, leftTop.MineCollider);
        Physics2D.IgnoreCollision(collider, rightLow.MineCollider);
        Physics2D.IgnoreCollision(collider, rightTop.MineCollider);
    }

    private void Weapon_Picked(Player player, Weapon weapon)
    {
        if(this.player == player)
        {
            SetIgnores(leftLow);
            SetIgnores(rightLow);

            void SetIgnores(TriggerComponent trigger)
            {
                Physics2D.IgnoreCollision(weapon.Collider, trigger.MineCollider);
                foreach (var col in weapon.GetComponentsInChildren<Collider2D>())
                {
                    Physics2D.IgnoreCollision(col, trigger.MineCollider);
                }
            }
        }
    }

    private void Start()
    {
        player.Ragdoll.IgnoreCollision(leftLow.MineCollider);
        player.Ragdoll.IgnoreCollision(rightLow.MineCollider);
    }

    private void Update()
    {
        if (player.PlayerState != PlayerState.Walk)
            return;

        LowCheck(leftLow, player.Dir < 0);
        LowCheck(rightLow, player.Dir > 0);
    }

    void LowCheck(TriggerComponent triggerComponent, bool condition)
    {
        if (triggerComponent.triggered)
        {
            //print(triggerComponent.collision);
            if (condition)
            {
                var body = triggerComponent.collision?.GetComponent<Rigidbody2D>();
                if (!body)
                {
                    var bodyes = triggerComponent.collision.GetComponentsInParent<Rigidbody2D>();
                    //print(bodyes.Length);
                    if (bodyes.Length > 0)
                    {
                        Physics2D.IgnoreCollision(triggerComponent.collision, triggerComponent.MineCollider);
                        body = bodyes[0];
                    }
                }

                //if (body)
                //{
                //    var weapon = body.GetComponent<Weapon>();
                //    if (weapon && weapon.IsTaked)
                //        body = null;
                //}
                if (body)
                {
                    if (triggerComponent.triggeredDuration > 1.8f)
                    {
                        //print($"хуйня какая-то валяется {triggerComponent.collision} - {triggerComponent.triggeredDuration}");
                        player.BentDown(body.GetComponent<Collider2D>());
                    }
                }
                
            }
        }
    }
}
