using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.Serialization;


public class CharacterControllerLogic : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float sprintSpeed = 2;

    [SerializeField] private float rotationDegreePerSecond = 120f;
    [SerializeField] private float directionDampTime = 0.25f;
    [SerializeField] private float speedDampTime = 0.05f;
    
    [SerializeField] private float fovDampTime = 3f;

    [SerializeField] private float jumpMultiplier = 1f;
    [SerializeField] private float jumpDist = 1f;

    [SerializeField] private float speedOffset = 0.1f;
    [SerializeField] private float speedChangeRate = 10.0f;
    
    [SerializeField]private bool isSprint;
    [SerializeField]private bool analogMovement;
    [SerializeField]private float targetSpeed;
    
    private AnimatorStateInfo stateInfo;
    private AnimatorTransitionInfo transInfo;
   
    private CharacterController character;

    [Header("Dynamic")] public Vector2 inputDirection;
    public float currentSpeed;
    public float direction;
    public float charAngle;
    public float testSpeed;

    [Header("Test Animation")] 
    public bool useInter = false;
    private const float SPRINT_FOV = 75.0f;
    private const float NORMAL_FOV = 60.0f;

    private int m_LocomotionId;
    private int m_LocomotionPivotLId;
    private int m_LocomotionPivotRId;
    private int m_LocomotionPivotLTransId;
    private int m_LocomotionPivotRTransId;

    public Animator Animator => animator;
    public float CurrentSpeed => currentSpeed;
    public float LocomotionThreshold => 0.2f;
    public Vector3 InputDirectionV3 => new Vector3(inputDirection.x, 0, inputDirection.y);
    public Vector3 CurrentDirection => new Vector3(transform.forward.x, 0, transform.forward.z);

    public Action OnSetNewInputMoveDirection;

    private void SetNewDirection(Vector2 newDirection)
    {
        if (CompareVector2(inputDirection, newDirection, 0.01f)) return;
        inputDirection = newDirection;
        OnSetNewInputMoveDirection?.Invoke();
    }

    private bool CompareVector2(Vector2 a, Vector2 b, float epsilon = 0.001f) =>
        Mathf.Abs(a.x - b.x) < epsilon && Mathf.Abs(a.y - b.y) < epsilon;


    public float CalculateRotationAngle(Vector2 vector1, Vector2 vector2)
    {
        return Vector2.SignedAngle(vector1, vector2);
    }

    private static Vector2 NormalizedToOne(Vector2 vector) // todo  as method .magnitode
    {
        return vector.magnitude > 1 ? vector.normalized : vector;
    }

    private void Start()
    {
        character = GetComponent<CharacterController>();

        if (animator.layerCount >= 2)
            animator.SetLayerWeight(1, 1);

        m_LocomotionId = Animator.StringToHash("Base Layer.Locomotion");
        m_LocomotionPivotLId = Animator.StringToHash("Base Layer.LocomotionPivotL");
        m_LocomotionPivotRId = Animator.StringToHash("Base Layer.LocomotionPivotR");
        m_LocomotionPivotLTransId = Animator.StringToHash("Locomotion -> LocomotionPivotL");
        m_LocomotionPivotRTransId = Animator.StringToHash("Locomotion -> LocomotionPivotR");
    }


    private void CalculateMove()
    {
    }

    
    private void Update()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        transInfo = animator.GetAnimatorTransitionInfo(0);

        GetInputDirection();
        bool ispiv = IsInPivot();
        isSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Jump");
        bool isJump = Input.GetKey(KeyCode.Space);

        StickToWorldSpace(CurrentDirection, camera.transform, ispiv);

        MoveSpeedCalculation();
        animator.SetFloat("Speed", currentSpeed);
        
        //currentSpeed = (isSprint ? sprintSpeed : moveSpeed) * inputDirection.magnitude;
        //currentSpeed = Round(currentSpeed);
        //animator.SetFloat("Speed", currentSpeed, speedDampTime, Time.deltaTime);
        
        if(!useInter)animator.SetFloat("Speed", currentSpeed);
        animator.SetFloat("Speed", currentSpeed, speedDampTime, Time.deltaTime);
        
        animator.SetFloat("Direction", direction, directionDampTime, Time.deltaTime);
        animator.SetFloat("Angle", charAngle);

        if (currentSpeed < LocomotionThreshold && Mathf.Abs(inputDirection.x) < 0.05f) // Dead zone
        {
            animator.SetFloat("Direction", 0f);
            animator.SetFloat("Angle", 0f);
           // animator.SetFloat("Speed", 0f);
        }

        animator.SetBool("Jump", isJump);
        
        
        //////////////////////////////////////Other//////////////////////////////////////
        
        float curFOV = isSprint ? SPRINT_FOV : NORMAL_FOV;
        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, curFOV, fovDampTime * Time.deltaTime);
    }

    private void MoveSpeedCalculation()
    {
        float maxSpeed = isSprint ? sprintSpeed : moveSpeed;
        if (inputDirection == Vector2.zero) maxSpeed = 0.0f;

        float inputMagnitude = analogMovement ? inputDirection.magnitude : 1f;
        targetSpeed = maxSpeed * inputMagnitude;


        float currentHorSpeed = currentSpeed;// need make y = 0
        testSpeed = character.velocity.magnitude;
        // accelerate or decelerate to target speed
        bool useSpeedCorrect = currentHorSpeed < targetSpeed - speedOffset ||
                               currentHorSpeed > targetSpeed + speedOffset;
        float speedCorrect = Mathf.Lerp(currentHorSpeed, targetSpeed,
            Time.deltaTime * speedChangeRate);

        currentSpeed = useSpeedCorrect ? speedCorrect : targetSpeed;
        currentSpeed = (float)Math.Round(currentSpeed, 3);
    }

    private void FixedUpdate()
    {
        //Rotate character model if stick is tilted right or left, but only if character is moving in that direction
        if (IsInLocomotion() && !IsInPivot() &&
            ((direction >= 0 && inputDirection.x >= 0) || (direction < 0 && inputDirection.x < 0)))
        {
            Vector3 rotationAmount = Vector3.Lerp(Vector3.zero,
                new Vector3(0f, rotationDegreePerSecond * (inputDirection.x < 0f ? -1f : 1f), 0f),
                Mathf.Abs(inputDirection.x));
            Quaternion deltaRotation = Quaternion.Euler(rotationAmount * Time.deltaTime);
            transform.rotation = this.transform.rotation * deltaRotation;
        }

        if (IsInJump())
        {
            float oldY = transform.position.y;
            transform.Translate(Vector3.up * jumpMultiplier * animator.GetFloat("JumpCurve"));
            if (IsInLocomotionJump()) transform.Translate(Vector3.forward * Time.deltaTime * jumpDist);

            character.height = (animator.GetFloat("CapsuleCurve") * 0.5f);
            camera.transform.Translate(Vector3.up * (transform.position.y - oldY));
        }
    }

    private void GetInputDirection()
    {
        Vector2 newDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        SetNewDirection(NormalizedToOne(newDirection));
    }


    public bool IsInJump() => IsInIdleJump() || IsInLocomotionJump();
    public bool IsInIdleJump() => animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.IdleJump");
    public bool IsInLocomotionJump() => animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.LocomotionJump");
    public bool IsInLocomotion() => stateInfo.nameHash == m_LocomotionId;

    public bool IsInPivot()
    {
        return stateInfo.nameHash == m_LocomotionPivotLId ||
               stateInfo.nameHash == m_LocomotionPivotRId ||
               transInfo.nameHash == m_LocomotionPivotLTransId ||
               transInfo.nameHash == m_LocomotionPivotRTransId;
    }


    private void StickToWorldSpace(Vector3 currentDirection, Transform camera, bool isPivoting)
    {
        Vector3 cameraDirection = camera.forward;
        cameraDirection.y = 0.0f; // kill Y
        Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, cameraDirection.normalized);

        Vector3 moveDirection = referentialShift * InputDirectionV3;
        Vector3 axisSign = Vector3.Cross(moveDirection, currentDirection);

        //float angleRootToMove = Vector3.Angle(currentDirection, moveDirection) * (axisSign.y >= 0 ? -1f : 1f);
        //angleRootToMove = (float)Math.Round(angleRootToMove, 2);


        ////////////////////////////////////////////////
        float horizontalCameraDirection = camera ? camera.transform.eulerAngles.y : 0;
        Vector3 direction2 = RotateVectorRelativeToOther(InputDirectionV3, horizontalCameraDirection);

        float angleRootToMove = Vector3.Angle(currentDirection, direction2) * (axisSign.y >= 0 ? -1f : 1f);
        angleRootToMove = (float)Math.Round(angleRootToMove, 2);
        ///////////////////////////////////////////////

        charAngle = isPivoting ? 0 : angleRootToMove;
        charAngle = angleRootToMove;
        angleRootToMove /= 180f;
        direction = angleRootToMove * moveSpeed;
    }

    private float Round(float value)
    {
        if (value < 0.01f) value = 0;
        return (float)Math.Round(value, 2);
    }

    private void OnDrawGizmos()
    {
        DrawInputDirection();
        DrawCurrentStateMove();
        DrawPassedPath();
        DrawCameraForwardDirection();
    }


    private void Awake()
    {
        StartCoroutine(fdd());
    }

    private Vector3[] points = new[] { Vector3.forward };

    private IEnumerator fdd()
    {
        points = new Vector3[10];
        while (true)
        {
            yield return new WaitForSeconds(.2f);
            for (int j = 0; j < points.Length - 1; j++)
                points[j] = points[j + 1];

            points[points.Length - 1] = transform.position;
        }
    }

    private void DrawPassedPath()
    {
        float thickness = 2;
        Color color = Color.red;

        Handles.color = color;
        for (int i = 0; i < points.Length - 1; i++)
            DrawArrow.HandlesArrow(points[i], points[i + 1], true, thickness, thickness);
    }

    private void DrawCurrentStateMove()
    {
        float thickness = 2;
        float lineLength = 3;
        float circleRadius = 0.25f;
        Vector3 shift = Vector3.up * .01f;
        Color color = Color.yellow;

        Vector3 direction = transform.forward;
        float currentSpeedInPercent = Mathf.InverseLerp(0f, 1f, currentSpeed);
        Vector3 start = transform.position + shift + direction.normalized * circleRadius;
        Vector3 endMax = start + direction.normalized * lineLength;
        Vector3 endCurrent = start + direction * lineLength * currentSpeedInPercent;

        Handles.color = color;
        DrawArrow.HandlesArrow(start, endMax, false, thickness, thickness); // max
        DrawArrow.HandlesArrow(start, endCurrent, true, thickness, thickness); // current
        Handles.DrawWireDisc(transform.position + shift, Vector3.up, circleRadius);
    }

    private void DrawInputDirection()
    {
        float thickness = 2;
        float lineLength = 3;
        float circleRadius = 0.3f;
        Color color = Color.cyan;
        Vector2 inputUserDirection = this.inputDirection;

        float horizontalCameraDirection = camera ? camera.transform.eulerAngles.y : 0;
        Vector3 inputDirection = new Vector3(inputUserDirection.x, 0, inputUserDirection.y);
        Vector3 direction = RotateVectorRelativeToOther(inputDirection, horizontalCameraDirection);
        Vector3 start = transform.position + direction.normalized * circleRadius;
        Vector3 endMax = transform.position + direction.normalized * lineLength;
        Vector3 endCurrent = transform.position + direction * lineLength;

        Handles.color = color;
        DrawArrow.HandlesArrow(start, endMax, false, thickness, thickness); // max
        DrawArrow.HandlesArrow(start, endCurrent, true, thickness, thickness); // current
        Handles.DrawWireDisc(transform.position, Vector3.up, circleRadius);
    }

    private void DrawCameraForwardDirection()
    {
        float thickness = 2;
        float lineLength = 3;
        float circleRadius = 0.2f;
        Color color = Color.white;

        Vector3 direction = new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z);
        Vector3 start = transform.position + direction.normalized * circleRadius;
        Vector3 endMax = transform.position + direction.normalized * lineLength;

        Handles.color = color;
        DrawArrow.HandlesArrow(start, endMax, true, thickness, thickness);
        Handles.DrawWireDisc(transform.position, Vector3.up, circleRadius);
    }


    private Vector3 RotateVectorRelativeToOther(Vector3 directionVector, float rotationVector)
    {
        Quaternion rotationY = Quaternion.Euler(0f, rotationVector, 0f);
        return rotationY * directionVector;
    }
}