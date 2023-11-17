using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    [Header("===== Параметры инициализации =====")]
    [SerializeField] int startMuscleForce = 88;

    [Space]

    [SerializeField]
    private Transform hip;
    [SerializeField]
    private Transform head;
    [SerializeField]
    private StickmanController ragdoll;
    [SerializeField]
    private AnimationHandler animationHandler;
    [SerializeField]
    private NoobAnimationHandler noobAnimations;
    [SerializeField]
    private NoobAIBehaviour noobAIBehaviour;
    [SerializeField] Team team;

    [Space]

    [SerializeField]
    private SpriteRenderer leftArmTop;

    public int totalPower;
    public bool isLongRangeAttack;

    [Space]

    [SerializeField] Weapon rightWeapon;

    public Transform Hip => hip;
    public Transform Head => head;
    public StickmanController Ragdoll => ragdoll;
    public AnimationHandler Handler => animationHandler;
    public NoobAIBehaviour NoobAIBehaviour => noobAIBehaviour;
    public List<Weapon> Weapons { get; private set; }
    public ArmJointSet AvailableArm { get; private set; }
    public Mineable Mineable { get; private set; }
    public List<ItemJoint> TakedItemsJoints { get; private set; } = new();
    public Vector2 LastDir { get; private set; } = Vector2.right;
    [field: SerializeField]
    public PlayerState PlayerState { get; private set; }
    public Team Team => team;
    public int Dir => animationHandler.DirHorizontal;
    public bool Attacking { get; set; }
    public WeaponSet WeaponSet 
    { 
        get => weaponSet; 
        set 
        { 
            weaponSet = value;
            if (noobAnimations)
                noobAnimations.SetWeaponSet(weaponSet);
        } 
    }

    [SerializeField] [Tooltip("Не для редактирования")]
    WeaponSet weaponSet;
    [SerializeField]
    float groundedRayLength = 1f;
    [SerializeField] public float punchDamagePower = 50f;
    [SerializeField] public LayerMask attackingMask;

    Mineable lastMineable;
    PlayerState previousState;
    [HideInInspector] public float globalRate;
    public int critPower = 15;
    public int punchPower = 2;
    public float mineTimer;

    private void Awake()
    {
        Weapons = new List<Weapon>();

        foreach (var item in GetComponentsInChildren<CollisionHandler>())
        {
            item.Player = this;
        }

        ragdoll.SetMuscleForce(startMuscleForce);
    }

    private void Start()
    {
        EventsHolder.playerSpawnedAny?.Invoke(this);

        //EventsHolder.onPlayerKeepItem.AddListener(Item_Keeped);
        //EventsHolder.onMeleeStucked.AddListener(Melee_Stucked);

        CalcultePower();

        if (rightWeapon)
        {
            TakeWeapon(rightWeapon);
        }
    }

    private void Melee_Stucked(Melee melee)
    {
        var weapon = Weapons.Find(w => w == melee);
        if (weapon)
        {
            LeanTween.delayedCall(5, () => ThrowWeapon(weapon));
        }
    }

    private void Item_Keeped(GameObject item)
    {
        if(Weapons.Count > 0 && item == Weapons[0].gameObject)
        {
            ThrowWeapon(Weapons[0]);
        }
    }

    private void Update() 
    {
        globalRate += Time.deltaTime;

        animationHandler.transform.position = hip.position;         

        CheckJointLifeTime();
    }

    void CheckJointLifeTime()
    {
        foreach (var item in TakedItemsJoints)
        {
            item.lifetime += Time.deltaTime;
            if (item.lifetime > 10)
                Destroy(item.joint);
        }

        TakedItemsJoints.RemoveAll(i => !i.joint);
    }

    public void ImmediateThrowItems()
    {
        foreach (var item in TakedItemsJoints)
        {
            Destroy(item.joint);
        }
        noobAnimations.Play();
        TakedItemsJoints.RemoveAll(i => !i.joint);
    }

    public void Move(Vector2 dir)
    {
        LastDir = dir;

        ragdoll.Move(dir);

        if (PlayerState == PlayerState.Jump || PlayerState == PlayerState.Attack)
            return;

        if(dir.x > 0)
        {
            if (noobAnimations)
                noobAnimations.Move(1);
            else
                animationHandler.Move(dir);
                //animationHandler.SetDirectionView(1);
            Weapons.ForEach(w => w.Rotate(1));
        }
        else if(dir.x < 0)
        {
            if (noobAnimations)
                noobAnimations.Move(-1);
            else
                animationHandler.Move(dir);
                //animationHandler.SetDirectionView(-1);
            Weapons.ForEach(w => w.Rotate(-1));
        }

        PlayerState = PlayerState.Walk;
    }

    public bool Jump()
    {
        bool jumpAvailable = false;
        var hits = Physics2D.RaycastAll(hip.position, Vector2.down, groundedRayLength);
        foreach (var hit in hits)
        {
            var foundBody = ragdoll.muscles.ToList().Find(b => b.bone.gameObject == hit.collider.gameObject)?.bone.gameObject;
            if (!foundBody)
            {
                jumpAvailable = true;
                break;
            }
        }

        if (jumpAvailable)
        {
            JumpSet jumpSet = JumpSet.Up;
            if (PlayerState == PlayerState.Walk)
            {
                jumpSet = JumpSet.Back;
            }
            previousState = PlayerState;
            PlayerState = PlayerState.Jump;
            animationHandler.Jump();
            //noobAnimations.Jump(jumpSet);
            //ragdoll.Jump(totalPower);
        }
        return jumpAvailable;
    }


    public void Attack(Vector2 dir)
    {
        if (ragdoll.IsStuned || ragdoll.IsDestroyed)
            return;

        Weapons.ForEach(w => w.Attack(dir));
        noobAnimations.Attack(dir);

        PlayerState = PlayerState.Attack;
        //if (Weapons.Count == 0)
        //    return;

        //var magnitude = dir.magnitude;

        //if (WeaponSet == WeaponSet.MeleeAndLongRange)
        //{
        //    if (magnitude > Constants.Melee_And_Long_Range_Attack_Range_Thresold
        //     && magnitude < Constants.Melee_And_Long_Range_Attack_Melee_Thresold)
        //    {
        //        isLongRangeAttack = true;
        //        Weapons.ForEach(w => w.Attack(dir));
        //    }
        //    else if(isLongRangeAttack)
        //    {
        //        isLongRangeAttack = false;
        //        // Возможно понадобится делать это только для лонг рендж
        //        Idle();
        //    }
        //}
        //else
        //{
        //    if (magnitude > 0.7f)
        //        Weapons.ForEach(w => w.Attack(dir));
        //}

        //animationHandler.ChooseAnimation(dir);
    }

    public void Attack()
    {
        if (ragdoll.IsStuned || ragdoll.IsDestroyed)
            return;

        if (PlayerState == PlayerState.Attack)
            return;

        Vector2 dirAttack = Dir > 0 ? Vector2.right : Vector2.left;

        Weapons.ForEach(w => w.Attack(dirAttack));
        noobAnimations.Attack(dirAttack);

        PlayerState = PlayerState.Attack;
    }

    public void Turn(Vector2 dir)
    {
        
    }

    public void Idle()
    {
        Weapons.ForEach(w => w.Idle());
        isLongRangeAttack = false;
        Mineable = null;
        ragdoll.mineAngle = 0;

        if (PlayerState == PlayerState.Jump)
        {
            previousState = PlayerState.Idle;
            return;
        }
        
        if (noobAnimations)
        {
            noobAnimations.Idle();
        }
        else
        {
            animationHandler.Idle();
        }
        

        PlayerState = PlayerState.Idle;
    }

    public void TakeWeapon(Weapon weapon)
    {
        CheckWeaponAndArmAvailable(weapon);

        ragdoll.IgnoreCollision(weapon.Collider);
        ragdoll.SetForceToArms(weapon.RequaredForceArms);
        Weapons.Add(weapon);
        weapon.SetOwner(this);
        if (!noobAnimations)
            animationHandler.ChooseWeaponPose(WeaponSet);

        EventsHolder.weaponPicked?.Invoke(this, weapon);
        
        if(weapon as Throwen)
        {
            (weapon as Throwen).captured += ThrowenWeapon_Captured;
        }
    }

    private void ThrowenWeapon_Captured(Throwen throwen)
    {
        if(totalPower > throwen.powerForGotUp)
        {
            animationHandler.GotUp();
            leftArmTop.sortingOrder = -1;
        }
    }

    public void ThrowWeapon(Weapon weapon)
    {
        // Пока хз как быть, когда нуб бросаем топор,
        // который торчит из врага, то он не может к нему
        // подойти к рукопашной, так как топор мешает..
        //LeanTween.delayedCall(0.88f, ignoreCollision);

        void ignoreCollision() => ragdoll.IgnoreCollision(weapon.Collider, false);

        ragdoll.SetForceToArms(Constants.FORCE_ARMS);

        weapon.BreakJoints();

        WeaponSet = WeaponSet.None;

        EventsHolder.weaponThrowed?.Invoke(this, weapon);

        // --- Ебучий костыль ---
        if (weapon as Throwen)
        {
            LeanTween.delayedCall(0.01f, () => Weapons.Remove(weapon));
        }
        else
        {
            Weapons.Remove(weapon);
        }
        // ----------------------

        if (weapon as Throwen)
        {
            (weapon as Throwen).captured -= ThrowenWeapon_Captured;
        }
    }
    
    /// <summary>
    /// А так же назначение типа набора оружия - WeaponSet
    /// </summary>
    /// <param name="newWeapon"></param>
    private void CheckWeaponAndArmAvailable(Weapon newWeapon)
    {
        if (newWeapon is Melee)
        {
            WeaponSet = WeaponSet.MeleeOne;

            if (newWeapon.JointSecond.hilt)
            {
                WeaponSet = WeaponSet.MeleeTwoHanded;
            }
        }
        else if(newWeapon is LongRange)
        {
            WeaponSet = WeaponSet.LongRangeOne;

            if (newWeapon.JointSecond.hilt)
            {
                WeaponSet = WeaponSet.LongRangeTwoHanded;
            }
        }
        else
        {
            WeaponSet = WeaponSet.Throwen;
        }

        if (Weapons.Any())
        {
            var currentWeapon = Weapons.First();
            if (currentWeapon.JointSecond.hilt || newWeapon.JointSecond.hilt || Weapons.Count > 0)
            {
                var length = Weapons.Count;
                for (int i = 0; i < length; i++) ThrowWeapon(Weapons.First());
                RightJointAvailable();
            }
            else
            {
                AvailableArm = new ArmJointSet
                {
                    arm = ragdoll.LeftArm,
                    weaponHolder = animationHandler.LeftWeapon,
                    hand = ragdoll.LeftHand,
                };

                if (currentWeapon is Melee && newWeapon is Melee)
                {
                    WeaponSet = WeaponSet.MeleeTwo;
                }
                else if (currentWeapon is LongRange && newWeapon is LongRange)
                {
                    WeaponSet = WeaponSet.LongRangeTwo;
                }
                else
                {
                    WeaponSet = WeaponSet.MeleeAndLongRange;

                    if (currentWeapon is LongRange)
                    {
                        CastlingWeapons(newWeapon, currentWeapon);
                    }
                }
            }
        }
        else
        {
            RightJointAvailable();
        }

        void RightJointAvailable()
        {
            AvailableArm = new ArmJointSet
            {
                arm = ragdoll.RightArm,
                weaponHolder = animationHandler.RightWeapon,
                hand = ragdoll.RightHand,
            };
        }

        // Реализация для двух оружий

        //if (Weapons.Any())
        //{
        //    var currentWeapon = Weapons.First();
        //    if (currentWeapon.JointSecond.hilt || newWeapon.JointSecond.hilt || Weapons.Count > 1)
        //    {
        //        var length = Weapons.Count;
        //        for (int i = 0; i < length; i++) ThrowWeapon(Weapons.First());
        //        RightJointAvailable();
        //    }
        //    else
        //    {
        //        AvailableArm = new ArmJointSet 
        //        { 
        //            arm = ragdoll.LeftArm, 
        //            weaponHolder = animationHandler.LeftWeapon,
        //            hand = ragdoll.LeftHand,
        //        };

        //        if(currentWeapon is Melee && newWeapon is Melee)
        //        {
        //            WeaponSet = WeaponSet.MeleeTwo;
        //        }
        //        else if(currentWeapon is LongRange && newWeapon is LongRange)
        //        {
        //            WeaponSet = WeaponSet.LongRangeTwo;
        //        }
        //        else
        //        {
        //            WeaponSet = WeaponSet.MeleeAndLongRange;

        //            if(currentWeapon is LongRange)
        //            {
        //                CastlingWeapons(newWeapon, currentWeapon);
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    RightJointAvailable();
        //}

        //void RightJointAvailable()
        //{
        //    AvailableArm = new ArmJointSet
        //    {
        //        arm = ragdoll.RightArm,
        //        weaponHolder = animationHandler.RightWeapon,
        //        hand = ragdoll.RightHand,
        //    };
        //}

        EventsHolder.weaponSetInited.Invoke(WeaponSet);
    }

    public float DistanceToAttack
    {
        get
        {
            float distance = 1.38f;

            if (Weapons.Count > 0)
            {
                distance = Weapons[0].distanceToAttack;
            }

            return distance;
        }
    }

    private void CastlingWeapons(Weapon melle, Weapon longRange)
    {
        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(0.5f);

            ThrowWeapon(longRange);
            ThrowWeapon(melle);

            yield return new WaitForSeconds(0.93f);

            TakeWeapon(melle);

            yield return new WaitForSeconds(0.1f);

            TakeWeapon(longRange);
        }
    }

    public void Mine()
    {
        float maxMineTime = 1.8f;

        Mineable = Miner.Instance.GetTile(Hip.position, LastDir, 5);

        if(Mineable == null)
            return;
        
        if(lastMineable == null || lastMineable.tile != Mineable.tile)
        {
            lastMineable = Mineable;
            mineTimer = 0;
        }

        var dirToMine = (Mineable.blockGlobalPos - head.position).normalized;
        dirToMine *= Dir;
        var angle = Vector2.SignedAngle(dirToMine, Vector2.right);
            
        ragdoll.mineAngle = (int)angle;

        animationHandler.Mine();
        Miner.Instance.Destruction(Mineable, mineTimer, maxMineTime);

        mineTimer += Time.deltaTime;
        
        if(mineTimer > maxMineTime)
        {
            mineTimer = 0;
            Mineable.tilemap.SetTile(Mineable.blockTilePos, null);
            Mineable.tilemap.RefreshTile(Mineable.blockTilePos);

            EventsHolder.onTileMined?.Invoke(Mineable);
        }
    }

    public void Stun()
    {
        ragdoll.IsStuned = true;
        ragdoll.Stun();
    }

    public void BentDown(Collider2D collider)
    {
        previousState = PlayerState;
        PlayerState = PlayerState.ThrowObstacle;

        Ragdoll.IgnoreCollision(collider);
        foreach (var col in Weapons.Select(w => w.Collider))
        {
            Physics2D.IgnoreCollision(col, collider);
        }

        noobAnimations.BentDown();

        TakeItem(collider);
    }

    void TakeItem(Collider2D collider)
    {
        int countTryed = 0;
        float castRadius = 0.1f;

        StartCoroutine(Take());

        IEnumerator Take()
        {
            while (true)
            {
                var hits = Physics2D.CircleCastAll(Ragdoll.RightHand.position, castRadius, Vector2.zero);

                var found = hits.ToList().Find(i => i.collider == collider).collider;
                //print(found);
                if (found)
                {
                    noobAnimations.Stop(0.38f);

                    var joint = collider.gameObject.AddComponent<FixedJoint2D>();
                    joint.connectedBody = Ragdoll.RightArm;
                    TakedItemsJoints.Add(new() { joint = joint });
                    
                    yield return new WaitForSeconds(1.5f);

                    noobAnimations.Play();
                    noobAnimations.ThrowItem();
                    break;
                }
                countTryed++;

                if (countTryed > 100)
                    castRadius += 0.01f;

                if(countTryed > 150)
                {
                    ReturnPreviousState();
                    break;
                }

                yield return null;
            }
        }
    }

    public void ReturnPreviousState()
    {
        if (previousState == PlayerState.Jump)
            previousState = PlayerState.Walk;

        PlayerState = previousState;
    }

    private void CalcultePower()
    {
        var bodyes = GetComponentsInChildren<Rigidbody2D>();
        foreach (var body in bodyes)
        {
            totalPower += Mathf.FloorToInt(body.mass);
        }
    }

    public class ArmJointSet
    {
        public Rigidbody2D arm;
        public Transform weaponHolder;
        public Transform hand;
    }
    
    public class ItemJoint
    {
        public FixedJoint2D joint;
        public float lifetime;
    }
}

public enum Team
{
    Friendly,
    Hostile
}

public enum PlayerState
{
    Idle,
    Walk,
    Jump,
    Attack,
    ThrowObstacle
}


