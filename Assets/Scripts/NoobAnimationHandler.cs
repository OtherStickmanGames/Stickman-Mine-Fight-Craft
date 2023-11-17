using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoobAnimationHandler : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve angleToTimeCurve;
    [SerializeField] float attackRate = 1.5f;

    Animator animator;
    PlayerBehaviour playerBehaviour;
    WeaponSet weaponSet;
    float currentRate;
    bool isMove;

    public int Dir => (int)animator.GetFloat("Dir");

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
        playerBehaviour = GetComponentInParent<PlayerBehaviour>();

        animator.SetFloat("Dir", 1);
    }


    public void Move(int dir)
    {
        animator.SetFloat("Dir", dir);
        
        if(!isMove || (isMove && Dir != dir))
        {
            animator.Play(GetWalkName());
        }
    }

    public void SetWeaponSet(WeaponSet weaponSet)
    {
        this.weaponSet = weaponSet;
    }

    public void Attack(Vector2 dir)
    {
        switch (weaponSet)
        {
            case WeaponSet.None:
                if (attackRate < currentRate)
                {
                    var randomDelay = Random.Range(0, 0.5f);
                    LeanTween.delayedCall(randomDelay, () => animator.Play(GetAttackName()));
                    
                    currentRate = 0;
                }
                else
                {
                    var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    //print(stateInfo.normalizedTime / stateInfo.length);
                    if(stateInfo.normalizedTime / stateInfo.length > 1)
                    {
                        //animator.Play("& Idle");
                    }
                }
                break;
            case WeaponSet.LongRangeOne:
                //var angle = MobileInput.VectorToSignedAngle(dir);
                //var offset = -18 * animator.GetFloat("Dir");
                //angle += offset;
                //float normalizedTime = angleToTimeCurve.Evaluate(angle);
                //var animState = "Noobick Long-Range";
                //animator.Play(animState, 0, normalizedTime);
                break;
            case WeaponSet.LongRangeTwo:
                break;
            case WeaponSet.LongRangeTwoHanded:
                break;
            case WeaponSet.MeleeOne:
                animator.Play("& Axe Attack 1");
                break;
            case WeaponSet.MeleeTwo:
                break;
            case WeaponSet.MeleeTwoHanded:
                break;
            case WeaponSet.MeleeAndLongRange:
                break;
            case WeaponSet.Throwen:
                break;
        }
        
    }

    public void Idle()
    {
        animator.Play("& Idle");
    }

    public void BentDown()
    {
        animator.Play("& Bent Down");
    }

    public void ThrowItem()
    {
        animator.Play("& Throw Item");
    }

    public void Jump(JumpSet jumpSet)
    {
        switch (jumpSet)
        {
            case JumpSet.Up:
                animator.Play("Noob Jump");
                break;
            case JumpSet.Forward:
                break;
            case JumpSet.Back:
                animator.Play("& Noob Jump Backflip");
                break;
            
        }
    }

    public void Stop(float delay)
    {
        LeanTween.delayedCall(delay, () => animator.enabled = false);
    }

    string GetWalkName()
    {
        string walk = "& Run";
        if (!playerBehaviour)
            walk = "& Zombie Walk";

        return walk;
    }

    string GetAttackName()
    {
        string attack = "$ One Punch";
        if (!playerBehaviour)
            attack = "& Zombie Attack";

        return attack;
    }

    public void Play()
    {
        animator.enabled = true;
    }

    private void Update()
    {
        currentRate += Time.deltaTime;

    }

}
