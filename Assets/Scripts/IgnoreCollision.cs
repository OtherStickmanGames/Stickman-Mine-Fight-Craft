using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    [SerializeField]
    private List<Collider2D> colliders;

    void Start()
    {
        

        for (int i = 0; i < colliders.Count; i++)
        {
            for (int k = i + 1; k < colliders.Count; k++)
            {
                Physics2D.IgnoreCollision(colliders[i], colliders[k]);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        //if (coll.gameObject.CompareTag("Player"))
        //{
        //    Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), coll.gameObject.GetComponent<Collider2D>());
        //}

        print(name);
    }
}
