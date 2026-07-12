using UnityEngine;

public class SkillComboBehaviour : StateMachineBehaviour
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

            JameeCombatInput combat = animator.GetComponent<JameeCombatInput>();

            if (combat != null)
                combat.TryPlayQueuedSkillCombo();
        }
    }
}