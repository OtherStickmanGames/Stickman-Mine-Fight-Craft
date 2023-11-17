using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class StickmanController : MonoBehaviour
{
    public Muscle[] muscles;
    public Transform[] bones;

    [SerializeField]
    Vector2 forceVector;
    [SerializeField]
    private float moveDelay;
    [SerializeField]
    private Rigidbody2D rightLeg;
    [SerializeField]
    private Rigidbody2D leftLeg;
    [SerializeField]
    private Rigidbody2D rightArm;
    [SerializeField]
    private Rigidbody2D leftArm;
    [SerializeField]
    private int musclePower = 100;
    [SerializeField] int maxMoveVelocity = 39;
    [SerializeField] public int stunThresold = 30;

    [Space]

    [SerializeField]
    private Transform rightHandPoint;
    [SerializeField]
    private Transform leftHandPoint;
    [SerializeField] Muscle[] armParts = new Muscle[4];


    public Rigidbody2D RightLeg => rightLeg;
    public Rigidbody2D LeftLeg => leftLeg;
    public Rigidbody2D RightArm => rightArm;
    public Rigidbody2D LeftArm => leftArm;
    public Transform RightHand => rightHandPoint;
    public Transform LeftHand => leftHandPoint;

    public bool IsDestroyed { get; set; }
    public bool IsStuned { get; set; }

    public int maxPower;
    public int startHP = 30;
    public int mineAngle = 0;

    float moveDelayPointer;
    bool outerForceImpact;
    int totalMass;

    private void Start()
    {
        armParts = muscles.ToList().Where(m => m.bone.name.Contains("Arm")).ToArray();
        InitHealthComponents();
        maxPower = musclePower;

        totalMass = CalcaulateTotalMass();
    }

    void InitHealthComponents()
    {
        foreach (Muscle muscle in muscles)
        {
            muscle.healthComponent = muscle.bone.GetComponent<HealthComponent>();
            muscle.healthComponent.Init(startHP);
        }
    }

    private void FixedUpdate()
    {
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.K))
            return;
#endif

        if (IsDestroyed)
            return;

        foreach (Muscle muscle in muscles)
        {
            muscle.ActivateMuscle();
            muscle.mineAngle = mineAngle;
        }
    }

    public void Stun()
    {
        musclePower = 0;
        SetMuscleForce(0);
    }

    public void SetForce(Vector2 force, ForceMode2D forceMode = ForceMode2D.Impulse)
    {
        foreach (var item in muscles)
        {
            item.bone.AddForce(force, forceMode);
        }
    }

    public void IgnoreCollision(Collider2D collider, bool ignore = true)
    {
        foreach (var muscle in muscles)
        {
            Physics2D.IgnoreCollision(muscle.bone.GetComponent<Collider2D>(), collider, ignore);
        }
    }

    public void SetForceToArms(int force)
    {
        armParts.ToList().ForEach(a => a.force = force);
    }

    public void Move(Vector2 dir)
    {
        if (Mathf.Abs(CurrentVelocityHorizontal()) > maxMoveVelocity)
            return;
        
        LeftLeg.AddForce(totalMass * dir, ForceMode2D.Force);
        RightLeg.AddForce(totalMass * dir, ForceMode2D.Force);

        //SetForce(totalMass * dir, ForceMode2D.Force);
    }

    public void MoveRight()
    {
        while (Time.time > moveDelayPointer)
        {
            Step1(1);
            Step2(1, 0.085f);
            moveDelayPointer = Time.time + moveDelay;
        }
    }

    public void MoveLeft()
    {
        while (Time.time > moveDelayPointer)
        {
            Step1(-1);
            Step2(-1, 0.085f);
            moveDelayPointer = Time.time + moveDelay;
        }
    }

    public void Jump(int force)
    {
        rightLeg.AddForce(3 * Vector2.up * force, ForceMode2D.Impulse);
        leftLeg.AddForce(3 * Vector2.up  * force, ForceMode2D.Impulse);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            SetMuscleForce(0);
        }
    }

    void Step1(int dir, float delay = 0)
    {
        StartCoroutine(DelayInvoke());

        IEnumerator DelayInvoke()
        {
            yield return new WaitForSeconds(delay);

            rightLeg.AddForce(forceVector * dir, ForceMode2D.Impulse);
            leftLeg.AddForce(forceVector * -0.5f * dir, ForceMode2D.Impulse);
        }
        
    }

    void Step2(int dir, float delay = 0)
    {
        StartCoroutine(DelayInvoke());

        IEnumerator DelayInvoke()
        {
            yield return new WaitForSeconds(delay);

            rightLeg.AddForce(forceVector * -0.5f * dir, ForceMode2D.Impulse);
            leftLeg.AddForce(forceVector * dir, ForceMode2D.Impulse);
        }
    }

    public void SetMuscleForce(int musclePower)
    {
        outerForceImpact = true;

        foreach (Muscle muscle in muscles)
        {
            muscle.force = musclePower;
        }
    }

    public void RestoreMuscleForce(float duration = 5)
    {
        if (IsStuned)
            return;

        if (Velocity() > 10)
        {
            LeanTween.delayedCall(1f, () => RestoreMuscleForce());
            return;
        }

        outerForceImpact = false;

        foreach (Muscle muscle in muscles)
        {
            var startForce = muscle.force;

            LeanTween.value
            (
                gameObject,
                force => 
                {
                    if (gameObject && !outerForceImpact)
                        muscle.force = force; 
                },
                startForce,
                maxPower,
                duration
            );
        }
    }

    int CurrentVelocityHorizontal()
    {
        int velocityX = 0;
        foreach (var muscle in muscles)
        {
            velocityX += (int)muscle.bone.velocity.x;
        }
        return velocityX;
    }

    int CalcaulateTotalMass()
    {
        float mass = 0;
        foreach (var muscle in muscles)
        {
            mass += muscle.bone.mass;
        }

        return (int)mass;
    }

    public int Velocity()
    {
        int result = 0;
        foreach (var muscle in muscles)
        {
            result += Mathf.Abs((int)muscle.bone.velocity.x);
            result += Mathf.Abs((int)muscle.bone.velocity.y);
        }
        //print(result);
        return result;
    }

    private void OnApplicationQuit()
    {
        LeanTween.cancelAll();
    }
}

[System.Serializable]
public class Muscle
{
    public Rigidbody2D bone;
    public Transform boneAnimated;
    public float restRotation;
    public float force;
    [Space]
    public int angleOffset;
    public bool armToMine;
    public int mineAngle;
    [HideInInspector]
    public HealthComponent healthComponent;

    public void ActivateMuscle()
    {
        if (healthComponent.Value < 1 || force < 0.1f)
            return;

        restRotation = boneAnimated.rotation.eulerAngles.z - angleOffset;

        if (armToMine)
            restRotation -= mineAngle;

        bone.MoveRotation(Mathf.LerpAngle(bone.rotation, restRotation, force * Time.deltaTime));    
    }
}
