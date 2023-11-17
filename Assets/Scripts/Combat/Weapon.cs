using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody2D body;
    [SerializeField]
    protected new Collider2D collider;
    [SerializeField]
    protected List<SpriteRenderer> sprites;
    [SerializeField]
    public float distanceToAttack = 1.8f;
    [SerializeField]
    protected float damage = 5;
    [SerializeField]
    protected float stepDamage = 1;

    [Header("Joint Setup")]

    [Space]

    [SerializeField]
    protected int sortingOrderThenUsed = 1;
    [SerializeField]
    protected JointSettings jointFirst;
    [SerializeField]
    protected JointSettings jointSecond;
    [SerializeField]
    private float maxForce = 10;
    [SerializeField]
    protected int requaredForceArms = 100;
    
    protected TriggerCallbacks triggerCallbacks;
    protected List<Player> playersTriggered;
    [Space]
    public float RestRotation;// { get; set; }


    public int Damage => (int)damage;
    public int RequaredForceArms => requaredForceArms;
    public Rigidbody2D Body => body;
    public Collider2D Collider => collider;
    public JointSettings JointSecond => jointSecond;
    public JointSettings JointFirst => jointFirst;
    public int? ViewID { get; set; }

    Coroutine triggerExit;
    int lookDirection = 0;

    protected Coroutine changeForce;
    protected Action<bool> colliderOnTaked;
    protected Player owner;
    protected float forceRotation = 0;
    protected bool isTaked;
    

    public bool IsTaked
    {
        get => isTaked;
        set
        {
            isTaked = value;
            if (value)
            {
                body.bodyType = RigidbodyType2D.Dynamic;
                colliderOnTaked(value);
                //collider.enabled = value;
                EventsHolder.weaponTriggerExit?.Invoke(null, this);
                sprites.ForEach(s => s.sortingOrder = sortingOrderThenUsed);
                changeForce = StartCoroutine(ChangeForce());
            }
            else
            {
                StopCoroutine(changeForce);
            }
        }
    }

    protected virtual void Awake()
    {
        playersTriggered = new List<Player>();

        triggerCallbacks = GetComponentInChildren<TriggerCallbacks>();

        if(collider is CompositeCollider2D)
        {
            colliderOnTaked = (e) => 
            {
                var maskNotCollision = LayerMask.NameToLayer("Idle Weapon");
                var maskDefault = LayerMask.NameToLayer("Default");
                gameObject.layer = e ? maskDefault : maskNotCollision;
            };
        }
        else
        {
            colliderOnTaked = (e) => collider.enabled = e;
        }

        //colliderOnTaked(false);
    }

    protected virtual void Start()
    {
        triggerCallbacks.onTriggered += Trigger_Entered;
        triggerCallbacks.deTriggered += Trigger_Exiting;
        triggerCallbacks.triggerStay += Trigger_Staying;

        
    }

    protected virtual void Update()
    {
        if (isTaked && !owner.Ragdoll.IsStuned && !owner.Ragdoll.IsDestroyed)
        {
            //if (Mathf.Abs(RestRotation) > 360)
            //    RestRotation -= 360;

            body.MoveRotation(Mathf.LerpAngle(body.rotation, RestRotation, forceRotation * Time.deltaTime));
        }

    }

    public virtual void Attack(Vector2 dir)
    {
        SetRestRotationByDir(dir);
    }

    public virtual void SetRestRotationByDir(Vector2 dir)
    {
        dir.y *= -1f;
        float angleTarget = Vector2.SignedAngle(dir, Vector2.up);

        //float angleCurrent = transform.rotation.eulerAngles.y;
        //float angle = Mathf.LerpAngle(angleCurrent, angleTarget, .3f);

        RestRotation = angleTarget - 90;
    }

    public virtual void Idle()
    {

    }

    public virtual void Rotate(int dir)
    {
        if (dir != lookDirection)
        {
            lookDirection = dir;

            if(owner.WeaponSet != WeaponSet.LongRangeTwo)
                RestRotation = 180 - RestRotation;

            var scale = transform.localScale;
            scale.y *= dir * Mathf.Sign(scale.y);// TO DO После удаления тестовой пухи
            transform.localScale = scale;

        }
    }

    protected virtual void Trigger_Entered(Collider2D collider)
    {
        var player = collider.transform.root.GetComponent<Player>();
        if (player && !isTaked)
        {
            if(!playersTriggered.Find(p => p == player))
                playersTriggered.Add(player);
            EventsHolder.weaponTriggered?.Invoke(player, this);
        }
    }

    protected virtual void Trigger_Exiting(Collider2D collider)
    {
        var player = collider.transform.root.GetComponent<Player>();
        if (player && !isTaked)
        {
            playersTriggered.Remove(player);

            if (triggerExit == null)
                triggerExit = StartCoroutine(CheckPlayer());

            // Из-за того что стикмен состоит из множества частей и они все время
            // двигаются, то эти методы вызываются беспорядочно, 
            // поэтому введена проверка со временем
            IEnumerator CheckPlayer()
            {
                yield return new WaitForSeconds(.3f);

                if(!playersTriggered.Find(p => p == player))
                    EventsHolder.weaponTriggerExit?.Invoke(player, this);

                triggerExit = null;
            }
        }
    }

    protected virtual void Trigger_Staying(Collider2D collider)
    {
        var player = collider.transform.root.GetComponent<Player>();
        if (player)
        {
           
        }
    }

    public virtual void SetOwner(Player player)
    {
        CreateJoint(player.AvailableArm.arm, player.AvailableArm.hand, jointFirst);

        IsTaked = true;

        if (jointSecond.hilt)
        {
            player.Handler.MoveLeftArmToHilt(this, player.Ragdoll.LeftHand, createSecondJoint);

            void createSecondJoint() => CreateJoint(player.Ragdoll.LeftArm, player.Ragdoll.LeftHand, jointSecond);
        }

        // Проверка является ли данное оружие вторым, и если да,
        // то ему нужны другие слои отображения
        if(player.AvailableArm.weaponHolder == player.Handler.LeftWeapon)
        {
            sprites.ForEach(s => s.sortingOrder = -2);
            transform.position += new Vector3(0, 0, 1);

            Physics2D.IgnoreCollision(Collider, player.Weapons.First().Collider);
        }

        owner = player;
    }

    public virtual void BreakJoints()
    {
        GetComponents<HingeJoint2D>().ToList().ForEach(j => Destroy(j));

        IsTaked = false;

        AddForceForThrow();
    }

    protected virtual void AddForceForThrow()
    {
        body.AddForceAtPosition(5 * body.mass * Vector2.up, transform.position + new Vector3(0.5f, 0), ForceMode2D.Impulse);
    }

    protected virtual void CreateJoint(Rigidbody2D arm, Transform hand, JointSettings jointSettings)
    {
        var offset = transform.position - jointSettings.hilt.position;
        var joint = gameObject.AddComponent<HingeJoint2D>();

        joint.connectedBody = arm;
        joint.anchor = jointSettings.anchor;
        joint.connectedAnchor = jointSettings.connectedAnchor;

        MoveToHandPosition(hand, offset);

        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.3f);

            if (joint)
                joint.autoConfigureConnectedAnchor = false;
        }
    }

    protected virtual void MoveToHandPosition(Transform hand, Vector3 offset)
    {
        transform.position = hand.position + offset;
    }

    /// <summary>
    /// Чтобы оружие плавно вращалось в руке
    /// </summary>
    /// <returns></returns>
    protected IEnumerator ChangeForce()
    {
        float velocity = 0;
        forceRotation = 0;

        while (!Mathf.Approximately(maxForce, forceRotation * 1.03f))
        {
            yield return null;

            forceRotation = Mathf.SmoothDamp(forceRotation, maxForce, ref velocity, 10 * Time.deltaTime);
        }

        changeForce = null;
    }

    public void Upgrade()
    {
        damage += stepDamage;
    }

    [Serializable]
    public struct JointSettings
    {
        public Vector2 anchor;
        public Vector2 connectedAnchor;
        public Transform hilt;
    }
}
