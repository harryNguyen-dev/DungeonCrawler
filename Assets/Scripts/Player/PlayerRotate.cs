using UnityEngine;

namespace Player
{
    /// <summary>
    /// Handles camera pitch/yaw rotation for Third-Person Camera with Cinemachine.
    /// Attach this to the Player GameObject.
    /// In Cinemachine FreeLook or Virtual Camera, set Follow & LookAt to the CameraTarget child.
    /// </summary>
    public class PlayerRotate : MonoBehaviour
    {
        [Header("Camera Target")]
        [Tooltip("Empty GameObject child of Player that Cinemachine will Follow/LookAt.")]
        [SerializeField] private Transform cameraTarget;

        [Header("Sensitivity")]
        [SerializeField] private float horizontalSensitivity = 1f;
        [SerializeField] private float verticalSensitivity = 1f;

        [Header("Vertical Clamp (degrees)")]
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 70f;

        [Header("Options")]
        [SerializeField] private bool invertY = false;
        [SerializeField] private bool lockCursorOnStart = true;

        // Current rotation angles for the camera target
        private float yaw;   // horizontal — rotates around Y axis
        private float pitch; // vertical   — rotates around X axis

        private PlayerInput input;

        private void Awake()
        {
            input = GetComponent<PlayerInput>();

            // Auto-create a CameraTarget child if none assigned
            if (cameraTarget == null)
            {
                GameObject target = new GameObject("CameraTarget");
                target.transform.SetParent(transform);
                target.transform.localPosition = new Vector3(0f, 1.5f, 0f); // eye height
                cameraTarget = target.transform;
                Debug.LogWarning("[PlayerRotate] CameraTarget not assigned — created automatically. " +
                                 "Assign it to your Cinemachine Virtual Camera's Follow & LookAt fields.");
            }

            // Seed angles from current transform so camera doesn't snap on first frame
            yaw   = transform.eulerAngles.y;
            pitch = cameraTarget.localEulerAngles.x;
        }

        private void Start()
        {
            if (lockCursorOnStart)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible   = false;
            }
        }

        private void LateUpdate()
        {
            RotateCamera();
            RotateBody();
        }

        private void RotateCamera()
        {
            // lookInput is supplied every frame by PlayerInput via the Input System
            Vector2 look = input.LookInput;

            if (look.sqrMagnitude < 0.01f) return;

            float deltaYaw   =  look.x * horizontalSensitivity;
            float deltaPitch = (invertY ? look.y : -look.y) * verticalSensitivity;

            yaw   += deltaYaw;
            pitch  = Mathf.Clamp(pitch + deltaPitch, minPitch, maxPitch);

            // Only pitch the camera target (up/down); yaw is owned by the character body
            // so horizontal aim always matches where the player faces.
            cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
        private void RotateBody()
        {
            // Strafe: body luôn nhìn theo yaw camera, bất kể input di chuyển
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
        // ── Public helpers ──────────────────────────────────────────────

        /// <summary>Snap camera yaw to match current body facing. Call after teleport / respawn.</summary>
        public void SyncYawToBody() => yaw = transform.eulerAngles.y;

        public void SetCursorLock(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible   = !locked;
        }
    }
}