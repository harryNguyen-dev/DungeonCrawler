using Core;
using Cysharp.Threading.Tasks;
using PlayerController.Skill;
using SO;
using UnityEngine;

namespace PlayerController
{
    public class PlayerSkill : MonoBehaviour
    {
        private PlayerStats playerStats;
        private PlayerAnimation playerAnimation;
        private Attack attack;
        private Rotate playerRotate;
        private HeroSkillSO activeSkill;
        private float lastSkillTime = -999f;
        private bool canUseSkill = true;
        private PlayerDash playerDash;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
            playerAnimation = GetComponent<PlayerAnimation>();
            playerDash = GetComponent<PlayerDash>();
            attack = GetComponent<Attack>();
            playerRotate = GetComponent<Rotate>();

            if (GetComponent<PlayerTimedBuffTracker>() == null)
                gameObject.AddComponent<PlayerTimedBuffTracker>();
        }

        public void SetSkillEnabled(bool enabled)
        {
            canUseSkill = enabled;
        }

        public void SetActiveSkill(HeroSkillSO skill)
        {
            activeSkill = skill;
        }

        public bool TryGetCooldown(out float remaining, out float duration)
        {
            duration = activeSkill != null ? activeSkill.cooldown : 0f;
            remaining = duration > 0f
                ? Mathf.Max(0f, lastSkillTime + duration - Time.time)
                : 0f;
            return remaining > 0f;
        }

        private void Update()
        {
            if (!canUseSkill || activeSkill == null)
                return;

            if (playerDash != null && playerDash.IsDashing)
                return;

            var input = InputManager.Instance;
            if (input == null || !input.WasSkillPressed())
                return;

            if (Time.time < lastSkillTime + activeSkill.cooldown)
                return;

            PerformSkill().Forget();
        }

        private async UniTask PerformSkill()
        {
            lastSkillTime = Time.time;

            playerRotate?.SnapFaceAimDirection();
            var direction = transform.forward;
            Core.GameAudio.PlayPlayerSkill(transform.position);
            playerAnimation?.SetSkill();

            await UniTask.Yield(PlayerLoopTiming.Update);
            ExecuteActiveSkill(direction);
        }

        private void ExecuteActiveSkill(Vector3 direction)
        {
            if (activeSkill == null || playerStats == null)
                return;

            if (activeSkill.deliveryType != SkillDeliveryType.SelfBuff
                && activeSkill.skillProjectilePrefab == null)
            {
                Debug.LogWarning($"[PlayerSkill] Skill '{activeSkill.skillId}' missing projectile prefab.");
                return;
            }

            var context = new SkillExecutionContext(
                activeSkill,
                playerStats,
                transform,
                attack != null ? attack.GetFirePoint() : null,
                direction);

            SkillDeliveryRegistry.Execute(activeSkill.deliveryType, context);
        }
    }
}
