using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Move : MonoBehaviour
{
    [SerializeField] private Character character;

    [SerializeField] private float moveSpeed = 1;
    //[SerializeField] private float moveAxeleration;
    //[SerializeField] private float movedamp;

    [SerializeField] private float sprintSpeed = 5;
    //[SerializeField] private float sprintAxeleration;
    //[SerializeField] private float sprintdamp;

    //[SerializeField] private float rotationDegreePerSecond = 120f;
    //[SerializeField] private float rotationSmoothTime = 0.12f;

    [SerializeField] private float speedChangeRate = 10.0f;
    [SerializeField] private float speedOffset = 0.1f;

    [SerializeField] private bool sprint = false;
    [SerializeField] private bool analogMovement = true;


    public Vector2 inputUserDirection;


    private Camera playerCamera;
    private float currentSpeed;
    private float targetSpeed;

    public Vector3 CameraDirectionHor => new Vector3(playerCamera.transform.forward.x, 0, playerCamera.transform.forward.z).normalized;


    private void FixedUpdate() => MoveLogic();

    private void MoveLogic()
    {
        float maxSpeed = sprint ? sprintSpeed : moveSpeed;
        if (inputUserDirection == Vector2.zero) maxSpeed = 0.0f;

        float inputMagnitude = analogMovement ? inputUserDirection.magnitude : 1f;
        targetSpeed = maxSpeed * inputMagnitude;


        float currentHorSpeed = character.CurrentHorMove.magnitude;

        // accelerate or decelerate to target speed
        bool useSpeedCorrect = currentHorSpeed < targetSpeed - speedOffset ||
                               currentHorSpeed > targetSpeed + speedOffset;
        float speedCorrect = Mathf.Lerp(currentHorSpeed, targetSpeed,
            Time.deltaTime * speedChangeRate);

        currentSpeed = useSpeedCorrect ? speedCorrect : targetSpeed;
        currentSpeed = (float)Math.Round(currentSpeed, 3);

        Vector3 relativeMoveDirection =
            character.GetRelativeHorizontalMovement(inputUserDirection, CameraDirectionHor).normalized;
        character.RotateTowardsVector(new Vector2(relativeMoveDirection.x, relativeMoveDirection.z));
        character.InputMoveDirection = new Vector3(relativeMoveDirection.x, 0, relativeMoveDirection.z);
        character.MoveSpeed = currentSpeed;
    }

    //private float MoveSpeedCalculation()
    //{
    //    
    //}
}