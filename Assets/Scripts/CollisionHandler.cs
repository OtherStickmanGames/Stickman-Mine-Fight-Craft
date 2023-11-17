using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CollisionHandler : MonoBehaviour
{
    [SerializeField] int angleOffset;
    [SerializeField] SpriteRenderer[] leftDecals;
    [SerializeField] SpriteRenderer[] rightDecals;
    public Transform damageDirection;
    public Player Player { private get; set; }

    public Action<float> onCollision;

    HealthComponent hp;
    CollisionDir dir;
    float collisionRate;

    private void Start()
    {
        gameObject.name += " " + gameObject.GetHashCode();
        hp = GetComponent<HealthComponent>();

        foreach (var item in leftDecals)
        {
            //BloodManager.SetColor(item);
            item.color = new(item.color.r, item.color.g, item.color.b, 0);
        }

        foreach (var item in rightDecals)
        {
            //BloodManager.SetColor(item);
            item.color = new(item.color.r, item.color.g, item.color.b, 0);
        }
    }

    private void Update()
    {
        collisionRate += Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (collisionRate < 0.9f)
            return;

        var body = GetComponent<Rigidbody2D>();
        var force = coll.relativeVelocity.normalized;

        var magnitude = coll.relativeVelocity.magnitude;

        if (magnitude > 8)
        {
            CheckCollisionDirection(coll);
           
            if (magnitude > 18)
            {
                var damage = (magnitude - 18) * 0.18f;

                hp.Value -= damage;
                GameManager.Instance.totalDamage += (int)damage;
                onCollision?.Invoke(magnitude);

                ShowDecal();
                SpawnBlood(coll);
                CheckOtherCollider(coll);

                collisionRate = 0;
            }

            if (body.velocity.magnitude > 5)
            {
                //print($"{coll.rigidbody?.velocity}  {coll.otherRigidbody?.velocity}");
                //print($"{view.ViewID} epta {body.velocity.magnitude}");
                //print($"{force} {coll.relativeVelocity} {coll.otherRigidbody.velocity}");
                //coll.rigidbody.AddRelativeForce(force * -100, ForceMode2D.Impulse);
                //coll.rigidbody.AddForce(force * -100, ForceMode2D.Impulse);
                //coll.transform.root.GetComponent<Player>().Stun();
                
                //print(coll.transform.root.GetComponent<PhotonView>().ViewID);
                //print(GetComponent<PhotonView>().ViewID + " Я ёбнул");
            }
            else
            {
                //print($" {coll.relativeVelocity} {coll.otherRigidbody.velocity}");
            }
        }
        
    }

    void ShowDecal()
    {
        if (leftDecals.Length == 0)
            return;

        if(dir == CollisionDir.Left)
        {
            var bro = 2f * (hp.Value / hp.MaxValue);
            //print(bro);
            var idx = Mathf.Clamp((int)(2 - bro), 0, 1);
            // HOT FIX
            if (idx == leftDecals.Length)
                return;

            var c = leftDecals[idx].color;
            
            var a = idx == 0 ? (2 - bro) : (1 - bro);
            leftDecals[idx].color = new(c.r, c.g, c.b, a);
        }

        if (dir == CollisionDir.Right)
        {
            var bro = 2f * (hp.Value / hp.MaxValue);
            //print(bro);
            var idx = Mathf.Clamp((int)(2 - bro), 0, 1);
            // HOT FIX
            if (idx == rightDecals.Length)
                return;

            var c = rightDecals[idx].color;

            var a = idx == 0 ? (2 - bro) : (1 - bro);
            rightDecals[idx].color = new(c.r, c.g, c.b, a);
        }
    }

    void CheckCollisionDirection(Collision2D coll)
    {
        if (!damageDirection)
            return;

        var velocity = coll.contacts[0].normal;// coll.relativeVelocity;
        velocity.y *= -1f;
        var angle = Vector2.SignedAngle(velocity, Vector2.up);

        //print(velocity);
        //print(angle);
        damageDirection.rotation = Quaternion.Euler(0, 0, angle);

        var localAngle = damageDirection.localRotation.eulerAngles.z;

        //print($"{localAngle} # {name}");

        localAngle += angleOffset;
        if (localAngle > 360)
            localAngle -= 360;

        if ((localAngle > -38 && localAngle < 38) || (localAngle > 315 && localAngle < 395))
        {
            dir = CollisionDir.Left;
        }
        else if (localAngle > 135 && localAngle < 225)
        {
            dir = CollisionDir.Right;
        }
        else if (localAngle > 235 && localAngle < 315)
        {
            dir = CollisionDir.Up;
        }
        else if (localAngle > 50 && localAngle < 150)
        {
            dir = CollisionDir.Down;
        }

        //print(dir);
    }

    private void OnJointBreak2D(Joint2D joint)
    {
        hp.Value = 0;

        GameManager.Instance.countParts++;
    }

    void SpawnBlood(Collision2D collision)
    {
        //BloodManager.Spawn(collision.GetContact(0).point, transform);
    }

    void CheckOtherCollider(Collision2D collision)
    {
        //var environment = collision.collider.GetComponent<EnvironmentObject>();
        //Vector3 pos = collision.GetContact(0).point;
        //if (environment && environment.AvailableSpace(pos))
        //{            
        //    pos.z = -1;

        //    if (environment.PosY != null)
        //        pos.y = environment.PosY.Value;

        //    if (environment.PosX != null)
        //        pos.x = environment.PosX.Value;

        //    var rot = environment.Angle;
            
        //    var blood = BloodManager.SpawnSplash(pos, rot);
        //    environment.AddSplash(blood.transform);
        //}
    }
}

public enum CollisionDir
{
    Left,
    Right,
    Up,
    Down
}
