using UnityEngine;

public class BasicAttackComboBehaviour : StateMachineBehaviour
{
    [Range(0.1f, 0.99f)]
    public float comboCheckTime = 0.75f;

    private bool checkedThisState;

    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        checkedThisState = false;
    }

    public override void OnStateUpdate(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        if (checkedThisState)
            return;

        if (stateInfo.normalizedTime >= comboCheckTime)
        {
            checkedThisState = true;

            JameeCombatInput combat =
                animator.GetComponentInParent<JameeCombatInput>();

            if (combat != null)
                combat.TryPlayQueuedBasicAttack();
        }
    }

    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        JameeCombatInput combat =
            animator.GetComponentInParent<JameeCombatInput>();

        if (combat != null)
            combat.BasicAttackAnimationFinished();
    }
}