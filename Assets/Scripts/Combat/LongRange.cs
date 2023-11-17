using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongRange : Weapon
{
    [Header("=== Long Range ===")]
    
    [Space]

    [SerializeField]
    private float rateOfFire = 0.5f;
    [SerializeField]
    private float forceBack = 18f;
    [SerializeField]
    private Projectile projectilePrefab;
    [SerializeField]
    private Transform muzzle;
    [SerializeField]
    private LongRangeType type;

    [Space]

    [SerializeField] Sprite[] splashes;
    [SerializeField] SpriteRenderer splashRender;
    [SerializeField] Animator splashAnimator;

    public System.Action<Projectile> onAttack;

    float currentRate;

    public override void Attack(Vector2 dir)
    {
        base.Attack(dir);

        if (currentRate > rateOfFire)
        {
            var p = Shot(Projectile.GenerateID());

            currentRate = 0;

            body.AddForce(body.mass * forceBack * -transform.right, ForceMode2D.Impulse);

            onAttack?.Invoke(p);
        }
    }

    public Projectile Shot(int projectileID)
    {
        var p = Instantiate(projectilePrefab, muzzle.position, transform.rotation);
        var body = p.GetComponent<Rigidbody2D>();
        var pCollider = p.GetComponent<Collider2D>();

        Physics2D.IgnoreCollision(collider, pCollider);
        foreach (var weapon in owner.Weapons)
        {
            Physics2D.IgnoreCollision(weapon.Collider, pCollider);
        }

        var forceDir = transform.right;

        body.AddForce(30 * body.mass * forceDir, ForceMode2D.Impulse);
        p.Init(owner, (int)damage);
        p.ID = projectileID;
        p.transform.up = forceDir;

        StartSplash();

        return p;
    }

    protected override void Update()
    {
        base.Update();

        // Костыль, чтобы два одинаковых пистолета стреляли чуть-чуть асинхронно
        currentRate += Time.deltaTime;
        if (Random.Range(0, 10) < 7)
        {
            currentRate += Time.deltaTime;
        }

        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    owner = FindObjectOfType<Player>();
        //    Shot(1);
        //}
    }

    public override void SetOwner(Player player)
    {
        base.SetOwner(player);

        if (type == LongRangeType.Pistol)
        {
            IdleRotate();
        }
    }

    private void IdleRotate()
    {
        Transform hand = null;
        int idx = owner.Weapons.IndexOf(this);
        switch (idx)
        {
            case 0:
                hand = owner.Handler.RightWeapon;
                break;
            case 1:
                hand = owner.Handler.LeftWeapon;
                break;
            default:
                break;
        }

        StartCoroutine(Duration(hand));        

        IEnumerator Duration(Transform hand)
        {
            float timer = 0;
            while (timer < 1.5f)
            {
                if (!owner)
                    break;
                if (owner.isLongRangeAttack)
                    break;

                RestRotation = hand.rotation.eulerAngles.z;
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }

    public override void Idle()
    {
        base.Idle();

        if(type == LongRangeType.Pistol)
        {
            IdleRotate();
        }
    }

    void StartSplash()
    {
        splashRender.sprite = splashes[Random.Range(0, splashes.Length)];
        splashAnimator.Play("Splash");
    }

    public enum LongRangeType
    {
        Rifle,
        Pistol,
    }
}
