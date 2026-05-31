using UnityEngine;

namespace PlayerController
{
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerEvents))]
    [RequireComponent(typeof(Rotate))]
    public class Movement : MonoBehaviour
    {
        private PlayerStats playerStats;
        private PlayerEvents events;
        private PlayerAnimation playerAnimation;
        private Rotate playerRotate;
        private float moveSpeed;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
            events = GetComponent<PlayerEvents>();
            playerAnimation = GetComponent<PlayerAnimation>();
            playerRotate = GetComponent<Rotate>();
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

        private void ModifyMoveSpeed(int moveSpeed)
        {
            this.moveSpeed = moveSpeed;
        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            Vector2 inputVector = InputManager.Instance.GetMovementVector();
            Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

            // Cũ: chỉ di chuyển, không xoay theo WASD
            // transform.position += moveDir * moveSpeed * Time.deltaTime;

            if (moveDir.sqrMagnitude > 0.0001f)
            {
                transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
                playerRotate?.FaceMovementDirection(moveDir);
            }

            playerAnimation.SetSpeed(moveDir.magnitude);
        }
    }
}
