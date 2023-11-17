using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockHelper : MonoBehaviour
{
    TriggerComponent[] triggers;
    public GameObject keeped;
    public GameObject connectedBlock;

    private void Awake()
    {
        triggers = GetComponentsInChildren<TriggerComponent>(true);

        EventsHolder.onPlayerDropItem.AddListener(Item_Droped);
        EventsHolder.onPlayerKeepItem.AddListener(Item_Keeped);

        SetEnableColliders(false);
    }

    private void Item_Droped()
    {
        if (keeped == transform.root.gameObject || !keeped)
            return;

        foreach (var trigger in triggers)
        {
            var hits = Physics2D.BoxCastAll(trigger.transform.position, Vector2.one * 0.7f, 0, Vector2.zero);
            var hit = hits.ToList().Find(h => h.collider.gameObject == keeped);
            //print("+++++++++++++++++++++++");
            //foreach (var item in hits)
            //{
            //    print(item.collider);
            //}

            //print("=================");
            //print($"{keeped} - keeped");
            if (hit && !keeped.GetComponent<FixedJoint2D>() && keeped.GetComponentInChildren<BlockHelper>().connectedBlock != transform.root.gameObject)
            {
                StartCoroutine(Connect());
            }

            IEnumerator Connect()
            {
                hit.collider.enabled = false;

                var connected = keeped;
                var body = connected.GetComponent<Rigidbody2D>();
                connectedBlock = connected;

                yield return new WaitForSeconds(0.1f);

                body.simulated = false;
                //body.MovePosition(trigger.transform.position);
                //body.MoveRotation(trigger.transform.rotation);
                connected.transform.position = trigger.transform.position;
                connected.transform.rotation = trigger.transform.rotation;
                print("коннектим");
                yield return new WaitForSeconds(0.1f);
                var joint = connected.AddComponent<FixedJoint2D>();
                joint.connectedBody = transform.root.GetComponent<Rigidbody2D>();
                joint.breakForce = 8000;
                body.simulated = true;
                hit.collider.enabled = true;

                EventsHolder.onBlockConnected?.Invoke(connected);

                yield return new WaitForSeconds(0.3f);
                if (joint)
                    joint.autoConfigureConnectedAnchor = false;
            }
        }

        keeped = null;
    }


    private void Item_Keeped(GameObject item)
    {
        if (GameManager.Instance.startSimulated)
            return;

        if (item)
        {
            if (item.CompareTag("BLOCK") && item != transform.root.gameObject)
            {
                SetEnableColliders(true);
                keeped = item;
            }
        }
        else
        {
            SetEnableColliders(false);
        }
    }

    void SetEnableColliders(bool value)
    {
        foreach (var col in triggers)
        {
            col.gameObject.SetActive(value);
        }
    }
}
