using UnityEngine;
using Combat;
using Interaction;

namespace Player
{
    [RequireComponent(typeof(PlayerStateMachine))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement (Souls-like)")]
        [Tooltip("Strafe / locked-on jog speed.")]
        public float walkSpeed = 2f;
        [Tooltip("Default run when not locked on.")]
        public float runSpeed = 3.4f;
        [Tooltip("Hold sprint — still gated by animator threshold.")]
        public float sprintSpeed = 4.8f;
        [Tooltip("Units/sec² toward target horizontal velocity while moving.")]
        public float moveAcceleration = 28f;
        [Tooltip("Units/sec² when releasing input or braking.")]
        public float moveDeceleration = 38f;
        [Tooltip("How fast the body turns toward move direction (unlocked).")]
        public float rotationSpeed = 5.5f;
        [Tooltip("How fast the body turns toward lock-on target.")]
        public float rotationSpeedLockOn = 7f;
        public float gravity = -20f;

        [Header("References")]
        public Transform cameraTransform;

        CharacterController _cc;
        PlayerStateMachine _sm;
        PlayerAnimator _anim;
        CombatController _combat;
        InteractionDetector _interact;

        Vector3 _velocity;           // gravity accumulation
        Vector3 _horizontalVelocity; // smoothed XZ velocity (weighty accel / decel)
        Vector2 _moveInput;
        bool _sprintInput;
        bool _blockInput;

        public bool ShouldFaceMoveDirection = true;

        bool IsLockedOn => _combat != null && _combat.LockOnTarget != null;

        float groundStickForce = -2f;
        float terminalVelocity = -50f;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _sm = GetComponent<PlayerStateMachine>();
            _anim = GetComponent<PlayerAnimator>();
            _combat = GetComponent<CombatController>();
            _interact = GetComponentInChildren<InteractionDetector>();
        }

        void OnEnable() => _sm.OnStateChanged += OnPlayerStateChanged;
        void OnDisable() => _sm.OnStateChanged -= OnPlayerStateChanged;

        void OnPlayerStateChanged(PlayerState prev, PlayerState next)
        {
            if (next == PlayerState.Attacking
                || next == PlayerState.Blocking
                || next == PlayerState.Parrying
                || next == PlayerState.HitReaction
                || next == PlayerState.Stunned)
                _horizontalVelocity = Vector3.zero;
        }

        void Update()
        {
            if (_combat != null && _combat.ParryInProgress)
            {
                FreezeLocomotionForBlockOrParry();
                ApplyGravity();
                _cc.Move(new Vector3(0f, _velocity.y, 0f) * Time.deltaTime);
                return;
            }

            switch (_sm.Current)
            {
                case PlayerState.Idle:
                case PlayerState.Moving:
                case PlayerState.Sprinting:
                case PlayerState.CombatIdle:
                    HandleLocomotion();
                    break;

                case PlayerState.Dodging:
                    ApplyGravity();
                    break;

                case PlayerState.Blocking:
                case PlayerState.Parrying:
                    FreezeLocomotionForBlockOrParry();
                    ApplyGravity();
                    _cc.Move(new Vector3(0f, _velocity.y, 0f) * Time.deltaTime);
                    break;

                case PlayerState.Attacking:
                    _horizontalVelocity = Vector3.zero;
                    FreezeLocomotionForBlockOrParry();
                    ApplyGravity();
                    _cc.Move(new Vector3(0f, _velocity.y, 0f) * Time.deltaTime);
                    break;

                case PlayerState.HitReaction:
                case PlayerState.Stunned:
                    FreezeLocomotionForBlockOrParry();
                    ApplyGravity();
                    _cc.Move(new Vector3(0f, _velocity.y, 0f) * Time.deltaTime);
                    break;

                case PlayerState.Dead:
                case PlayerState.Interacting:
                    FreezeLocomotionForBlockOrParry();
                    ApplyGravity();
                    _cc.Move(new Vector3(0f, _velocity.y, 0f) * Time.deltaTime);
                    break;
            }
        }

