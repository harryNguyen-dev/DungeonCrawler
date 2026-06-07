using UnityEngine;

namespace PlayerController.Skill.Deliveries
{
    public sealed class GroundAoESkillDelivery : ISkillDelivery
    {
        public void Execute(in SkillExecutionContext context)
        {
            Debug.LogWarning($"[GroundAoESkillDelivery] Not implemented for skill '{context.Skill?.skillId}'.");
        }
    }
}
