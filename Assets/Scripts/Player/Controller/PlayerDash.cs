using Core;
using Cysharp.Threading.Tasks;
using Global;
using SO;
using UnityEngine;

namespace PlayerController
{
    public class PlayerDash : MonoBehaviour
    {
        [SerializeField] private DashConfigSO dashConfig;

        private Movement movement;
        private Health health;
        private PlayerAnimation playerAnimation;
        private float lastDashTime = -999f;
        private bool isDashing;

        public bool IsDashing => isDashing;

        private void Awake()
        {
            movement = GetComponent<Movement>();
            health = GetComponent<Health>();
            playerAnimation = GetComponent<PlayerAnimation>();

            if (dashConfig == null)
                dashConfig = GlobalEntities.Instance?.DefaultDashConfig;
        }

        public void SetDashEnabled(bool enabled)
        {
            enabledState = enabled;
        }

        private bool enabledState = true;

        private void Update()
        {
            if (!enabledState || isDashing)
                return;

            if (!InputManager.Instance.WasDashPressed())
                return;

            if (Time.time < lastDashTime + GetCooldown())
                return;

            var direction = GetDashDirection();
            if (direction.sqrMagnitude < 0.0001f)
                direction = transform.forward;

            PerformDash(direction.normalized).Forget();
        }

        private Vector3 GetDashDirection()
        {
            Vector2 input = InputManager.Instance.GetMovementVector();
            if (input.sqrMagnitude > 0.01f)
                return new Vector3(input.x, 0f, input.y);

            return transform.forward;
        }

        private async UniTask PerformDash(Vector3 direction)
        {
            isDashing = true;
            lastDashTime = Time.time;
            movement?.SetMovementEnabled(false);
            health?.SetInvulnerable(true);
            playerAnimation?.SetDash();

            var config = dashConfig;
            float distance = config != null ? config.distance : 4f;
            float duration = config != null ? config.duration : 0.2f;
            float iFrameDuration = config != null ? config.iFrameDuration : 0.18f;

            Vector3 start = transform.position;
            Vector3 target = start + direction * distance;
            target = ClampToGround(target);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
                transform.position = Vector3.Lerp(start, target, t);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            transform.position = target;

            if (iFrameDuration > duration)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(iFrameDuration - duration),
                    ignoreTimeScale: false);
            }

            health?.SetInvulnerable(false);
            movement?.SetMovementEnabled(true);
            isDashing = false;
        }

        private static Vector3 ClampToGround(Vector3 position)
        {
            position.y = 0f;
            return position;
        }

        private float GetCooldown() => dashConfig != null ? dashConfig.cooldown : 2f;
    }
}
