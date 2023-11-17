using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;
using System.Linq;
using Cinemachine;

public class InputHandler : MonoBehaviour
{
    public CinemachineVirtualCamera cam;

    GameObject hook;
    Rigidbody2D hookBody;

    StickmanController ragdoll;
    GameObject spawnPrefab;
    Vector3 touchPos;

    private void Start()
    {

    }


    private void Update()
    {
        SpawnObject();
        ObjectDrag();
        CameraMove();
        CameraZoom();
        KeyboardInput();
    }

    void KeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            EventsHolder.onLeftControl?.Invoke();
        }
    }

    void CameraZoom()
    {
        cam.m_Lens.OrthographicSize -= Input.GetAxis("Mouse ScrollWheel") * 10;

        if (Input.touchCount == 2 && !IsUIClick())
        {
            var touchOne = Input.GetTouch(0);
            var touchTwo = Input.GetTouch(1);

            var touchOneLastPos = touchOne.position - touchOne.deltaPosition;
            var touchTwoLastPos = touchTwo.position - touchTwo.deltaPosition;

            var distance = (touchOneLastPos - touchTwoLastPos).magnitude;
            var currentDistance = (touchOne.position - touchTwo.position).magnitude;

            var difference = currentDistance - distance;
            cam.m_Lens.OrthographicSize -= difference * 0.05f;
        }
    }

    void CameraMove()
    {
        if (Input.GetMouseButtonDown(0) && Input.touchCount != 2)
        {
            //var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //var hit = Physics2D.Raycast(pos, Vector2.zero);

            //if (!hit || !hit.collider.GetComponent<Rigidbody2D>())
            if(!hook)
            {
                touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        if (Input.GetMouseButton(0) && touchPos != Vector3.zero && Input.touchCount != 2)
        {
            var direction = touchPos - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Camera.main.transform.position += direction;
            cam.transform.position += direction;
        }

        if (Input.GetMouseButtonUp(0))
        {
            touchPos = Vector3.zero;
        }
    }

    void ObjectDrag()
    {
        if (Input.GetMouseButtonDown(0) && Input.touchCount != 2)
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hits = Physics2D.CircleCastAll(pos, 0.3f, Vector2.zero);
            var hit = hits.ToList().Find(h => h.collider.GetComponent<Rigidbody2D>() && h.collider.GetComponent<CollisionHandler>());
            if (!hit)
                hit = hits.ToList().Find(h => h.collider.GetComponent<Rigidbody2D>());
            if (hit)
            {
                hook = new("Hook (Created from code)");
                hookBody = hook.AddComponent<Rigidbody2D>();
                var joint = hook.AddComponent<HingeJoint2D>();
                var collider = hook.AddComponent<CircleCollider2D>();
                collider.radius = 0.1f;

                GameManager.Instance.allPlayers.ForEach(p => p.Ragdoll.IgnoreCollision(collider));

                hookBody.gravityScale = 0;
                hookBody.mass = 50;

                hook.transform.position = pos;
                joint.connectedBody = hit.collider.attachedRigidbody;

                ragdoll = hit.transform.GetComponentInParent<StickmanController>();

                

                EventsHolder.onPlayerKeepItem?.Invoke(hit.collider.gameObject);
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (hook)
            {
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                hookBody.MovePosition(pos);
                CheckRagdoll();
            }
        }

        if (Input.GetMouseButtonUp(0) && hook)
        {
            Destroy(hook);

            EventsHolder.onPlayerDropItem?.Invoke();
            EventsHolder.onPlayerKeepItem?.Invoke(null);

            if (ragdoll)
            {
                ragdoll.RestoreMuscleForce();
                ragdoll = null;
            }
        }
    }

    void CheckRagdoll()
    {
        if(ragdoll && hookBody.velocity.magnitude > 5 && !ragdoll.IsStuned)
        {
            ragdoll.SetMuscleForce(5);
        }
    }

    void SpawnObject()
    {
        if (spawnPrefab && Input.GetMouseButtonDown(0) && !IsUIClick())
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;
            var spawned = Instantiate(spawnPrefab, pos, Quaternion.identity);
            
            EventsHolder.onObjectSpawned?.Invoke(spawned);
        }
    }

    bool IsUIClick()
    {
        var eSystem = EventSystem.current;

        List<RaycastResult> hits = new();
        //eSystem.RaycastAll(new(eSystem) { position = Input.mousePosition }, hits);
        eSystem.RaycastAll(new(eSystem) { position = Input.GetTouch(0).position }, hits);

        foreach (var hit in hits)
        {
            if (hit.gameObject.layer == 5)
                return true;
        }

        return false;
    }

}
