using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sky : MonoBehaviour
{
    Vector3 originScale;

    private void Start()
    {
        originScale = transform.localScale;
    }

    private void Update()
    {
        transform.localScale = originScale * (Camera.main.orthographicSize / 3.9f);
    }
}
