using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class DepthSortingFitter : MonoBehaviour
{
    [SerializeField] int depth;
    [SerializeField] Color32 color;
    [SerializeField] bool enableSorting;
    [SerializeField] bool disableSkin;

    [SerializeField]
    private List<SpriteRenderer> spritesAnimated;

    private void Awake()
    {
        if (enableSorting)
        {
            spritesAnimated.ForEach(s => 
            {
                if (disableSkin)
                {
                    s.enabled = false;
                    s.GetComponent<SpriteSkin>().enabled = false;
                }

                s.sortingOrder = depth;
                s.color = color;
            });
        }
    }
}
