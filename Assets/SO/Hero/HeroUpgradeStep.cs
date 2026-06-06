using System;
using UnityEngine;

namespace SO
{
    [Serializable]
    public struct HeroUpgradeStep
    {
        public int cost;
        public int damageBonus;
        public int healthBonus;
        public float cooldownReduction;
        [Range(0f, 1f)]
        public float critChanceBonus;
    }
}
