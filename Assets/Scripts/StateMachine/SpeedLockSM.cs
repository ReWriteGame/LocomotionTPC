using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SpeedLockSM : StateMachineBehaviour
{
    public float speedFix;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        speedFix = animator.GetFloat("Speed");
        animator.SetFloat("SpeedFix", speedFix);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        speedFix = 0;
    }
}