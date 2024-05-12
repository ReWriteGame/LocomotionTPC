using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ParamSetSM : StateMachineBehaviour
{
    public string targetStateName;
    public string outStateName;
    public float stateValue;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        stateValue = animator.GetFloat(targetStateName);
        animator.SetFloat(outStateName, stateValue);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        stateValue = 0;
    }
}