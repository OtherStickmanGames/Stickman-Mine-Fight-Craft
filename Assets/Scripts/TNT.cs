using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Linq;

public class TNT : MonoBehaviour
{
    public bool useAsTNT;
    public int radius = 5;
    public GameObject effect;
    public SpriteRenderer spriteRenderer;
    private enum Mode { simple, adaptive }
    [SerializeField] private Mode mode;
    [SerializeField] public float power;
    [SerializeField] public bool spawnEffect = true;
    [SerializeField] public LayerMask layerMask;

    [HideInInspector] public bool playerDamage;

    private IEnumerator Start()
    {
        yield return null;

        AudioManager.Instance.Explosion(transform);

        var hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero);
        var hit = hits.ToList().Find(h => h.collider.GetComponent<Tilemap>());
        if (hit)
        {
            var tilemap = hit.collider.GetComponent<Tilemap>();
            WorldDestructor.Instance.Destruct(tilemap, transform.position, radius);
        }

        yield return null;

        Explosion2D(transform.position);

        effect.SetActive(spawnEffect);
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(10);

        Destroy(gameObject);
    }

    void Explosion2D(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius, layerMask);

        foreach (Collider2D hit in colliders)
        {
            if (hit.attachedRigidbody != null)
            {
                Vector3 direction = hit.transform.position - position;
                direction.z = 0;

                if (CanUse(position, hit.attachedRigidbody))
                {
                    hit.attachedRigidbody.AddForce(direction.normalized * power, ForceMode2D.Impulse);
                    if (useAsTNT)
                    {
                        var player = hit.gameObject.GetComponentInParent<Player>();
                        if(player && player.NoobAIBehaviour && !player.Ragdoll.IsDestroyed)
                        {
                            player.Stun();
                            player.Ragdoll.muscles.ToList().ForEach(m => m.healthComponent.Value -= 50);

                            if (Random.Range(0, 10) >= 5)
                                AudioManager.Instance.Screem(player.Hip);
                        }
                    }
                    if (playerDamage)
                    {
                        var player = hit.gameObject.GetComponentInParent<Player>();
                        if (player && !player.NoobAIBehaviour)
                        {
                            player.GetComponent<HealthComponent>().Value -= 1 + (GameManager.Wave / 15);
                            playerDamage = false;

                            if (Random.Range(0, 10) >= 5)
                                AudioManager.Instance.Screem(player.Hip);
                        }

                        
                    }
                }
            }
        }
    }

    bool CanUse(Vector3 position, Rigidbody2D body)
    {
        if (mode == Mode.simple) return true;

        RaycastHit2D hit = Physics2D.Linecast(position, body.position);

        if (hit.rigidbody != null && hit.rigidbody == body)
        {
            return true;
        }

        return false;
    }
}
