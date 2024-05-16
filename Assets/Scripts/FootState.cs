using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FootState : MonoBehaviour
{
    [SerializeField] private Transform leftFoot;
    [SerializeField] private Transform rightFoot;
    [SerializeField] private Transform centerPelvis;
   
    [SerializeField] private Animator animator;
    [SerializeField] public float footDistance;
    [SerializeField] public bool forwardFoot; //=> x1> x2 ;// &&&&&&&&&&&&&&&???????????????????????????????????????????????????????
    [SerializeField] public bool stableFoot;



    public float L;
    public float R;

    private void Update() => RecountLogic();

    private float initialDistance;

    void Start()
    {
        initialDistance = Vector3.Distance(leftFoot.position, rightFoot.position);
    }

    Vector3 CalcProjection(Vector3 center, Vector3 point, Vector3 direction)
    {
        var a = point - center;
        var c = direction - center;
        return center + Vector3.Project(a, c.normalized);
    }

    private Vector3 caltest(Vector3 direction, Vector3 point)
    {
        return Vector3.Dot(point, direction) / direction.magnitude * (direction / direction.magnitude);
    }

    private void RecountLogic()
    {
        Vector3 footLeftXZ = new Vector3(leftFoot.position.x, 0, leftFoot.position.z);
        Vector3 footRightXZ = new Vector3(rightFoot.position.x, 0, rightFoot.position.z);
        Vector3 directionMove = transform.forward;
        
        //stableFoot = Vector3.Distance(animator.pivotPosition, leftFoot.position) >
        //             Vector3.Distance(animator.pivotPosition, rightFoot.position);


        L = (caltest(directionMove, footLeftXZ) -transform.position).x;
        R = (caltest(directionMove, footRightXZ) -transform.position).x;


       

        footDistance = Vector3.Distance(caltest(directionMove, footLeftXZ), caltest(directionMove, footRightXZ));

        if (footDistance > 0.05f)
        {
            forwardFoot = L < R;

        }

        float delta = Mathf.Abs(Vector3.Distance(animator.pivotPosition, leftFoot.position) -
                                Vector3.Distance(animator.pivotPosition, rightFoot.position));
        if (delta > 0.15f)
        {
            stableFoot = Vector3.Distance(animator.pivotPosition, leftFoot.position) >
                         Vector3.Distance(animator.pivotPosition, rightFoot.position);
        }
    }

    private enum MainFoot
    {
        Left = 0,
        Right = 1,
    }
}