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
    [SerializeField] private int maxSkill2Combo = 3;
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

    [Tooltip("How many dashes Jamee can do before waiting for recharge.")]
    [SerializeField] private int maxDashCharges = 2;

    [Tooltip("Each used dash charge returns after this many seconds.")]
    [SerializeField] private float dashChargeCooldown = 1f;

    [Range(0.1f, 0.99f)]
    [SerializeField] private float skillComboCheckTime = 0.75f;

    public bool IsUsingAction { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool FreezeCameraFollow { get; private set; }
    public int CurrentDashCharges { get; private set; }

    private float dashEndTime;
    private float[] dashChargeReadyTimes;

    private bool basicAttackPlaying;
    private bool nextBasicAttackQueued;
    private int queuedAttackStep;

    private bool isSkillPlaying;
    private string activeSkillName = "";

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

    void Awake()
    {
        CurrentDashCharges = maxDashCharges;
        dashChargeReadyTimes = new float[maxDashCharges];
    }

    void Update()
    {
        UpdateDashCharges();

        if (IsDashing && Time.time >= dashEndTime)
        {
            IsDashing = false;
            IsUsingAction = false;
            FreezeCameraFollow = false;
        }

        if (isSkillPlaying)
            return;

        if (Input.GetMouseButtonDown(0))
            HandleBasicAttack();

        if (Input.GetKeyDown(KeyCode.Q))
            Skill1();

        if (Input.GetKeyDown(KeyCode.E))
            Skill2();

        if (Input.GetKeyDown(KeyCode.R))
            Ultimate();

        if (Input.GetMouseButtonDown(1))
            StartDash();

        if (Input.GetKeyDown(KeyCode.Space))
            animator.SetTrigger("Jump");
    }

    void UpdateDashCharges()
    {
        for (int i = 0; i < dashChargeReadyTimes.Length; i++)
        {
            if (dashChargeReadyTimes[i] > 0f && Time.time >= dashChargeReadyTimes[i])
            {
                dashChargeReadyTimes[i] = 0f;
                CurrentDashCharges = Mathf.Min(CurrentDashCharges + 1, maxDashCharges);
            }
        }
    }

    void HandleBasicAttack()
    {
        if (Time.time - lastAttackInputTime > comboResetTime)
        {
            attackStep = 0;
            nextBasicAttackQueued = false;
            queuedAttackStep = 0;
        }

        lastAttackInputTime = Time.time;

        if (basicAttackPlaying)
        {
            if (!nextBasicAttackQueued && attackStep < maxBasicAttackCombo)
            {
                nextBasicAttackQueued = true;
                queuedAttackStep = attackStep + 1;
            }
            return;
        }

        if (attackStep >= maxBasicAttackCombo)
            attackStep = 0;

        attackStep++;
        StartBasicAttackAnimation("Attack" + attackStep);
    }

    public void TryPlayQueuedBasicAttack()
    {
        if (!basicAttackPlaying)
            return;

        if (!nextBasicAttackQueued || queuedAttackStep > maxBasicAttackCombo)
            return;

        nextBasicAttackQueued = false;
        attackStep = queuedAttackStep;
        queuedAttackStep = 0;

        animator.SetTrigger("Attack" + attackStep);
    }

    public void BasicAttackAnimationFinished()
    {
        if (isSkillPlaying)
            return;

        basicAttackPlaying = false;
        nextBasicAttackQueued = false;
        queuedAttackStep = 0;
        RefreshCombatState();
    }

    void Skill1()
    {
        HandleSkillInput(
            "Skill1",
            maxSkill1Combo,
            skill1ComboResetTime,
            skill1Cooldown,
            ref skill1Step,
            ref lastSkill1InputTime,
            ref skill1ReadyTime
        );
    }

    void Skill2()
    {
        HandleSkillInput(
            "Skill2",
            maxSkill2Combo,
            skill2ComboResetTime,
            skill2Cooldown,
            ref skill2Step,
            ref lastSkill2InputTime,
            ref skill2ReadyTime
        );
    }

    void Ultimate()
    {
        HandleSkillInput(
            "Ultimate",
            maxUltimateCombo,
            ultimateComboResetTime,
            ultimateCooldown,
            ref ultimateStep,
            ref lastUltimateInputTime,
            ref ultimateReadyTime
        );
    }

    void HandleSkillInput(
        string skillName,
        int maxCombo,
        float comboResetTimeValue,
        float cooldown,
        ref int step,
        ref float lastInputTime,
        ref float readyTime)
    {
        if (step > 0 && Time.time - lastInputTime > comboResetTimeValue)
            step = 0;

        if (step == 0 && Time.time < readyTime)
            return;

        if (step > 0 && Time.time - lastInputTime <= comboResetTimeValue && step < maxCombo)
            step++;
        else
            step = 1;

        lastInputTime = Time.time;

        StartSkillAnimation(GetComboTriggerName(skillName, step, maxCombo), skillName);
    }

    public void TryPlayQueuedSkillCombo()
    {
        // This version does not auto-chain during animation.
        // Skill combo continues by pressing the same skill again within the reset window.
    }

    string GetComboTriggerName(string baseTriggerName, int step, int maxCombo)
    {
        return maxCombo == 1 ? baseTriggerName : baseTriggerName + "_" + step;
    }

    void StartBasicAttackAnimation(string triggerName)
    {
        basicAttackPlaying = true;
        RefreshCombatState();
        animator.SetTrigger(triggerName);
    }

    void StartSkillAnimation(string triggerName, string skillName)
    {
        isSkillPlaying = true;
        activeSkillName = skillName;
        IsAttacking = true;
        IsUsingAction = true;
        animator.SetTrigger(triggerName);
    }

    void StartDash()
    {
        if (CurrentDashCharges <= 0)
            return;

        for (int i = 0; i < dashChargeReadyTimes.Length; i++)
        {
            if (dashChargeReadyTimes[i] == 0f)
            {
                dashChargeReadyTimes[i] = Time.time + dashChargeCooldown;
                CurrentDashCharges--;
                break;
            }
        }

        basicAttackPlaying = false;
        nextBasicAttackQueued = false;
        queuedAttackStep = 0;

        IsUsingAction = true;
        IsDashing = true;
        FreezeCameraFollow = false;
        dashEndTime = Time.time + dashTime;
        animator.SetTrigger("Dash");
    }

    public void SkillAnimationFinished()
    {
        if (!isSkillPlaying)
            return;

        switch (activeSkillName)
        {
            case "Skill1":
                FinishSkillStage(ref skill1Step, ref lastSkill1InputTime, ref skill1ReadyTime, skill1Cooldown, maxSkill1Combo);
                break;

            case "Skill2":
                FinishSkillStage(ref skill2Step, ref lastSkill2InputTime, ref skill2ReadyTime, skill2Cooldown, maxSkill2Combo);
                break;

            case "Ultimate":
                FinishSkillStage(ref ultimateStep, ref lastUltimateInputTime, ref ultimateReadyTime, ultimateCooldown, maxUltimateCombo);
                break;
        }

        isSkillPlaying = false;
        activeSkillName = "";
        IsAttacking = false;
        RefreshCombatState();
    }

    void FinishSkillStage(
        ref int step,
        ref float lastInputTime,
        ref float readyTime,
        float cooldown,
        int maxCombo)
    {
        if (step >= maxCombo)
        {
            readyTime = Time.time + cooldown;
            step = 0;
        }

        lastInputTime = Time.time;
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

    void RefreshCombatState()
    {
        IsAttacking = basicAttackPlaying || isSkillPlaying;
        IsUsingAction = IsDashing || IsAttacking;
    }
}
