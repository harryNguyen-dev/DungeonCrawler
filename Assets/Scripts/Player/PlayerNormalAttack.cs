using UnityEngine;

namespace Player
{
    public class PlayerNormalAttack : MonoBehaviour
    {
        private int attackCombo = -1;
        private int maxAttackCombo = 3;
        public int AttackCombo { get { return attackCombo; } }

        private PlayerInput input;
        private PlayerAnimation playerAnimation;
        private bool wasAttackTriggered;
        private bool isPerformedAttack = false;
        public bool IsPerformedAttack { get { return isPerformedAttack; } }

        [SerializeField] private float attackInterval = 1f; // Mỗi 1 giây khi giữ chuột sẽ advance combo
        [SerializeField] private float comboResetDelay = 1f; // Sau 1 giây không đánh thì reset về -1
        private float attackTimer = 0f;
        private float resetTimer = 0f;


        private void Awake()
        {
            input = GetComponent<PlayerInput>();
            playerAnimation = GetComponent<PlayerAnimation>();
        }

        private void Update()
        {
            bool attackTriggered = input.AttackTriggered;

            // Vừa bấm/giữ chuột (edge: false -> true) => bắt đầu combo hoặc advance combo (cho bấm liên tiếp)
            if (attackTriggered && !wasAttackTriggered)
            {
                if (attackCombo < 0)
                    attackCombo = 0;
                else
                    attackCombo = (attackCombo + 1) % maxAttackCombo;
                attackTimer = 0f;
                resetTimer = 0f;
            }

            if (attackTriggered)
            {
                // Đang giữ chuột: mỗi attackInterval giây tăng combo
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackInterval)
                {
                    attackTimer = 0f;
                    attackCombo = (attackCombo + 1) % maxAttackCombo;
                }
                resetTimer = 0f;
            }
            else
            {
                // Không giữ chuột: sau comboResetDelay giây thì reset về -1
                resetTimer += Time.deltaTime;
                if (resetTimer >= comboResetDelay)
                {
                    attackCombo = -1;
                    resetTimer = 0f;
                }
            }

            wasAttackTriggered = attackTriggered;
            playerAnimation.SetAnimationAttackCombo(attackCombo);
        }
    }
}
