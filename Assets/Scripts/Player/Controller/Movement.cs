using UnityEngine;

namespace PlayerController
{
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerEvents))]
    [RequireComponent(typeof(Rotate))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class Movement : MonoBehaviour
    {
        private PlayerStats playerStats;
        private PlayerEvents events;
        private PlayerAnimation playerAnimation;
        private Rotate playerRotate;
        private CapsuleCollider bodyCollider;
        private Rigidbody bodyRigidbody;
        private float moveSpeed;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
            events = GetComponent<PlayerEvents>();
            playerAnimation = GetComponent<PlayerAnimation>();
            playerRotate = GetComponent<Rotate>();
            bodyCollider = GetComponent<CapsuleCollider>();
            bodyRigidbody = GetComponent<Rigidbody>();
            ConfigureBodyRigidbody();
        }

        private void ConfigureBodyRigidbody()
        {
            if (bodyRigidbody == null)
                return;

            bodyRigidbody.isKinematic = true;
            bodyRigidbody.useGravity = false;
            bodyRigidbody.constraints =
                RigidbodyConstraints.FreezeRotation |
                RigidbodyConstraints.FreezePositionY;
        }

        private void OnEnable()
        {
            if (events == null)
            {
                events = GetComponent<PlayerEvents>();
            }

            if (events != null)
            {
                events.OnIncreaseMoveSpeed += ModifyMoveSpeed;
            }
        }

        private void Start()
        {
            if (playerStats != null)
            {
                moveSpeed = playerStats.GetMoveSpeed();
            }
        }

        private void OnDisable()
        {
            if (events != null)
            {
                events.OnIncreaseMoveSpeed -= ModifyMoveSpeed;
            }
        }

        private void ModifyMoveSpeed(float moveSpeed)
        {
            this.moveSpeed = moveSpeed;
        }

        private bool movementEnabled = true;

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;
            if (!movementEnabled)
                playerAnimation?.SetSpeed(0f);
        }

        private void Update()
        {
            if (!movementEnabled) return;
            Move();
        }

        private void LateUpdate()
        {
            LockToGroundPlane();
        }

        private void LockToGroundPlane()
        {
            var pos = transform.position;
            if (Mathf.Abs(pos.y) > 0.001f)
            {
                pos.y = 0f;
                transform.position = pos;
            }
        }

        private void Move()
        {
            Vector2 inputVector = InputManager.Instance.GetMovementVector();
            Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

            // Cũ: chỉ di chuyển, không xoay theo WASD
            // transform.position += moveDir * moveSpeed * Time.deltaTime;

            if (moveDir.sqrMagnitude > 0.0001f)
            {
                var delta = moveDir.normalized * moveSpeed * Time.deltaTime;
                transform.position = PlayerMovementCollision.ResolveMovement(
                    transform.position,
                    delta,
                    bodyCollider,
                    bodyCollider);
                playerRotate?.FaceMovementDirection(moveDir);
            }

            playerAnimation.SetSpeed(moveDir.magnitude);
        }
    }
}
