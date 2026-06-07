using UnityEngine;

namespace PlayerController.Skill.Deliveries
{
    public sealed class SelfBuffSkillDelivery : ISkillDelivery
    {
        public void Execute(in SkillExecutionContext context)
        {
            var skill = context.Skill;
            var config = skill.buffConfig;
            if (config.duration <= 0f || config.modifiers == null || config.modifiers.Count == 0)
            {
                Debug.LogWarning($"[SelfBuffSkillDelivery] Skill '{skill.skillId}' has invalid buff config.");
                return;
            }

            var tracker = context.Caster.GetComponent<PlayerTimedBuffTracker>();
            if (tracker == null)
                tracker = context.Caster.gameObject.AddComponent<PlayerTimedBuffTracker>();

            if (!tracker.TryApplyBuff(skill.skillId, config))
                Debug.Log($"[SelfBuffSkillDelivery] Buff '{skill.skillId}' already active.");
        }
    }
}
