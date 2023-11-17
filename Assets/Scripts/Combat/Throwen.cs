using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Throwen : Weapon
{
    [Header("=== Throwen ===")]

    [Space]

    [SerializeField]
    public int powerForGotUp = 30;
    [SerializeField]
    public int powerShot = 10;

    public Action<Throwen> captured;
    public Action<Vector2> onAttack;

    Action jointCreated;
    int countJoints;


    public override void Attack(Vector2 dir)
    {
        if (!IsTaked)
            return;

        ThrowAttack(dir);

        isTaked = false;

        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.07f);

            owner.ThrowWeapon(this);
        }

        onAttack?.Invoke(dir);
    }

    public void ThrowAttack(Vector2 dir)
    {
        body.AddForce(powerShot * owner.totalPower * dir, ForceMode2D.Impulse);
    }

    public override void SetOwner(Player player)
    {
        jointCreated += Joint_Created;

        OverrideIsTakedApartFirst();

        player.Handler.MoveRightArmToHilt(this, player.Ragdoll.RightHand, createFirstJoint);

        void createFirstJoint() => CreateJoint(player.Ragdoll.RightArm, player.Ragdoll.RightHand, jointFirst);
        
        player.Handler.MoveLeftArmToHilt(this, player.Ragdoll.LeftHand, createSecondJoint);

        void createSecondJoint() => CreateJoint(player.Ragdoll.LeftArm, player.Ragdoll.LeftHand, jointSecond);

        owner = player;
    }

    protected override void CreateJoint(Rigidbody2D arm, Transform hand, JointSettings jointSettings)
    {
        base.CreateJoint(arm, hand, jointSettings);

        jointCreated?.Invoke();
    }

    private void Joint_Created()
    {
        countJoints++;

        if (countJoints == 2)
        {
            jointCreated -= Joint_Created;

            StartCoroutine(DelayCall());
        }

        IEnumerator DelayCall()
        {
            yield return new WaitForSeconds(0.8f);

            if (owner)
            {
                captured?.Invoke(this);

                OverrrideIsTakedFull();
            }
        }
    }

    void OverrrideIsTakedFull()
    {
        body.bodyType = RigidbodyType2D.Dynamic;
        colliderOnTaked(true);
        changeForce = StartCoroutine(ChangeForce());
    }

    void OverrideIsTakedApartFirst()
    {
        isTaked = true;
        EventsHolder.weaponTriggerExit?.Invoke(null, this);
        sprites.ForEach(s => s.sortingOrder = sortingOrderThenUsed);
    }

    public override void BreakJoints()
    {
        base.BreakJoints();

        countJoints = 0;
        forceRotation = 0;
    }

    public override void SetRestRotationByDir(Vector2 dir)
    {
        
    }

    protected override void AddForceForThrow()
    {

    }

    protected override void MoveToHandPosition(Transform hand, Vector3 offset)
    {
        
    }

    public override void Rotate(int dir)
    {
        
    }
}