        void HandleLocomotion()
        {
            bool lockedOn = IsLockedOn;

            // Camera-relative movement direction (Souls-style)
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = camForward * _moveInput.y + camRight * _moveInput.x;
            if (moveDir.sqrMagnitude > 1f)
                moveDir.Normalize();

            float inputMag = _moveInput.magnitude;
            bool hasMoveInput = inputMag > 0.1f;

            float speed = !hasMoveInput ? 0f
                        : _sprintInput && !lockedOn ? sprintSpeed
                        : lockedOn ? walkSpeed
                        : runSpeed;

            // State (sprint only when not locked — matches typical souls lock strafe)
            var nextState = speed < 0.1f ? PlayerState.Idle
                          : _sprintInput && !lockedOn ? PlayerState.Sprinting
                          : lockedOn && hasMoveInput ? PlayerState.CombatIdle
                          : hasMoveInput ? PlayerState.Moving
                          : PlayerState.Idle;
            _sm.TryTransition(nextState);

            Vector3 targetHorizontal = moveDir * speed;
            float accelRate = hasMoveInput && moveDir.sqrMagnitude > 0.0001f
                ? moveAcceleration
                : moveDeceleration;
            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity,
                targetHorizontal,
                accelRate * Time.deltaTime);

            ApplyFacing(lockedOn, moveDir, hasMoveInput);

            ApplyGravity();
            Vector3 finalMove = _horizontalVelocity + _velocity;
            _cc.Move(finalMove * Time.deltaTime);

            float animSpeed = _horizontalVelocity.magnitude;
            bool sprintAnim = _sprintInput && !lockedOn && hasMoveInput;

            if (lockedOn)
            {
                // Lock-on: cần strafe/backward → project moveInput sang không gian nhân vật
                Vector3 localMove = transform.InverseTransformDirection(moveDir);
                Vector2 animInput = new Vector2(localMove.x, localMove.z);
                _anim.SetLocomotion(animInput, animSpeed, true, false);
            }
            else
            {
                // Free movement: nhân vật luôn chạy "forward" theo hướng mình đang nhìn
                // → chỉ truyền forward (0,1) scaled by speed, không có strafe
                Vector2 animInput = animSpeed > 0.1f ? Vector2.up : Vector2.zero;
                _anim.SetLocomotion(animInput, animSpeed, false, sprintAnim);
            }
        }

        void ApplyFacing(bool lockedOn, Vector3 moveDir, bool hasMoveInput)
        {
            if (!ShouldFaceMoveDirection) return;

            if (lockedOn && _combat.LockOnTarget != null)
            {
                Vector3 toTarget = _combat.LockOnTarget.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude < 0.0001f) return;
                Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized);
                float turn = rotationSpeedLockOn * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turn);
                return;
            }

            if (hasMoveInput && moveDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                float turn = rotationSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turn);
            }
        }

        void FreezeLocomotionForBlockOrParry()
        {
            _anim.SetLocomotion(Vector2.zero, 0f, IsLockedOn, false);
        }

        void ApplyGravity()
        {
            if (_cc.isGrounded && _velocity.y < 0f)
                _velocity.y = groundStickForce;
            else
                _velocity.y += gravity * Time.deltaTime;

            _velocity.y = Mathf.Max(_velocity.y, terminalVelocity);
        }

        void TryDodge()
        {
            if (_sm.Current is PlayerState.Dead or PlayerState.Stunned or PlayerState.HitReaction) return;
            _sm.TryTransition(PlayerState.Dodging);
            _anim.TriggerDodge(_moveInput);
        }
        public void SetMoveInput(Vector2 v) => _moveInput = v;
        public void SetSprintInput(bool v) => _sprintInput = v;
        public void SetBlockInput(bool v)
        {
            _blockInput = v;
            _combat.SetBlocking(v);
        }
        public void OnAttackInput() => _combat.OnAttackInput();
        public void OnDodgeInput() => TryDodge();
        public void OnParryInput() => _combat.TryParry();
        public void OnInteractInput() { }
        public void OnLockOnInput() => _combat.ToggleLockOn();
    }
}