using UnityEngine;

namespace PlayerController
{

    public class Movement : MonoBehaviour
    {
        private PlayerStats playerStats;
        private PlayerEvents events;
        private float moveSpeed;

        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            events = GetComponent<PlayerEvents>();
            moveSpeed = playerStats.GetMoveSpeed();
            events.OnIncreaseMoveSpeed += ModifyMoveSpeed;
        }
        private void OnDestroy()
        {
            events.OnIncreaseMoveSpeed -= ModifyMoveSpeed;
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