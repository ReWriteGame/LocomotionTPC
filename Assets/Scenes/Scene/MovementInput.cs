using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script requires you to have setup your animator with 3 parameters, "InputMagnitude", "InputX", "InputZ"
//With a blend tree to control the inputmagnitude and allow blending between animations.
[RequireComponent(typeof(CharacterController))]
public class MovementInput : MonoBehaviour
{
    public float InputX;
    public float InputZ;
    public Vector3 desiredMoveDirection;
    public bool blockRotationPlayer;
    public float desiredRotationSpeed;
    public Animator anim;
    public float Speed;
    public float allowPlayerRotation;
     private Camera cam;
    public CharacterController controller;
    public bool isGrounded;
    private float verticalVel;
    private Vector3 moveVector;

    [Header("Feet Grounder")]
    private Vector3 rightFootPosition;
    private Vector3 leftFootPosition;
    private Vector3 rightFootIKPosition;
    private Vector3 leftFootIKPosition;
    private Quaternion leftFootIKRotation;
    private Quaternion rightFootIKRotation;
    private float lastPelvisPositionY;
    private float lastRightFootPositionY;
    private float lastLeftFootPositionY;

    public bool enableFeetIK = true;
    public float hightFromGroundRaycast = 1.1f;
    public float raycastDownDistance = 1.5f;
    public LayerMask enviromentLayer;
    private float pelvisOffset;
    private float prlvisUpAndDownSpeed;
    private float feetToIKPositionSpeed;

    private string leftFootAnimName = "LeftFoodCurve";
    private string rightFootAnimName = "RightFoodCurve";

    public bool usePRoIKFeature = false;
    public bool showSolverDebug = true;
    
    
    
    
    
    void Start()
    {
        anim = this.GetComponent<Animator>();
        cam = Camera.main;
        controller = this.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        InputMagnitude();

        //If you don't need the character grounded then get rid of this part.
        isGrounded = controller.isGrounded;
        if (isGrounded)
        {
            verticalVel -= 0;
        }
        else
        {
            verticalVel -= 2;
        }

        moveVector = new Vector3(0, verticalVel, 0);
        controller.Move(moveVector);
        //
    }

    private void FixedUpdate()
    {
        
        if(enableFeetIK== false)return;
        if(anim == null) return;

        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);
        
        FeetPositionSolver(rightFootPosition,ref rightFootIKPosition,ref rightFootIKRotation);
        FeetPositionSolver(leftFootPosition,ref leftFootIKPosition,ref leftFootIKRotation);
    }


    private void OnAnimatorIK(int layerIndex)
    {
        if(enableFeetIK== false)return;
        if(anim == null) return;
        
        MovePelvisHeight();
        
        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot,1);

        if (usePRoIKFeature)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot,anim.GetFloat(rightFootAnimName));
        }

        MoveFeetToPointIK(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPositionY);
        
        /////////////////////////////////////////////////////////

        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot,1);

        if (usePRoIKFeature)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot,anim.GetFloat(leftFootAnimName));
        }

        MoveFeetToPointIK(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY);

    }

    private void MoveFeetToPointIK(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFoodPositionY)
    {
        Vector3 targetPositionIK = anim.GetIKPosition(foot);
        if (positionIKHolder != Vector3.zero)
        {
            targetPositionIK = transform.InverseTransformPoint(targetPositionIK);
            positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

            float yVar = Mathf.Lerp(lastFoodPositionY, positionIKHolder.y, feetToIKPositionSpeed);
            targetPositionIK.y += yVar;
            lastFoodPositionY = yVar;

            targetPositionIK = transform.TransformPoint(targetPositionIK);
            anim.SetIKPosition(foot,targetPositionIK);
        }
    }

    private void MovePelvisHeight()
    {
        if (rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0)
        {
            lastPelvisPositionY = anim.bodyPosition.y;
            return;
        }

        float lOffsetPosition = leftFootIKPosition.y - transform.position.y;
        float rOffsetPosition = rightFootIKPosition.y - transform.position.y;

        float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;

        Vector3 newPelvisPosition = anim.bodyPosition + Vector3.up * totalOffset;
        newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, prlvisUpAndDownSpeed);
        anim.bodyPosition = newPelvisPosition;
        lastPelvisPositionY = anim.bodyPosition.y;
    }

    private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPosition, ref Quaternion feetIKRotation)
    {
        RaycastHit feetOutHit;
        
        if(showSolverDebug)
            Debug.DrawLine(fromSkyPosition, fromSkyPosition+ Vector3.down * (raycastDownDistance + hightFromGroundRaycast),Color.yellow);

        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + hightFromGroundRaycast,
                enviromentLayer))
        {
            feetIKPosition = fromSkyPosition;
            feetIKPosition.y = feetOutHit.point.y - pelvisOffset;
            feetIKRotation = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;
            return;
        }

        feetIKPosition = Vector3.zero; 
    }

    private void AdjustFeetTarget(ref Vector3 feetPosition, HumanBodyBones foot)
    {
        feetPosition = anim.GetBoneTransform(foot).position;
        feetPosition.y = transform.position.y + hightFromGroundRaycast;
    }

    void PlayerMoveAndRotation()
    {
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        var camera = Camera.main;
        var forward = cam.transform.forward;
        var right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        desiredMoveDirection = forward * InputZ + right * InputX;

        if (blockRotationPlayer == false)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection),
                desiredRotationSpeed);
        }
    }

    void InputMagnitude()
    {
        //Calculate Input Vectors
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        anim.SetFloat("InputZ", InputZ, 0.0f, Time.deltaTime * 2f);
        anim.SetFloat("InputX", InputX, 0.0f, Time.deltaTime * 2f);

        //Calculate the Input Magnitude
        Speed = new Vector2(InputX, InputZ).sqrMagnitude;

        //Physically move player
        if (Speed > allowPlayerRotation)
        {
            anim.SetFloat("InputMagnitude", Speed, 0.0f, Time.deltaTime);
            PlayerMoveAndRotation();
        }
        else if (Speed < allowPlayerRotation)
        {
            anim.SetFloat("InputMagnitude", Speed, 0.0f, Time.deltaTime);
        }
    }
}