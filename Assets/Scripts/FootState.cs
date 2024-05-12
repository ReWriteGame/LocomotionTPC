using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootState : MonoBehaviour
{
   [SerializeField] private Transform leftFoot;
   [SerializeField] private Transform rightFoot;
   [SerializeField] private Transform centerPelvis;
   [SerializeField] private bool mainFoot;
   [SerializeField] private Animator animator;
   [SerializeField] public float footDistance;
   [SerializeField] public float footDistance2;
   [SerializeField] public float footDistance3;

   public float x1;
   public float x2;
   public Action OnChangeMainFoot;
   public Action OnSetMainFootLeft;
   public Action OnSetMainFootRight;

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
   
   private void RecountLogic()
   {
      mainFoot = Vector3.Distance(animator.pivotPosition, leftFoot.position) >
                 Vector3.Distance(animator.pivotPosition, rightFoot.position);
      
      footDistance = Vector3.Distance(leftFoot.position-new Vector3(0,0,.22f), rightFoot.position + new Vector3(0,0,.22f));
      footDistance = Vector3.Distance(leftFoot.position, rightFoot.position);
      //footDistance = Vector3.Distance(new Vector3(leftFoot.position.x,0,leftFoot.position.z - .22f), new Vector3(rightFoot.position.x,0,rightFoot.position.z + .22f));
      footDistance2 = Mathf.Abs(leftFoot.position.x -rightFoot.position.x);


      
      x1 = (CalcProjection(transform.position, leftFoot.position, transform.forward) - transform.position).x;
      x2 = (CalcProjection(transform.position, rightFoot.position, transform.forward) - transform.position).x;

      footDistance2 = Mathf.Abs(x1) + Mathf.Abs(x2);
      
      if (Input.GetKeyUp(KeyCode.W)) footDistance3 = footDistance - initialDistance;
   }

   private enum  MainFoot
   {
      Left = 0,
      Right = 1,
   }
}
