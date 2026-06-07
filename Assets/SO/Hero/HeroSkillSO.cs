using System;
using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    public enum SkillDeliveryType
    {
        Projectile,
        Cone,
        GroundAoE,
        Beam,
        SelfBuff,
    }

    public enum SkillDamageMode
    {
        Fixed,
        PercentOfAttack,
        RollAttackDamage,
    }

    [Serializable]
    public struct ConeSkillConfig
    {
        public int projectileCount;
        public float coneAngle;
        public float spawnDelayMs;
        public bool centerPelletPierce;
    }

    [Serializable]
    public struct BuffSkillConfig
    {
        public float duration;
        public bool refreshOnReuse;
        public List<StatModifier> modifiers;
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
        public SkillDamageMode damageMode = SkillDamageMode.Fixed;
        public int damage = 25;
        [Range(0f, 2f)]
        public float damagePercent = 0.45f;
        public float range = 12f;
        public float projectileSpeed = 18f;
        public GameObject skillProjectilePrefab;
        public List<WeaponEffectModifier> skillEffects = new();

        [Header("Cone")]
        public ConeSkillConfig coneConfig;

        [Header("Buff")]
        public BuffSkillConfig buffConfig;

        [Header("AoE (future)")]
        public float coneAngle = 45f;
        public float aoeRadius = 3f;
    }
}
