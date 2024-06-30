using Architecture;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Block, Interactable
{
    Player player;

    public Door()
    {
        player = GameObject.FindObjectOfType<Player>();
    }


    public void Interact()
    {
        //Debug.Log(Vector2.Distance(tile));
    }
}
