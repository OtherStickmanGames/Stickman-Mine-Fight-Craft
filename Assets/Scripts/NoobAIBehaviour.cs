using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoobAIBehaviour : AIBehaviour
{
    NoobAnimationHandler noobAnimations;
    Transform target;

    float targetFindTimer;
    float timerHigherAttackDistance = 5;
    float timerCheckLandscape;

    protected override void Start()
    {
        base.Start();

        noobAnimations = GetComponentInChildren<NoobAnimationHandler>();
    }

    protected override void Update()
    {
        if (!GameManager.Instance.startSimulated)
            return;

        if (player.Ragdoll.IsStuned || player.Ragdoll.IsDestroyed)
            return;

        base.Update();

        targetFindTimer += Time.deltaTime;
        timerCheckLandscape += Time.deltaTime;

        if (target)
        {
            var dir = target.position - player.Hip.position;
            var isBentDown = player.PlayerState == PlayerState.ThrowObstacle;

            if (currentState == AIState.Attack && !isBentDown)
            {
                if (Mathf.Abs(dir.x) < player.DistanceToAttack)
                {
                    LeanTween.delayedCall(1f, () => player.Attack(dir.normalized));
                }
                else
                {
                    player.Move(dir.x > 0 ? Vector2.right : Vector2.left);
                    CheckLandscape();
                }
            }
            if(currentState == AIState.Defence)
            {
                Defence(dir);
            }

            UpdateTarget();
        }
        else if(targetFindTimer > 3)
        {
            FindTarget();
            targetFindTimer = 0;
        }
    }

    void Defence(Vector2 directiontoTarget)
    {
        var distanceToAttack = player.DistanceToAttack;
        if (player.WeaponSet != WeaponSet.LongRangeOne && timerHigherAttackDistance < 5)
        {
            distanceToAttack *= 1.8f;
        }
        //print($"{distanceToAttack} === {directiontoTarget.magnitude}");
        if (distanceToAttack > directiontoTarget.magnitude)
        {
            player.Attack(directiontoTarget.normalized);
            timerHigherAttackDistance = 0;
        }
        else
        {
            player.Idle();
        }

        timerHigherAttackDistance += Time.deltaTime;
    }

    void UpdateTarget()
    {
        if(targetFindTimer > 5)
        {
            FindTarget();
            targetFindTimer = 0;
        }
    }

    void FindTarget()
    {
        var enemies = GameManager.Instance.allPlayers.FindAll(p => p.Team != player.Team && !p.Ragdoll.IsDestroyed);

        float minDistance = float.MaxValue;

        foreach (var item in enemies)
        {
            float dist = Vector2.Distance(player.Hip.position, item.Head.position);
            if(dist < minDistance)
            {
                target = item.Head;
                minDistance = dist;
            }
        }

        if (!target)
            return;

        if(minDistance > 78)
        {
            var pos = GetTeleportPos();
            LeanTween.delayedCall(0.3f, () =>
            {
                var clone = Instantiate(GameManager.Instance.AiPrefab, pos, Quaternion.identity);
                AudioManager.Instance.ZombieSpawn(clone.Hip);
            });
            EffectManager.SpawnEffectZombie(pos);
            gameObject.SetActive(false);
        }

        var dir = target.position - player.Hip.position;
        var dirX = dir.x > 0 ? 1 : -1;
        if (dirX != player.Dir)
        {
            player.Move(dir.normalized);
            LeanTween.delayedCall(0.1f, () => player.Idle());
        }
    }

    Vector3 GetTeleportPos()
    {
        var center = target.position;
        int maxRadiusSpawn = 8;

        var randomDir = Random.insideUnitCircle;
        randomDir.y = Mathf.Abs(randomDir.y);
        Vector3 pos = center + (Vector3)(randomDir * Random.Range(3, maxRadiusSpawn));

        int countTryed = 0;
        //while (Physics2D.OverlapBoxAll(pos, new(2, 3), 0).Length > 0)
        while (Physics2D.CircleCastAll(pos, 3, Vector2.zero).Length > 0)
        {
            randomDir = Random.insideUnitCircle;
            randomDir.y = Mathf.Abs(randomDir.y);
            pos = center + (Vector3)(randomDir * Random.Range(3, maxRadiusSpawn));
            countTryed++;
            if (countTryed > 30)
                maxRadiusSpawn++;
        }

        return pos;
    }

    float checkedPosX;
    int countTryed = 0;

    void CheckLandscape()
    {
        if (timerCheckLandscape < 3 || player.Ragdoll.IsStuned || player.Ragdoll.IsDestroyed)
            return;
        
        timerCheckLandscape = 0;

        Vector2 dir = Vector2.right * player.Dir;
        Vector2 origin = player.Hip.position;

        var lands = Physics2D.CircleCastAll(origin, 0.07f, Vector2.zero);
        foreach (var item in lands)
        {
            if (item.collider.GetComponentInParent<Chunck>())
            {
                EffectManager.Punch(origin);
                return;
            }
        }


        var hits = Physics2D.CircleCastAll(origin + dir, 0.77f, Vector2.zero);
        foreach (var hit in hits)
        {
            //print($"{hit.collider} Dir: {dir}");
            if (hit.collider.GetComponentInParent<Chunck>())
            {
                if(Mathf.Abs(checkedPosX - player.Hip.position.x) < 3)
                {
                    countTryed++;
                }
                else
                {
                    checkedPosX = player.Hip.position.x;
                    countTryed = 0;
                }
                    
                if (!player.Jump() || countTryed > 3)
                    player.Attack();
                return;
            }
        }
        
    }
}
