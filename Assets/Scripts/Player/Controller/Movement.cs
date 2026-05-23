using UnityEngine;

namespace PlayerController
{
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerEvents))]
    public class Movement : MonoBehaviour
    {
        private PlayerStats playerStats;
        private PlayerEvents events;
        private float moveSpeed;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
            events = GetComponent<PlayerEvents>();
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
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }
    }
}
