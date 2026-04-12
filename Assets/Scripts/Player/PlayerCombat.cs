using Combat;
using UnityEngine;

namespace Player
{
    public class PlayerCombat : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] HealthSystem _health;
        [SerializeField] PlayerAnimator _playerAnimator;
        [SerializeField] CharacterController _characterController;
        [SerializeField] ICombatable _combat;
        PlayerStateMachine _sm;

        [SerializeField] float _drag = 5f;
        Vector3 _impact = Vector3.zero;

        void Awake()
        {
            if (_health == null)
                _health = GetComponent<HealthSystem>();
            _sm = GetComponent<PlayerStateMachine>();
            _combat = GetComponent<ICombatable>();
        }
        void Update()
        {
            // Áp dụng lực đẩy vào CharacterController
            if (_impact.magnitude > 0.2f)
            {
                _characterController.Move(_impact * Time.deltaTime);
            }

            // Giảm dần lực đẩy (Lerp về zero)
            _impact = Vector3.Lerp(_impact, Vector3.zero, _drag * Time.deltaTime);
        }

        public void ReceiveHit(in DamageHitInfo info)
        {
            if (_combat.IsParry())
            {
                return;
            }

            if (_health != null)
                _health.TakeDamage(info.Damage);

            if (_sm != null)
                _sm.TryTransition(PlayerState.HitReaction);
            if (_playerAnimator != null)
                _playerAnimator.TriggerTakeDamage();

            if (_characterController != null)
            {
                Knockback(info);
            }
        }

        private void Knockback(DamageHitInfo info)
        {
            Vector3 dir = info.KnockbackDirection;
            dir.y = 0; // Giữ hướng đẩy nằm ngang nếu không muốn nảy lên

            if (dir.sqrMagnitude > 0.0001f)
                dir.Normalize();

            // Thay vì AddForce, chúng ta cộng dồn vào biến impact
            // Công thức: Hướng * Sức mạnh
            _impact += dir * info.KnockbackImpulse;

            // Nếu có lực bay lên (Vertical Impulse)
            if (info.VerticalImpulse > 0)
            {
                _impact.y += info.VerticalImpulse;
            }
        }
    }
}