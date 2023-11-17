using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class AnimationHandler : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve angleToTimeCurve;
    [SerializeField]
    private AnimationCurve XDirToIKCurve;
    [SerializeField]
    private AnimationCurve YDirToIKCurve;
    [SerializeField]
    private Rigidbody2D rightArmLow;
    [SerializeField]
    private Transform rightWeapon;
    [SerializeField]
    private Transform leftWeapon;

    [Header("IK Points")]
    [SerializeField]
    private Transform rightArmIK;
    [SerializeField]
    private Transform leftArmIK;

    public float mult = 1.8f;

    [Space]

    [SerializeField]
    private List<SpriteRenderer> sprites;
    [SerializeField]
    private GameObject testSword;

    public Transform RightWeapon => rightWeapon;
    public Transform LeftWeapon => leftWeapon;
    public bool IsIdle { get; set; }
    private string AnimDir => directionView > 0 ? "" : Constants.Inverse;

    public int DirHorizontal => (int)animator.GetFloat("Dir Horizontal");


    int directionView = 1;
    string lastAnimState;
    float lastJoystickAngle;
    float timerResetJoystickAngle;
    bool isMove;

    Player player;
    Animator animator;
    WeaponSet weaponSet;
    Vector2 lastRightJoystickValue;

    private void Awake()
    {
        sprites.ForEach(s => { s.sortingOrder = -5; s.color -= new Color(0, 0, 0, 0.3f); });

        player = GetComponentInParent<Player>();
        animator = GetComponentInParent<Animator>();

        if (testSword)
            testSword.SetActive(false);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    animator.SetTrigger("Grozniy Ebaka");
        //}

        timerResetJoystickAngle += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.N))
        {
            animator.SetBool("Mirror", !animator.GetBool("Mirror"));
        }
    }

    public void Move(Vector2 dir)
    {
        IsIdle = false;

        animator.SetFloat("Dir Horizontal", dir.x);

        animator.Play("Move");
    }

    public void Mine()
    {
        animator.Play("Mine");

        IsIdle = false;
    }

    public void SetDirectionView(int dir)
    {
        if (animator.GetInteger("Dir View") != dir)
        {
            animator.SetInteger("Dir View", dir);
            animator.SetFloat("Inverse", dir);
            directionView = dir;
        }

        bool joystickInOneSide = lastRightJoystickValue.x > -0.1f && dir > 0 || lastRightJoystickValue.x < 0.1f && dir < 0;
        
        if(!joystickInOneSide && weaponSet.IsMeleeSet())
        {
            dir *= -1; 
            if (animator.GetInteger("Dir View") != dir)
            {
                animator.SetInteger("Dir View", dir);
                animator.SetFloat("Inverse", dir);
                directionView = dir;
            }
        }
    }

    
    public void ChooseAnimation(Vector2 dir)
    {
        var magnitude = dir.magnitude;

        if (weaponSet == WeaponSet.MeleeAndLongRange)
        {
            if(magnitude > Constants.Melee_And_Long_Range_Attack_Range_Thresold)
            {
                Attack(dir);
            }
        }
        else
        {
            if (magnitude > 0.7f)
            {
                Attack(dir);
            }
            else if (magnitude > 0.05f)
            {
                IKControl(dir);
            }
        }

        lastRightJoystickValue = dir;
    }

    public void IKControl(Vector2 dir)
    {
        if (!weaponSet.IsMeleeSet())
            return;

        float x = XDirToIKCurve.Evaluate(dir.x * mult);
        float Y = XDirToIKCurve.Evaluate(dir.y * mult);
        
        rightArmIK.localPosition = new Vector3(x, Y);

        animator.SetBool("IK Control", true);
        
    }

    public void Attack(Vector2 dir)
    {
        IsIdle = false;
        animator.SetBool("IK Control", false);

        if (weaponSet == WeaponSet.LongRangeOne || weaponSet == WeaponSet.MeleeOne)
        {
            OneHandWeapon(dir);
        }
        else if (weaponSet == WeaponSet.LongRangeTwo)
        {
            TwoWeapon(dir);
        }
        else if(weaponSet == WeaponSet.MeleeAndLongRange)
        {
            MeleeAndLongRangeAttack(dir);
        }
    }

   
    private void MeleeAndLongRangeAttack(Vector2 dir)
    {
        //var angle = MobileInput.VectorToSignedAngle(dir);
        //rightArmIK.localPosition = new Vector2(-0.09f, 0.05f);
        //string animState = string.Empty;

        //if(dir.magnitude < Constants.Melee_And_Long_Range_Attack_Melee_Thresold)
        //{
        //    float x = XDirToIKCurve.Evaluate(dir.x * mult);
        //    float Y = XDirToIKCurve.Evaluate(dir.y * mult);

        //    leftArmIK.localPosition = new Vector3(x, Y);

        //    //animState = $"Melee Long-Range Attack Long-Range{AnimDir}";
        //}
        //else
        //{
        //    leftArmIK.localPosition = Vector3.zero;
        //    animState = $"Melee Long-Range Attack Melee{AnimDir}";
        //}

        //float normalizedTime = angleToTimeCurve.Evaluate(angle);
        //if (!string.IsNullOrEmpty(animState))
        //    animator.Play(animState, 0, normalizedTime);
    }

    private void OneHandWeapon(Vector2 dir)
    {
        //var angle = MobileInput.VectorToSignedAngle(dir);
        //rightArmIK.localPosition = new Vector2(-0.09f, 0.05f);
        //string animState;

        //if (weaponSet == WeaponSet.MeleeOne)
        //{
        //    animState = AnimStateByJoystickMoveDirection(angle);
        //}
        //else
        //{
        //    animState = $"Long-Range One Hand Attack{AnimDir}";
        //}

        //float normalizedTime = angleToTimeCurve.Evaluate(angle);
        //animator.Play(animState, 0, normalizedTime);
    }

    internal void Jump()
    {
        animator.Play("Jump");
    }

    private void TwoWeapon(Vector2 dir)
    {
        //var angle = MobileInput.VectorToSignedAngle(dir);
        //rightArmIK.localPosition = new Vector2(-0.09f, 0.05f);
        //string animState;

        //if (weaponSet == WeaponSet.LongRangeTwo)
        //{
        //    animState = $"Long-Range Two Attack{AnimDir}";
        //}
        //else
        //{
        //    print("Сюда никогда не заходит???????");
        //    animState = $"Long-Range One Hand Attack{AnimDir}";
        //}

        //float normalizedTime = angleToTimeCurve.Evaluate(angle);
        //animator.Play(animState, 0, normalizedTime);
    }

    public void Idle()
    {
        if (!IsIdle)
        {
            IsIdle = true;

            animator.Play("Idle");

            var value = weaponSet == WeaponSet.LongRangeTwo ? 1f : 0.1f;

            StartCoroutine(Delay(value));
        }

        IEnumerator Delay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (IsIdle)
            {
                animator.SetTrigger("Idle");
                animator.SetBool("IK Control", false);
            }
        }

        lastRightJoystickValue = Vector2.zero;

        leftArmIK.localPosition = Vector3.zero;
    }

    public void ChooseWeaponPose(WeaponSet weaponSet)
    {
        this.weaponSet = weaponSet;

        switch (weaponSet)
        {
            case WeaponSet.None:
                break;
            case WeaponSet.LongRangeOne:
                animator.SetTrigger(Constants.Long_Range_One_Hand_Pose);
                break;
            case WeaponSet.LongRangeTwo:
                animator.SetTrigger(Constants.Long_Range_Two_Pose);
                break;
            case WeaponSet.LongRangeTwoHanded:
                animator.SetTrigger(Constants.Long_Range_Two_Hand_Pose);
                break;
            case WeaponSet.MeleeOne:
                animator.SetTrigger(Constants.Melee_One_Hand_Pose);
                break;
            case WeaponSet.MeleeTwo:
                break;
            case WeaponSet.MeleeTwoHanded:
                break;
            case WeaponSet.MeleeAndLongRange:
                animator.SetTrigger(Constants.Melee_And_Long_Range_Pose);
                break;
            case WeaponSet.Throwen:
                animator.SetTrigger(Constants.Throwen_Two_Hand_Taking);

                break;
            default:
                break;
        }

        leftArmIK.localPosition = Vector3.zero;
    }

    public void MoveRightArmToHilt(Weapon weapon, Transform rightHand, Action callback)
    {
        StartCoroutine(Move());

        IEnumerator Move()
        {
            yield return new WaitForSeconds(0.88f);

            while (Vector2.Distance(weapon.JointFirst.hilt.position, rightHand.position) > 0.1f)//0.087f)
            {
                rightArmIK.position = weapon.JointFirst.hilt.position;

                yield return new WaitForSeconds(0.01f);
            }

            rightArmIK.parent.gameObject.SetActive(false);

            callback?.Invoke();
        }
    }


    //Coroutine moveArm;
    public void MoveLeftArmToHilt(Weapon weapon, Transform leftHand, Action callback)
    {
        //if(moveArm != null) StopCoroutine(moveArm);
        //leftArmIK.localPosition = Vector3.zero;
        /*moveArm =/**/ StartCoroutine(Move());

        IEnumerator Move()
        {
            yield return new WaitForSeconds(0.88f);

            while (Vector2.Distance(weapon.JointSecond.hilt.position, leftHand.position) > 0.1f)// 0.087f)
            {
                leftArmIK.position = weapon.JointSecond.hilt.position;

                yield return new WaitForSeconds(0.01f);
            }

            leftArmIK.parent.gameObject.SetActive(false);

            callback?.Invoke();

            //moveArm = null;
        }
    }

    public void GotUp()
    {
        animator.SetTrigger(Constants.Throwen_Two_Hand_Pose);

        StartCoroutine(Move(rightArmIK, 0.1f));
        StartCoroutine(Move(leftArmIK,  3.7f));

        IEnumerator Move(Transform ik, float delay)
        {
            var startPosIk = ik.localPosition;

            var step = 1f / 100f;

            yield return new WaitForSeconds(delay);

            for (float f = 0; f < 30; f += step)
            {
                ik.localPosition = Vector2.Lerp(startPosIk, Vector2.zero, f);

                yield return null;
            }
        }
    }

    private string AnimStateByJoystickMoveDirection(float angle)
    {
        if (Mathf.Approximately(lastJoystickAngle, 0))
        {
            lastJoystickAngle = angle;
        }

        string result;

        if(Mathf.Abs(lastJoystickAngle - angle) < 8)
        {
            return lastAnimState;
        }

        if (lastJoystickAngle > angle)
        {
            //lastAnimState = "Melee One Hand Attack Clockwise";
            //lastAnimState = "Melee One Hand Attack";
            lastAnimState = $"Melee One Hand Imba Attack{AnimDir}";
            result = lastAnimState;
        }
        else if(lastJoystickAngle < angle)
        {
            //lastAnimState = "Melee One Hand Attack";
            /////////lastAnimState = "Melee One Hand Saber Attack";
            lastAnimState = $"Melee One Hand Imba Attack{AnimDir}";
            result = lastAnimState;
        }
        else
        {
            result = lastAnimState;
        }

        if (timerResetJoystickAngle > 0.3f)
        {
            lastJoystickAngle = angle;
        }

        return result;
    }

    public void Dance()
    {
        List<string> dances = new()
        {
            "Dance 01",
            "Dance 02",
            "Dance 03",
            "Dance 04",
            "Dance 05",
        };

        var dance = dances[Random.Range(0, dances.Count)];
        animator.Play(dance);
    }

    public void Dance(string dance)
    {
        animator.Play(dance);
    }

    //public void SetAttackState(string value) => player.isAttack = bool.Parse(value);

    //public void Weighting()
    //{
    //    normalMass = rightArmLow.mass;
    //    rightArmLow.mass = 2.5f;
    //}

    //public void NormalMass()
    //{
    //    rightArmLow.mass = normalMass;
    //}

}

