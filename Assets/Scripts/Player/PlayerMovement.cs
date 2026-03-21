using System.Collections;
using UnityEngine;

namespace Player
{
        
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 15f; // Tốc độ xoay người
        [SerializeField] private float gravity = -20f; // Trọng lực mạnh hơn chút cho game action cảm giác chắc chắn
        [Tooltip("Độ mượt khi xoay nhân vật. Càng nhỏ xoay càng nhanh.")]
        [SerializeField] private float turnSmoothTime = 0.1f;
        
        [Header("Jump Settings")]
        [SerializeField] private float jumpHeight = 1.5f;
        
        [Header("Dash Settings")]
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;

        // State Variables
        private Vector3 velocity;
        private float verticalVelocity;
        private bool isGrounded;
        private bool isDashing;
        private float lastDashTime = -100f; // Để dash được ngay lần đầu
        private float turnSmoothVelocity; // Biến phụ dùng cho hàm SmoothDampAngle

        // References
        private CharacterController cc;
        private PlayerInput input;
        private Transform cameraTransform;
        private PlayerNormalAttack playerNormalAttack;
        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            input = GetComponent<PlayerInput>();
            cameraTransform = Camera.main.transform;
            playerNormalAttack = GetComponent<PlayerNormalAttack>();
        }
        private void Start()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }
        private void Update()
        {
            if(playerNormalAttack.IsPerformedAttack)
            {
                return;
            }
            // 1. Dash (Ưu tiên cao nhất)
            if (input.SprintTriggered && CanDash())
            {
                StartCoroutine(DashRoutine());
                input.SprintTriggered = false;
            }

            if (isDashing) return; // Nếu đang lướt thì không điều khiển di chuyển thường

            // 2. Trọng lực & Nhảy
            HandleGravityAndJump();

            // 3. Di chuyển & Xoay góc nhìn thứ 3
            HandleTPCMovement();
        }

        private void HandleGravityAndJump()
        {
            if (cc.isGrounded)
            {
                if (verticalVelocity < 0) verticalVelocity = -2f; // Giữ chặt xuống đất

                if (input.JumpTriggered)
                {
                    // v = sqrt(h * -2 * g)
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    input.JumpTriggered = false;
                }
            }

            verticalVelocity += gravity * Time.deltaTime;
        }

        private void HandleTPCMovement()
        {
            Vector2 inputDir = input.MoveInput;
            Vector3 direction = new Vector3(inputDir.x, 0f, inputDir.y).normalized;

            if (direction.magnitude >= 0.1f)
            {
                // --- LOGIC GÓC NHÌN THỨ 3 ---
                
                Vector3 moveDir = (transform.forward * inputDir.y + transform.right * inputDir.x).normalized;

                // 4. Di chuyển
                cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
            }

            // Áp dụng trọng lực (luôn luôn áp dụng kể cả khi không bấm nút di chuyển)
            cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }

        // --- DASH SYSTEM ---

        private bool CanDash()
        {
            return Time.time >= lastDashTime + dashCooldown && ! isDashing;
        }

        private IEnumerator DashRoutine()
        {
            isDashing = true;
            lastDashTime = Time.time;

            // Xác định hướng Dash
            Vector3 dashDirection;
            
            // Nếu đang di chuyển thì dash theo hướng đó, nếu đứng im thì dash thẳng về phía trước mặt
            if (input.MoveInput.sqrMagnitude > 0.01f)
            {
                // Tính lại hướng input so với camera để chính xác
                Vector3 camForward = cameraTransform.forward; camForward.y = 0; camForward.Normalize();
                Vector3 camRight = cameraTransform.right; camRight.y = 0; camRight.Normalize();
                dashDirection = (camForward * input.MoveInput.y + camRight * input.MoveInput.x).normalized;
                
                // Xoay người theo hướng dash ngay lập tức cho mượt
                transform.rotation = Quaternion.LookRotation(dashDirection);
            }
            else
            {
                dashDirection = transform.forward;
            }

            float startTime = Time.time;

            while (Time.time < startTime + dashDuration)
            {
                // Dash bỏ qua trọng lực (y = 0) để lướt qua hố hoặc tạo cảm giác bay
                cc.Move(dashDirection * dashSpeed * Time.deltaTime);
                yield return null; // Chờ frame tiếp theo
            }

            // Kết thúc Dash, reset vận tốc rơi để không bị kéo tuột xuống quá nhanh nếu dash trên không
            verticalVelocity = 0f;
            isDashing = false;
        }
    }
}