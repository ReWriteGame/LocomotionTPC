using System;
using System.Collections;
//using Modules.Cooldown;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private Movement movement;
    [SerializeField] private float moveSpeed = 1;

    [SerializeField] private bool jump;
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private float heightJump = 1;
    [SerializeField] private float durationJump = 1;
    [SerializeField] private float jumpTimeout = 0.50f;// delay until next use
    [SerializeField] private float fallTimeout = 0.15f;// delay until next use
    [SerializeField] private float rotationSpeed = 5f;

    [SerializeField] private Vector3 groundedOffset = Vector3.zero;
    [SerializeField] private float groundedRadius = 0.28f;
    [SerializeField] private LayerMask groundLayers;

    [SerializeField] private bool useGravity = true;
    [SerializeField] private float gravityScale = 1;
    [SerializeField] private bool useLocalGravity = false;
    [SerializeField] private Vector3 localGravity;
    [SerializeField] private bool grounded;
    [SerializeField] private Vector3 gravityForce = Vector3.zero;

    public Action OnJump;
    public Action OnFreeFall;
    public Action OnGrounded;

    private Vector3 inputMoveDirection;
    private float jumpDirectionY = 0;
    private Coroutine jumpCoroutine;
    private bool accumulationGravity = true;
    //private Cooldown jumpCooldown;
    //private Cooldown fallCooldown;

    public bool Jump { get => jump; set => jump = value; }
    public Vector3 InputMoveDirection { get => inputMoveDirection; set => inputMoveDirection = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public Movement Movement => movement;
    public bool Grounded => grounded;
    //public float JumpTimeoutDelta => jumpCooldown.CurrentTime;
    //public float FallTimeoutDelta => fallCooldown.CurrentTime;
    public Vector2 CurrentHorMove => new Vector2(movement.Controller.velocity.x,movement.Controller.velocity.z);


    private void Awake() => Initialize();
    private void Update() => MovementLogic();

    public void Initialize()
    {
      //  jumpCooldown = new Cooldown(jumpTimeout);
      //  fallCooldown = new Cooldown(fallTimeout);
    }

    private void MovementLogic()// long jump?
    {
        GroundedCheckLogic();
        
        accumulationGravity = !grounded;
        //if (!grounded) fallCooldown.Activate();
        //
        //if (jump && jumpCooldown.Activate(jumpTimeout))
        //{
        //    useGravity = false;
        //    OnJump?.Invoke();
        //    JumpLogic();
        //}

        GravityCalculation();

        Vector3 allAdditionalForces = new Vector3(0, jumpDirectionY, 0) + gravityForce;
        movement.SetMoveValues(inputMoveDirection, moveSpeed, allAdditionalForces);
    }

    private void JumpLogic()
    {
        if (jumpCoroutine != null) StopCoroutine(jumpCoroutine);
        jumpCoroutine = StartCoroutine(JumpLogicRoutine());
    }

    private IEnumerator JumpLogicRoutine()
    {
        float lastValue = 0;
        for (float time = durationJump; time > 0; time -= Time.deltaTime)
        {
            yield return null;

            float value = 1 - time / durationJump;
            Vector2 directionAnimationCurve = new Vector2(value - lastValue, jumpCurve.Evaluate(value) - jumpCurve.Evaluate(lastValue));

            float jumpDistance = directionAnimationCurve.y * heightJump;
            jumpDirectionY = jumpDistance / durationJump / (value - lastValue);

            lastValue = value;
        }
        jumpDirectionY = 0;// jumpCurve.Evaluate(max)
    }
    private void GroundedCheckLogic()
    { 
        bool newGroundedState = GroundedCheckSphere(transform.position, groundedOffset, groundedRadius, groundLayers);
        if (newGroundedState == true && grounded == false)
        {
            useGravity = true;
            OnGrounded?.Invoke();
        }

        if (newGroundedState == false && grounded == true)
            OnFreeFall?.Invoke();
        
        grounded = newGroundedState;
    }
    
    private bool GroundedCheckSphere(Vector3 position, Vector3 positionOffset, float sphereRadius, LayerMask groundLayers)
    {
        Vector3 spherePosition = position + positionOffset;
        return Physics.CheckSphere(spherePosition, sphereRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void GravityCalculation()
    {
        Vector3 gravityDirection = useLocalGravity ? localGravity : Physics.gravity;
        if (accumulationGravity) gravityForce += gravityDirection * gravityScale * Time.deltaTime;
        else gravityForce = Vector3.down * 2;
        if (!useGravity) gravityForce = Vector3.zero;
    }

    public void RotateTowardsVector(Vector2 direction)
    {
        if (direction == Vector2.zero) return;
        float targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public Vector3 GetRelativeHorizontalMovement(Vector2 inputVector, Vector3 relativeVector)
    {
        Vector3 relativeVectorRight = Vector3.Cross(Vector3.up, relativeVector).normalized;
        return relativeVector * inputVector.y + relativeVectorRight * inputVector.x;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + groundedOffset, groundedRadius);
    }
}
