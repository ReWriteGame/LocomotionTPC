using UnityEngine;

public class AngleLockSM : StateMachineBehaviour
{
    public float angleFix;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        angleFix = animator.GetFloat("Angle");
        animator.SetFloat("AngleFix", angleFix);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        angleFix = 0;
    }
}
