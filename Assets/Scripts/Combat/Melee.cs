using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Melee : Weapon
{
    [Header("= Melee =")]

    
    [SerializeField]
    private float attackRate = 0.3f;
    [SerializeField]
    private int hitForce = 30;
    [SerializeField]
    protected JointSettings jointBladeSettings;
    

    Action Rotation;
    Transform weaponHolder;
    TriggerComponent blade;
    FixedJoint2D bladeJoint;

    float hitResetTimer;

    protected override void Awake()
    {
        base.Awake();

        blade = GetComponentInChildren<TriggerComponent>();
    }

    protected override void Update()
    {
        Rotation?.Invoke();

        base.Update();

        hitResetTimer += Time.deltaTime;
    }

    public override void SetOwner(Player player)
    {
        base.SetOwner(player);

        weaponHolder = player.AvailableArm.weaponHolder;

        //Rotation = () => RestRotation = animationHandler.RightWeapon.rotation.eulerAngles.z;
        Rotation = () => RestRotation = weaponHolder.rotation.eulerAngles.z;
    }

        
    public override void BreakJoints()
    {
        base.BreakJoints();

        Rotation = null;
    }

    public override void Attack(Vector2 dir)
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var magnitude = collision.relativeVelocity.magnitude;
        //print(magnitude);
        if (magnitude <= 15)
            return;

        var hp = collision.gameObject.GetComponent<HealthComponent>();

        if (hp && hitResetTimer > attackRate)
        {
            hitResetTimer = 0;

            hp.Value -= damage;

            var body = collision.gameObject.GetComponent<Rigidbody2D>();
            if (body)
            {
                var dir = collision.relativeVelocity * -1;
                body.AddForce(hitForce * dir, ForceMode2D.Impulse);
            }
        }

        // В случае топора, это алгоритм застревания 
        // оружия в теле врага, и не только..
        if (blade.triggered && !bladeJoint && hp)
        {
            // Если оружие находится в руке,
            // то втыкается оно с вероятностью
            if (GetComponent<HingeJoint2D>() && Random.Range(0, 10) > 5)
                return;

            var pointOne = collision.GetContact(0).point;
            var pointTwo = blade.collision.ClosestPoint(blade.transform.position);
            //print(Vector2.Distance(pointOne, pointTwo));
            //var testo1 = new GameObject("Ебало на 0");
            //testo1.transform.parent = collision.gameObject.transform;
            //testo1.transform.position = pointOne;

            //var testo2 = new GameObject("Ебало на 1");
            //testo2.transform.parent = collision.gameObject.transform;
            //testo2.transform.position = pointTwo;

            if (collision.rigidbody)
            {
                if (Vector2.Distance(pointOne, pointTwo) < 0.15f)
                {
                    bladeJoint = gameObject.AddComponent<FixedJoint2D>();
                    bladeJoint.connectedBody = collision.rigidbody;
                    bladeJoint.anchor = jointBladeSettings.anchor;
                    bladeJoint.connectedAnchor = jointBladeSettings.connectedAnchor;

                    hp.GetComponentInParent<StickmanController>().IgnoreCollision(Collider);
                    hp.transform.root.GetComponentInChildren<NoobAreaChecker>().IgnoreTriggers(Collider);

                    var offset = transform.position - jointBladeSettings.hilt.position;
                    transform.position = (Vector3)pointTwo + offset;

                    EventsHolder.onMeleeStucked?.Invoke(this);

                    StartCoroutine(Delay());

                    IEnumerator Delay()
                    {
                        yield return new WaitForSeconds(0.3f);

                        if (bladeJoint)
                            bladeJoint.autoConfigureConnectedAnchor = false;
                    }
                }
            }
        }
    }
}
