using UnityEngine;

public class SkillFinishedBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        JameeCombatInput combat = animator.GetComponent<JameeCombatInput>();

        if (combat != null)
            combat.SkillAnimationFinished();
    }
}