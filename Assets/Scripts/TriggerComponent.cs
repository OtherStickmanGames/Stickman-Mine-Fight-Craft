using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerComponent : MonoBehaviour
{
    public Collider2D MineCollider;
    public bool triggered;
    public Collider2D collision;
    public float triggeredDuration;

    [SerializeField] Collider2D[] ignores;

    private void Start()
    {
        foreach (var item in ignores)
        {
            Physics2D.IgnoreCollision(item, MineCollider);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        triggered = true;
        this.collision = collision;
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        triggered = true;
        this.collision = collision;
    }


    private void Update()
    {
        if (triggered)
        {
            triggeredDuration += Time.deltaTime;
        }
        else
        {
            triggeredDuration = 0;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        triggered = false;
        this.collision = collision;
    }
}
