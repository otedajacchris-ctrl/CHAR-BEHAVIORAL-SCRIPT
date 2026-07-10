using UnityEngine;

public class JameeMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float turnSpeed = 12f;
    [SerializeField] private float actionTurnSpeed = 2f;

    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private JameeCombatInput combatInput;

    private CharacterController controller;

    private bool walkMode;
    private bool sprintMode;
    private bool wasActuallySprinting;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            walkMode = !walkMode;

            if (walkMode)
                sprintMode = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            sprintMode = !sprintMode;

            if (sprintMode)
                walkMode = false;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;

        Vector3 cameraForward = cameraTarget.forward;
        Vector3 cameraRight = cameraTarget.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 move = cameraForward * vertical + cameraRight * horizontal;
        bool isMoving = move.sqrMagnitude > 0.01f;

        bool isDashing = combatInput.IsDashing;
        bool isAttacking = combatInput.IsAttacking;

        // During attacks, movement animation booleans stay off.
        // This prevents Run/Walk/Sprint from interrupting attack states.
        bool canMoveNormally = !isAttacking && !isDashing;

        bool isSprinting = isMoving && sprintMode && canMoveNormally;
        bool isWalking = isMoving && walkMode && !isSprinting && canMoveNormally;
        bool isRunning = isMoving && !walkMode && !isSprinting && canMoveNormally;

        if (wasActuallySprinting && !isMoving && canMoveNormally)
        {
            animator.SetTrigger("SprintStop");
            sprintMode = false;
        }

        wasActuallySprinting = isSprinting;

        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsSprinting", isSprinting);

        if (!isMoving)
            return;

        move.Normalize();

        // Dash: WASD cannot move or rotate Jamee.
        if (isDashing)
            return;

        // Normal mode = fast turning.
        // Attack/skill/ultimate mode = slow rotation only.
        float currentTurnSpeed = isAttacking ? actionTurnSpeed : turnSpeed;

        Quaternion targetRotation = Quaternion.LookRotation(move);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            currentTurnSpeed * Time.deltaTime
        );

        // Attack/skill/ultimate: rotation works, position stays still.
        if (isAttacking)
            return;

        float currentSpeed = runSpeed;

        if (isWalking)
            currentSpeed = walkSpeed;
        else if (isSprinting)
            currentSpeed = sprintSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);
    }
}