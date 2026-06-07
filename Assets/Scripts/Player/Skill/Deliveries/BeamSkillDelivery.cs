using UnityEngine;

namespace PlayerController.Skill.Deliveries
{
    public sealed class BeamSkillDelivery : ISkillDelivery
    {
        public void Execute(in SkillExecutionContext context)
        {
            Debug.LogWarning($"[BeamSkillDelivery] Not implemented for skill '{context.Skill?.skillId}'.");
        }
    }
}
