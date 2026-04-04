using UnityEngine;
using Player;
namespace Combat
{
    public class CombatController : MonoBehaviour
    {
        [Header("Settings")]
        public float comboResetTime = 1.2f;
        public int maxComboRight = 4;

        [Header("Lock-on")]
        public float lockOnRadius = 12f;
        public LayerMask enemyLayer;

        PlayerStateMachine _sm;
        PlayerAnimator _anim;
        HitboxController _hitbox;
        int _comboIndex;
        bool _canCombo;
        bool _attackQueued;
        float _comboTimer;
        public Transform LockOnTarget { get; private set; }
        void Awake()
        {
            _sm = GetComponent<PlayerStateMachine>();
            _anim = GetComponent<PlayerAnimator>();
            _hitbox = GetComponentInChildren<HitboxController>();
        }
        void Update()
        {
            // Reset combo nếu idle quá lâu
            if (_comboTimer > 0)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0) _comboIndex = 0;
            }
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
            else if (_canCombo)
            {
                _comboIndex = Mathf.Min(_comboIndex + 1, maxComboRight - 1);
                ExecuteAttack();
            }
            else
            {
                _attackQueued = true;   // Buffer: nhớ input
            }
        }

        void ExecuteAttack()
        {
            _sm.TryTransition(PlayerState.Attacking);
            _anim.TriggerAttack(_comboIndex);
            _canCombo = false;
            _attackQueued = false;
            _comboTimer = comboResetTime;
        }

        // ── Animation Events (gắn vào clip trong Animation window) ──
        public void AE_ComboWindowOpen() => _canCombo = true;
        public void AE_ComboWindowClose()
        {
            _canCombo = false;
            if (_attackQueued)
            {
                _comboIndex = Mathf.Min(_comboIndex + 1, maxComboRight - 1);
                ExecuteAttack();
            }
        }
        public void AE_AttackEnd()
        {
            _comboIndex = 0;
            _sm.TryTransition(PlayerState.CombatIdle);
        }
        public void AE_HitboxOn() => _hitbox.SetActive(true);
        public void AE_HitboxOff() => _hitbox.SetActive(false);

        // ── Block & Parry ─────────────────────────────────
        public void SetBlocking(bool value)
        {
            if (_sm.Current == PlayerState.Dead) return;
            _anim.SetBlocking(value);
            if (value) _sm.TryTransition(PlayerState.Blocking);
            else if (_sm.Current == PlayerState.Blocking)
                _sm.TryTransition(PlayerState.CombatIdle);
        }

        public void TryParry()
        {
            if (_sm.Current != PlayerState.Blocking) return;
            _sm.TryTransition(PlayerState.Parrying);
            _anim.TriggerParry();
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