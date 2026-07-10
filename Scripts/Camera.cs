using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -5f);
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float mouseSensitivity = 150f;
    [SerializeField] private float autoTurnSpeed = 2f;
    [SerializeField] private JameeCombatInput combatInput;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 8f;
    [SerializeField] private float zoomSmoothTime = 0.12f;

    private float yaw;
    private float pitch = 15f;

    private float currentZoom;
    private float targetZoom;
    private float zoomVelocity;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // Start at the distance already set in Offset Z.
        currentZoom = Mathf.Abs(offset.z);
        targetZoom = currentZoom;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Mouse can always rotate the camera.
        yaw += mouseX * mouseSensitivity * Time.deltaTime;
        pitch -= mouseY * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -30f, 60f);

        // Mouse wheel: scroll up = closer, scroll down = farther.
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        targetZoom -= scroll * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        // Smooth zoom instead of snapping.
        currentZoom = Mathf.SmoothDamp(
            currentZoom,
            targetZoom,
            ref zoomVelocity,
            zoomSmoothTime
        );

        // Only auto-turn toward Jamee when camera follow is not frozen.
        if (!combatInput.FreezeCameraFollow && Mathf.Abs(mouseX) < 0.01f)
        {
            float targetYaw = target.eulerAngles.y;
            yaw = Mathf.LerpAngle(yaw, targetYaw, autoTurnSpeed * Time.deltaTime);
        }

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Same offset as before, except Z changes with mouse-wheel zoom.
        Vector3 zoomedOffset = new Vector3(
            offset.x,
            offset.y,
            -currentZoom
        );

        Vector3 wantedPosition = target.position + transform.rotation * zoomedOffset;

        // During Ultimate frames 75–150:
        // camera can rotate with mouse, but does not chase Jamee's position.
        if (!combatInput.FreezeCameraFollow)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                wantedPosition,
                followSpeed * Time.deltaTime
            );
        }
    }
}
