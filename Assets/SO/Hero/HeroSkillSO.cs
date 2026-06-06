using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    public enum SkillDeliveryType
    {
        Projectile,
        Cone,
        GroundAoE,
        Beam
    }

    [CreateAssetMenu(fileName = "HeroSkill", menuName = "Hero/Hero Skill")]
    public class HeroSkillSO : ScriptableObject
    {
        [Header("Identity")]
        public string skillId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Combat")]
        public SkillDeliveryType deliveryType = SkillDeliveryType.Projectile;
        public float cooldown = 3f;
        public int damage = 25;
        public float range = 12f;
        public float projectileSpeed = 18f;
        public GameObject skillProjectilePrefab;
        public List<WeaponEffectModifier> skillEffects = new();

        [Header("Cone / AoE (future)")]
        public float coneAngle = 45f;
        public float aoeRadius = 3f;
    }
}
