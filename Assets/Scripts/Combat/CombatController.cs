using Core;
using UnityEngine;
using Player;
namespace Combat
{
    public class CombatController : MonoBehaviour
    {
        [Header("Settings")]
        public int maxComboCount = 4; // số combo tối đa
        [Tooltip("Sau mỗi lần bắt đầu nhát đánh, không cho hủy attack bằng di chuyển trong khoảng này (tránh giữ stick + bấm đánh bị hủy ngay).")]
        public float movementCancelLockoutAfterAttackStart = 0.12f;

        [Header("Lock-on")]
        public float lockOnRadius = 12f;
        public LayerMask enemyLayer;

        PlayerStateMachine _sm;
        PlayerAnimator _anim;
        HitboxController _hitbox;
        int _comboIndex;
        bool _attackQueued;
        float _attackMovementCancelAllowedTime;
        bool _parryInProgress;
        public Transform LockOnTarget { get; private set; }
        /// <summary>True từ khi bắt đầu parry đến khi AE_ParryEnd — dùng để khóa locomotion/IsBlock kể cả khi state machine hoặc animator lệch một frame.</summary>
        public bool ParryInProgress => _parryInProgress;
        void Awake()
        {
            _sm = GetComponent<PlayerStateMachine>();
            _anim = GetComponent<PlayerAnimator>();
            _hitbox = GetComponentInChildren<HitboxController>();
        }

        void LateUpdate()
        {
            if (!_parryInProgress) return;
            _anim.SetBlocking(true);
        }
        // ── Input từ PlayerController ──────────────────────
        public void OnAttackInput()
        {
            if (_sm.Current == PlayerState.Dead) return;

            if (_sm.Current != PlayerState.Attacking)
            {
                // Bắt đầu combo từ đầu
                _comboIndex = 0;
                ExecuteAttack();
            }
            else
            {
                if (_comboIndex < maxComboCount - 1) {
                    _attackQueued = true;   // Buffer: nhớ input
                    Debug.Log("Attack queued");
                }
            }
        }

        void ExecuteAttack()
        {
            _sm.TryTransition(PlayerState.Attacking);
            _anim.TriggerAttack(_comboIndex);
            _attackQueued = false;
            _attackMovementCancelAllowedTime = Time.time + movementCancelLockoutAfterAttackStart;
        }

        /// <summary>
        /// Hủy đòn đang swing khi có input di chuyển — đồng bộ state machine với locomotion.
        /// </summary>
        public bool TryCancelAttackForMovement()
        {
            if (_sm.Current != PlayerState.Attacking) return false;
            if (Time.time < _attackMovementCancelAllowedTime) return false;
            _attackQueued = false;
            _comboIndex = 0;
            _anim.ResetAttackLayer();
            if (_hitbox != null) _hitbox.SetActive(false);
            return true;
        }

        // ── Animation Events (gắn vào clip trong Animation window) ──
        public void AE_AttackEnd()
        {
            if (_sm.Current != PlayerState.Attacking) return;

            if (_attackQueued && _comboIndex < maxComboCount - 1)
            {
                _comboIndex++;
                ExecuteAttack();        // tiếp tục combo
            }
            else
            {
                _comboIndex = 0;
                _attackQueued = false;
                _sm.TryTransition(PlayerState.CombatIdle);
            }
        }
        public void AE_HitboxOn() => _hitbox.SetActive(true);
        public void AE_HitboxOff() => _hitbox.SetActive(false);

        // ── Block & Parry ─────────────────────────────────
        public void SetBlocking(bool value)
        {
            if (_sm.Current == PlayerState.Dead) return;

            if (value)
            {
                _anim.SetBlocking(true);
                _sm.TryTransition(PlayerState.Blocking);
                return;
            }

            if (_parryInProgress || _sm.Current == PlayerState.Parrying)
                return;

            _anim.SetBlocking(false);
            if (_sm.Current == PlayerState.Blocking)
                _sm.TryTransition(PlayerState.CombatIdle);
        }

        public void TryParry()
        {
            if (_sm.Current == PlayerState.Dead) return;
            if (_sm.Current != PlayerState.Blocking) return;
            if (!_sm.TryTransition(PlayerState.Parrying)) return;
            _parryInProgress = true;
            _anim.TriggerParry();
        }

        /// <summary>Animation Event cuối clip Parry — đồng bộ gameplay state với input block.</summary>
        public void AE_ParryEnd()
        {
            if (_sm.Current != PlayerState.Parrying)
            {
                _parryInProgress = false;
                return;
            }
            bool blockHeld = InputManager.BlockHeld;
            _anim.SetBlocking(blockHeld);
            if (blockHeld)
                _sm.TryTransition(PlayerState.Blocking);
            else
                _sm.TryTransition(PlayerState.CombatIdle);
            _parryInProgress = false;
        }

        // ── Lock-on ───────────────────────────────────────
        public void ToggleLockOn()
        {
            if (LockOnTarget != null) { LockOnTarget = null; return; }

            var hits = Physics.OverlapSphere(transform.position, lockOnRadius, enemyLayer);
            Transform best = null;
            float minDist = float.MaxValue;
            foreach (var h in hits)
            {
                float d = Vector3.Distance(transform.position, h.transform.position);
                if (d < minDist) { minDist = d; best = h.transform; }
            }
            LockOnTarget = best;
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, lockOnRadius);
            if (LockOnTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, LockOnTarget.position);
            }
        }
    }
}