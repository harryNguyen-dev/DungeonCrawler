using UnityEngine;

namespace Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        private int MoveXHash;
        private int MoveYHash;
        private int AttackComboHash;



        private float currentMoveX;
        private float currentMoveY;
        private float smoothTime = 0.1f;


        private Animator animator;
        private PlayerInput input;
        private PlayerNormalAttack playerNormalAttack;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            input = GetComponent<PlayerInput>();
            playerNormalAttack = GetComponent<PlayerNormalAttack>();

            MoveXHash = Animator.StringToHash("MoveX");
            MoveYHash = Animator.StringToHash("MoveY");
            AttackComboHash = Animator.StringToHash("AttackCombo");
        }

        private void Update()
        {
            Vector2 inputDir = input.MoveInput;
            SetAnimationMove(inputDir);
        }
        private void SetAnimationMove(Vector2 inputDir)
        {
            currentMoveX = Mathf.Lerp(currentMoveX, inputDir.x, smoothTime);
            currentMoveY = Mathf.Lerp(currentMoveY, inputDir.y, smoothTime);

            Mathf.Clamp01(currentMoveX);
            Mathf.Clamp01(currentMoveY);
            animator.SetFloat(MoveXHash, currentMoveX);
            animator.SetFloat(MoveYHash, currentMoveY);
        }
    
        // -1 is no attack combo
        // 0 is attack combo 1
        // 1 is attack combo 2
        // 2 is attack combo 3
        public void SetAnimationAttackCombo(int attackCombo)
        {
            animator.SetInteger(AttackComboHash, attackCombo);
        }
    }
}