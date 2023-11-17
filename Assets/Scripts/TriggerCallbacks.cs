using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class TriggerCallbacks : MonoBehaviour
{

    public Action<Collider2D> onTriggered;
    public Action<Collider2D> deTriggered;
    public Action<Collider2D> triggerStay;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        onTriggered?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        deTriggered?.Invoke(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        triggerStay?.Invoke(collision);
    }
}
