using Core;
using UnityEngine;
using Player;
using System;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
namespace Combat
{
    public class CombatController : MonoBehaviour, ICombatEvents, ICombatable
    {
        [Header("Settings")]
        public int maxComboCount = 4; // số combo tối đa

        public List<int> DamagePerCombo;

        [Tooltip("Sau khi một đòn kết thúc: trong khoảng này bấm đánh tiếp sẽ lên đòn combo kế; quá lâu không đánh thì về đòn 0.")]
        public float comboResetTimeout = 5f;

        [Header("Lock-on")]
        public float lockOnRadius = 12f;
        public LayerMask enemyLayer;

        PlayerStateMachine _sm;
        PlayerAnimator _anim;
        HitboxController _hitbox;
        int _comboIndex;
        /// <summary>Time.time mà sau đó (khi không đang Attacking) combo memory hết — về đòn 0.</summary>
        float _comboMemoryExpireTime;
        bool _parryInProgress;
        public Transform LockOnTarget { get; private set; }

        bool isParry;

        /// <summary>True từ khi bắt đầu parry đến khi AE_ParryEnd — dùng để khóa locomotion/IsBlock kể cả khi state machine hoặc animator lệch một frame.</summary>
        public bool ParryInProgress => _parryInProgress;
        void Awake()
        {
            _sm = GetComponent<PlayerStateMachine>();
            _anim = GetComponent<PlayerAnimator>();
            _hitbox = GetComponentInChildren<HitboxController>();
        }

        void Update()
        {
            if (_comboMemoryExpireTime <= 0f) return;
            if (_sm.Current == PlayerState.Attacking) return;
            if (Time.time <= _comboMemoryExpireTime) return;
            ResetComboMemory();
        }

        void LateUpdate()
        {
            if (!_parryInProgress) return;
            _anim.SetBlocking(true);
        }
        // ── Input từ PlayerController ──────────────────────
        public void OnAttackInput()
        {
            if (_sm.Current is PlayerState.Dead or PlayerState.HitReaction or PlayerState.Stunned) return;

            if (_sm.Current != PlayerState.Attacking)
            {
                if (_comboMemoryExpireTime > 0f && Time.time > _comboMemoryExpireTime)
                    ResetComboMemory();
                ExecuteAttack();
            }
        }

        void ResetComboMemory()
        {
            _comboIndex = 0;
            _comboMemoryExpireTime = 0f;
        }

        void ExecuteAttack()
        {
            _sm.TryTransition(PlayerState.Attacking);
            _anim.TriggerAttack(_comboIndex);
        }

        // ── Animation Events (gắn vào clip trong Animation window) ──
        public void AE_AttackEnd()
        {
            if (_sm.Current != PlayerState.Attacking) return;

            int finished = _comboIndex;
            if (finished < maxComboCount - 1)
            {
                _comboIndex = finished + 1;
                _comboMemoryExpireTime = Time.time + comboResetTimeout;
            }
            else
                ResetComboMemory();

            _sm.TryTransition(PlayerState.CombatIdle);
        }
        public void AE_HitboxOn() => _hitbox.SetActive(true);
        public void AE_HitboxOff() => _hitbox.SetActive(false);

        // ── Block & Parry ─────────────────────────────────
        public void SetBlocking(bool value)
        {
            if (_sm.Current is PlayerState.Dead or PlayerState.HitReaction or PlayerState.Stunned) return;

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
            if (_sm.Current is PlayerState.Dead or PlayerState.HitReaction or PlayerState.Stunned) return;
            if (_sm.Current != PlayerState.Blocking) return;
            if (!_sm.TryTransition(PlayerState.Parrying)) return;
            _parryInProgress = true;
            _anim.TriggerParry();
        }

        /// <summary>Animation Event cuối clip Parry — đồng bộ gameplay state với input block.</summary>
        public void AE_ParryEnd()
        {
            Debug.Log("AE_ParryEnd");
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
        public void AE_HitReactionEnd()
        {
            if (_sm == null || _sm.Current != PlayerState.HitReaction) return;
            bool locked = LockOnTarget != null;
            if(_parryInProgress)
            {
                _parryInProgress = false;
                _anim.SetBlocking(false);
            }
            _sm.TryTransition(locked ? PlayerState.CombatIdle : PlayerState.Idle);
        }
        // ── Lock-on ───────────────────────────────────────
        public void ToggleLockOn()
        {
            if (_sm.Current is PlayerState.HitReaction or PlayerState.Stunned) return;

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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, lockOnRadius);
            if (LockOnTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, LockOnTarget.position);
            }
        }

        public int GetDamageThisAttack()
        {
            return DamagePerCombo[_comboIndex];
        }

        public bool IsParry()
        {
            return isParry;
        }

        public void AE_ParryStart()
        {
            isParry = true;
        }

        public void AE_ParryStop()
        {
            isParry = false;
        }
    }
}