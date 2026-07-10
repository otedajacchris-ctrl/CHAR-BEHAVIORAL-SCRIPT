using UnityEngine;

public class JameeCombatInput : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Basic Attack Combo")]
    [Tooltip("Time allowed to continue Attack1 → Attack2 → etc. This does not control movement.")]
    [SerializeField] private float comboResetTime = 3f;
    [SerializeField] private int maxBasicAttackCombo = 5;

    [Header("Skill 1")]
    [SerializeField] private int maxSkill1Combo = 1;
    [SerializeField] private float skill1ComboResetTime = 3f;
    [SerializeField] private float skill1Cooldown = 1.2f;
    [SerializeField] private float skill1AnimationTime = 1.2f;

    [Header("Skill 2")]
    [SerializeField] private int maxSkill2Combo = 1;
    [SerializeField] private float skill2ComboResetTime = 3f;
    [SerializeField] private float skill2Cooldown = 1.2f;
    [SerializeField] private float skill2AnimationTime = 1.2f;

    [Header("Ultimate")]
    [SerializeField] private int maxUltimateCombo = 1;
    [SerializeField] private float ultimateComboResetTime = 3f;
    [SerializeField] private float ultimateCooldown = 1.8f;
    [SerializeField] private float ultimateAnimationTime = 1.8f;

    [Header("Dash")]
    [SerializeField] private float dashTime = 0.35f;

    public bool IsUsingAction { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool FreezeCameraFollow { get; private set; }

    private float dashEndTime;

    // Basic-attack queue.
    private bool nextBasicAttackQueued;
    private int queuedAttackStep;

    // True while Skill1, Skill2, or Ultimate is playing.
    private bool isSkillPlaying;

    private int attackStep;
    private float lastAttackInputTime;

    private int skill1Step;
    private float lastSkill1InputTime;
    private float skill1ReadyTime;

    private int skill2Step;
    private float lastSkill2InputTime;
    private float skill2ReadyTime;

    private int ultimateStep;
    private float lastUltimateInputTime;
    private float ultimateReadyTime;

    void Update()
    {
        if (IsDashing && Time.time >= dashEndTime)
        {
            IsDashing = false;
            IsUsingAction = false;
            FreezeCameraFollow = false;
        }

        // Skills block combat inputs until SkillFinishedBehaviour says
        // the real skill animation state has ended.
        // WASD is still handled by JameeMovement.
        if (isSkillPlaying)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
            Skill1();

        if (Input.GetKeyDown(KeyCode.E))
            Skill2();

        if (Input.GetKeyDown(KeyCode.R))
            Ultimate();

        if (Input.GetMouseButtonDown(0))
            HandleBasicAttack();

        if (Input.GetMouseButtonDown(1))
            StartDash();

        if (Input.GetKeyDown(KeyCode.Space))
            animator.SetTrigger("Jump");
    }

    void HandleBasicAttack()
    {
        // More than comboResetTime seconds without clicking:
        // next click starts Attack1.
        if (Time.time - lastAttackInputTime > comboResetTime)
        {
            attackStep = 0;
            nextBasicAttackQueued = false;
        }

        lastAttackInputTime = Time.time;

        // Current attack is still playing:
        // store ONE next attack and never cut the current animation.
        if (IsAttacking)
        {
            if (!nextBasicAttackQueued && attackStep < maxBasicAttackCombo)
            {
                nextBasicAttackQueued = true;
                queuedAttackStep = attackStep + 1;
            }

            return;
        }

        // Final attack ended. New click begins a fresh combo.
        if (attackStep >= maxBasicAttackCombo)
        {
            attackStep = 0;
        }

        attackStep++;
        StartAttackAnimation("Attack" + attackStep);
    }

    // Called near the end of Attack1–Attack4 by BasicAttackComboBehaviour.
    // This sends the next trigger before the current attack exits,
    // so Animator can go directly Attack1 → Attack2 without visiting Idle.
    public void TryPlayQueuedBasicAttack()
    {
        if (!nextBasicAttackQueued)
            return;

        if (queuedAttackStep > maxBasicAttackCombo)
            return;

        nextBasicAttackQueued = false;
        attackStep = queuedAttackStep;

        animator.SetTrigger("Attack" + attackStep);
    }

    // Called when an Attack state exits.
    public void BasicAttackAnimationFinished()
    {
        if (isSkillPlaying)
            return;

        IsAttacking = false;

        // If no next attack was triggered, the Animator returns normally to Idle.
        // Do not trigger another attack here: it was already triggered early
        // by BasicAttackComboBehaviour.
        nextBasicAttackQueued = false;
    }

    void Skill1()
    {
        if (Time.time < skill1ReadyTime)
            return;

        if (Time.time - lastSkill1InputTime > skill1ComboResetTime)
            skill1Step = 0;

        if (skill1Step >= maxSkill1Combo)
            skill1Step = 0;

        skill1Step++;
        lastSkill1InputTime = Time.time;
        skill1ReadyTime = Time.time + skill1Cooldown;

        string triggerName = GetComboTriggerName("Skill1", skill1Step, maxSkill1Combo);
        StartSkillAnimation(triggerName);
    }

    void Skill2()
    {
        if (Time.time < skill2ReadyTime)
            return;

        if (Time.time - lastSkill2InputTime > skill2ComboResetTime)
            skill2Step = 0;

        if (skill2Step >= maxSkill2Combo)
            skill2Step = 0;

        skill2Step++;
        lastSkill2InputTime = Time.time;
        skill2ReadyTime = Time.time + skill2Cooldown;

        string triggerName = GetComboTriggerName("Skill2", skill2Step, maxSkill2Combo);
        StartSkillAnimation(triggerName);
    }

    void Ultimate()
    {
        if (Time.time < ultimateReadyTime)
            return;

        if (Time.time - lastUltimateInputTime > ultimateComboResetTime)
            ultimateStep = 0;

        if (ultimateStep >= maxUltimateCombo)
            ultimateStep = 0;

        ultimateStep++;
        lastUltimateInputTime = Time.time;
        ultimateReadyTime = Time.time + ultimateCooldown;

        string triggerName = GetComboTriggerName("Ultimate", ultimateStep, maxUltimateCombo);
        StartSkillAnimation(triggerName);
    }

    string GetComboTriggerName(string baseTriggerName, int step, int maxCombo)
    {
        if (maxCombo == 1)
            return baseTriggerName;

        return baseTriggerName + "_" + step;
    }

    void StartAttackAnimation(string triggerName)
    {
        IsAttacking = true;
        animator.SetTrigger(triggerName);
    }

    // Skills end through SkillFinishedBehaviour, not a timer.
    void StartSkillAnimation(string triggerName)
    {
        isSkillPlaying = true;
        IsAttacking = true;
        animator.SetTrigger(triggerName);
    }

    void StartDash()
    {
        IsUsingAction = true;
        IsDashing = true;
        FreezeCameraFollow = false;

        dashEndTime = Time.time + dashTime;
        animator.SetTrigger("Dash");
    }

    // Called by SkillFinishedBehaviour when a real skill state exits.
    public void SkillAnimationFinished()
    {
        isSkillPlaying = false;
        IsAttacking = false;
    }

    public void FreezeCamera()
    {
        FreezeCameraFollow = true;
    }

    public void UnfreezeCamera()
    {
        FreezeCameraFollow = false;
    }

    public void OnFootstep()
    {
    }
}