using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawned : MonoBehaviour
{
    [SerializeField] float distanceToTake = 3.8f;
    [SerializeField] int distanceToDestroy = 88;
    [SerializeField] new SpriteRenderer renderer;
    [SerializeField] GameObject takeEffectPrefab;

    public SpriteRenderer Renderer => renderer;

    public Rigidbody2D Body => body;

    Rigidbody2D body;
    Player player;

    public float Distance;
    float lifetime;
    bool taking;

    public void Init(Sprite sprite)
    {
        renderer.sprite = sprite;

        body = GetComponent<Rigidbody2D>();
    }


    private void Update()
    {
        lifetime += Time.deltaTime;

        if(lifetime > 3)
        {
            foreach (var p in GameManager.Instance.allPlayers)
            {
                var distance = Vector2.Distance(p.Hip.position + Vector3.down, transform.position);
                if (distance < distanceToTake)
                {
                    var dir = (p.Hip.position - transform.position).normalized;

                    //body.MovePosition(transform.position + (dir * Time.deltaTime));
                    body.AddForce(dir * 0.38f, ForceMode2D.Impulse);

                    if(distance < 0.88f && !taking)
                    {
                        taking = true;

                        LeanTween.scale(gameObject, Vector3.zero, 0.18f)
                            .setEaseOutQuad()
                            .setOnComplete(() => Take(p));
                    }
                    
                    
                }
            }
        }

        
    }

    private void Take(Player player)
    {
        Instantiate(takeEffectPrefab, transform.position, Quaternion.identity);

        player.GetComponent<Inventory>().TakeItem
        (
            new() { id = 0, sprite = renderer.sprite }
        );
    }
}
