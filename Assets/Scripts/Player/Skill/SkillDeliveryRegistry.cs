using System.Collections.Generic;
using PlayerController.Skill.Deliveries;
using SO;
using UnityEngine;

namespace PlayerController.Skill
{
    public static class SkillDeliveryRegistry
    {
        private static readonly Dictionary<SkillDeliveryType, ISkillDelivery> Deliveries = new()
        {
            [SkillDeliveryType.Projectile] = new ProjectileSkillDelivery(),
            [SkillDeliveryType.Cone] = new ConeSkillDelivery(),
            [SkillDeliveryType.SelfBuff] = new SelfBuffSkillDelivery(),
            [SkillDeliveryType.GroundAoE] = new GroundAoESkillDelivery(),
            [SkillDeliveryType.Beam] = new BeamSkillDelivery(),
        };

        public static void Execute(SkillDeliveryType type, in SkillExecutionContext context)
        {
            if (Deliveries.TryGetValue(type, out var delivery))
            {
                delivery.Execute(context);
                return;
            }

            Debug.LogWarning($"[SkillDeliveryRegistry] No delivery handler for {type}.");
        }
    }
}
