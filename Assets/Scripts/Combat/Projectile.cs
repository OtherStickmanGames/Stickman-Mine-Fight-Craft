using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IDamage
{
    [SerializeField]
    float damageValue = 1f;
    public int OwnerID { get; set; }
    public int? ID { get; set; }
    public float Lifetime { get; set; }
    public float DamageValue { get => damageValue; set => damageValue = value; }
    public Rigidbody2D Body => body;

    private Rigidbody2D body;

    List<HealthComponent> hitsObjects = new();
    int weaponDamage;

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.relativeVelocity.sqrMagnitude < 80)
            return;
        //if (collision.gameObject.layer == Constants.LAYER_PLAYER)
        //{
        var health = collision.gameObject.GetComponent<HealthComponent>();

        if (health && !hitsObjects.Contains(health))
        {
            hitsObjects.Add(health);

            health.Value -= weaponDamage + DamageValue;

            SpawnSplash(collision);
        }

        Destroy(gameObject);
        //}
    }

    public void Init(Player owner, int weaponDamage)
    {
        this.weaponDamage = weaponDamage;
        body = GetComponent<Rigidbody2D>();
        var col = GetComponent<Collider2D>();

        var allies = GameManager.Instance.allPlayers.FindAll(p => p.Team == owner.Team);

        foreach (var ally in allies)
        {
            foreach (var muscle in ally.Ragdoll.muscles)
            {
                Physics2D.IgnoreCollision(col, muscle.bone.GetComponent<Collider2D>());
            }

            foreach (var weapon in ally.Weapons)
            {
                Physics2D.IgnoreCollision(col, weapon.Collider);
            }
        }
        
        Lifetime = 0;
    }

    void SpawnSplash(Collision2D collision, bool isBlood = true)
    {
        var pos = collision.GetContact(0).point;
        var dir = collision.relativeVelocity.normalized * -1;
        pos += dir * 0.1f;
        if (isBlood)
        {
            //BloodManager.SpawnBulletSplash(pos, dir);
        }

    }

    private void Update()
    {
        Lifetime += Time.deltaTime;

        if(Lifetime > 8f && GameManager.DESTROY_BULLET)
        {
            Destroy(gameObject);
        }
    }

    public static int GenerateID() => Random.Range(10000, 100000);
}
