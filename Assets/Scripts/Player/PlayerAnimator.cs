using UnityEngine;

namespace Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        Animator _anim;

        // Cache parameter hashes (nhanh hơn string lookup)
        static readonly int MoveX = Animator.StringToHash("MoveX");
        static readonly int MoveY = Animator.StringToHash("MoveY");
        static readonly int Speed = Animator.StringToHash("Speed");
        static readonly int IsStrafe = Animator.StringToHash("IsStrafe");
        static readonly int IsSprint = Animator.StringToHash("IsSprint");
        static readonly int AttackIdx = Animator.StringToHash("AttackIndex");
        static readonly int TrigAttack = Animator.StringToHash("Attack");
        static readonly int TrigDodge = Animator.StringToHash("Dodge");
        static readonly int TrigParry = Animator.StringToHash("Parry");
        static readonly int IsBlock = Animator.StringToHash("IsBlock");
        static readonly int TrigHit = Animator.StringToHash("Hit");
        static readonly int TrigDeath = Animator.StringToHash("Death");
        static readonly int DeathIdx = Animator.StringToHash("DeathIndex");
        static readonly int IsStunned = Animator.StringToHash("IsStunned");

        void Awake() => _anim = GetComponentInChildren<Animator>();

        public void SetLocomotion(Vector2 input, float speed, bool strafe, bool sprinting)
        {
            _anim.SetFloat(MoveX, input.x, 0.12f, Time.deltaTime);
            _anim.SetFloat(MoveY, input.y, 0.12f, Time.deltaTime);
            _anim.SetFloat(Speed, speed, 0.12f, Time.deltaTime);
            _anim.SetBool(IsStrafe, strafe);
            _anim.SetBool(IsSprint, sprinting);
        }

        public void TriggerAttack(int comboIdx)
        {
            _anim.SetInteger(AttackIdx, comboIdx);
            _anim.SetTrigger(TrigAttack);
        }

        public void TriggerDodge(Vector2 dir)
        {
            // Optional: set dodge direction cho blend tree dodge
            _anim.SetFloat(MoveX, dir.x);
            _anim.SetFloat(MoveY, dir.y);
            _anim.SetTrigger(TrigDodge);
        }

        public void SetBlocking(bool value) => _anim.SetBool(IsBlock, value);
        public void TriggerParry() => _anim.SetTrigger(TrigParry);
        public void TriggerHitReaction() => _anim.SetTrigger(TrigHit);

        public void TriggerDeath(int deathIdx = -1)
        {
            int idx = deathIdx < 0 ? Random.Range(0, 6) : deathIdx;
            _anim.SetInteger(DeathIdx, idx);
            _anim.SetTrigger(TrigDeath);
        }

        public void SetStunned(bool value) => _anim.SetBool(IsStunned, value);

        // Gọi từ Animation Event trong clip
        public void OnRootMotionToggle(int useRoot) =>
            _anim.applyRootMotion = useRoot == 1;
    }
}