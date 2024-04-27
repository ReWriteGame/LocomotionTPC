using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSM : StateMachineBehaviour
{
    public float angleFix;
    public float test;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        angleFix = animator.GetFloat("Angle");
        test = animator.GetFloat("Angle");
        animator.SetFloat("AngleFix", angleFix);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        angleFix = 0;
    }

}
