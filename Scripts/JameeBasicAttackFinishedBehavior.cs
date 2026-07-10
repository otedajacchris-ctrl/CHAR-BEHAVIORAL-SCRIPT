using UnityEngine;

public class BasicAttackFinishedBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        JameeCombatInput combat = animator.GetComponentInParent<JameeCombatInput>();

        if (combat != null)
        {
            combat.BasicAttackAnimationFinished();
        }
    }
}