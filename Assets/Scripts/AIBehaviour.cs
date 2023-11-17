using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviour : MonoBehaviour
{
    [SerializeField] public AIState currentState;

    protected Player player;

    List<Player> targetPlayers;
    List<Weapon> targetWeapons;

    protected AnimationHandler animationHandler;
    Weapon nearestWeapon;
    Player nearestPlayer;
    AIState state;
    Vector2 dir;


    protected virtual void Start()
    {
        targetPlayers = new List<Player>();
        targetWeapons = new List<Weapon>();

        player = GetComponent<Player>();
        animationHandler = GetComponentInChildren<AnimationHandler>();

        //if (team == Team.Hostile)
        //{
        //    player.Move(Vector2.left);
        //}
        //else
        //{
        //    player.Move(Vector2.right);
        //}

        CheckStartState();
    }

    void CheckStartState()
    {
        switch (currentState)
        {
            case AIState.None:
                break;
            case AIState.MoveToWeapon:
                break;
            case AIState.Attack:
                break;
            case AIState.Dance:
                animationHandler.Dance();
                break;

            case AIState.Dance_01:
                animationHandler.Dance("Dance 01");
                break;
            case AIState.Dance_02:
                animationHandler.Dance("Dance 02");
                break;
            case AIState.Dance_03:
                animationHandler.Dance("Dance 03");
                break;
            case AIState.Dance_04:
                animationHandler.Dance("Dance 04");
                break;
            case AIState.Dance_05:
                animationHandler.Dance("Dance 05");
                break;
        }
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            CheckEnvironment();
        }

        //if(state == AIState.MoveToWeapon)
        //{
        //    var dist = Vector2.Distance(player.Hip.position, nearestWeapon.transform.position);

        //    if(dist < 1f)
        //    {
        //        player.TakeWeapon(nearestWeapon);

        //        state = AIState.None;
        //        FindNearestPlayer();
        //    }
        //    else
        //    {
        //        player.Move(dir);
        //    }
        //}
        //else if(state == AIState.Attack)
        //{
        //    dir = (nearestPlayer.Hip.position + new Vector3(0, 1)) - player.Hip.position;
        //    dir.Normalize();

        //    player.Attack(dir);
        //}
    }

    void FindNearestPlayer()
    {
        var minDistance = float.MaxValue;
        foreach (var player in targetPlayers)
        {
            var dist = Vector2.Distance(player.Hip.position, this.player.Hip.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPlayer = player;
            }
        }

        if (nearestPlayer)
        {
            dir = nearestPlayer.Hip.position - player.Hip.position;
            dir.Normalize();

            state = AIState.Attack;
        }
    }

    private void CheckEnvironment()
    {
        targetWeapons.Clear();
        targetPlayers.Clear();

        var hits = Physics2D.CircleCastAll(player.Hip.position, 30, Vector2.zero);

        foreach (var hit in hits)
        {
            var player = hit.collider.transform.root.GetComponent<Player>();
            if (player && !targetPlayers.Contains(player) && player != this.player)
            {
                targetPlayers.Add(player);
            }

            var weapon = hit.collider.transform.root.GetComponent<Weapon>();
            if (weapon && !targetWeapons.Contains(weapon))
            {
                targetWeapons.Add(weapon);
            }
        }

        if(player.Weapons.Count == 0 && targetWeapons.Count > 0)
        {
            var minDistance = float.MaxValue;
            nearestWeapon = null;
            foreach (var weapon in targetWeapons)
            {
                var dist = Vector2.Distance(player.Hip.position, weapon.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestWeapon = weapon;
                }
            }

            if (nearestWeapon)
            {
                dir = nearestWeapon.transform.position - player.Hip.position;
                dir.Normalize();

                state = AIState.MoveToWeapon;
            }
        }
    }

    public enum AIState
    {
        None,
        MoveToWeapon,
        Attack,
        Defence,
        Dance,
        Dance_01,
        Dance_02,
        Dance_03,
        Dance_04,
        Dance_05,
    }
}
