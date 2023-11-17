using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] StickmanController ragdoll;
    [SerializeField] public float jumpPower = 1f;
    [SerializeField] float punchPower = 1f;
    [SerializeField] float physicsMoveSpeed = 1f;

    Player player;

    int totalMass;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Start()
    {
        totalMass = CalcaulateTotalMass();
    }

    // Called From Animation 
    public void ZombieAttack()
    {
        var force = totalMass * GetDir() * Vector2.right * 3;
        //force.y += totalMass / 5;
        ragdoll.RightArm.AddForce(force, ForceMode2D.Impulse);
        ragdoll.LeftArm.AddForce(force, ForceMode2D.Impulse);

        player.Attacking = true;
        LeanTween.delayedCall(0.18f, () => player.Attacking = false);
    }

    // Called From Animation 
    public void ThrowItem()
    {
        foreach (var item in player.TakedItemsJoints)
        {
            Destroy(item.joint);
        }
        player.TakedItemsJoints.Clear();
        player.ReturnPreviousState();
    }

    public void Anim_Jump(JumpSet jumpSet)
    {
        switch (jumpSet)
        {
            case JumpSet.Up:
                var force = jumpPower * totalMass * Vector2.up;
                ragdoll.SetForce(force);
                break;
            case JumpSet.Forward:
                break;
            case JumpSet.Back:
                break;
        }
    }

    public void Anim_Punch(int dir)
    {
        ragdoll.RightArm.AddForce(punchPower * dir * totalMass * Vector2.right, ForceMode2D.Impulse);
        player.Attacking = true;
        LeanTween.delayedCall(0.58f, () => player.Attacking = false);
    }

    public void Anim_BackToPreviousState()
    {
        player.ReturnPreviousState();
    }

    public void Anim_SetIdleState()
    {
        player.Idle();
    }

    public void Anim_MineEffect()
    {
        if (player.Mineable == null)
            return;

        var dir = (player.Mineable.blockGlobalPos - player.Ragdoll.RightArm.transform.position).normalized;

        var pos = player.Mineable.blockGlobalPos - (dir);// * .5f);
        //var pos = Miner.Instance.GetMineEffectPos(player.Ragdoll.RightArm.position, dir);
        dir.y *= -1;
        var angle = Vector2.SignedAngle(dir, Vector2.up);

        EffectManager.SpawnMineEffect(player.Mineable, pos, angle);
    }

    public void PhysicsMove(int dir)
    {
        ragdoll.RightLeg.AddForce(dir * physicsMoveSpeed * ragdoll.RightLeg.mass * Vector2.right, ForceMode2D.Force);
        ragdoll.LeftLeg.AddForce(dir * physicsMoveSpeed * ragdoll.LeftLeg.mass * Vector2.right, ForceMode2D.Force);
    }

    int CalcaulateTotalMass()
    {
        float mass = 0;
        foreach (var item in ragdoll.muscles)
        {
            mass += item.bone.mass;
        }

        return (int)mass;
    }

    int GetDir()
    {
        return (int)animator.GetFloat("Dir");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            foreach (var item in ragdoll.muscles)
            {
                item.bone.AddForce(Random.insideUnitCircle.normalized * 500, ForceMode2D.Impulse);
            }
        }
    }
}
