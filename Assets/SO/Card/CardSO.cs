using UnityEngine;

namespace SO
{
    
    public enum CardEffect
    {
        None,
        
        // ATK
        IncreaseDamage,
        IncreaseAttackSpeed,

        // DEF
        IncreaseMaxHealth,
        HealHealth,

        // AMOR
        IncreaseAmor,
        ThornArmor,


        // BUFF
        IncreaseRunSpeed,
        InceaseHealSpeed,
        IncreaseExpGain,
        IncreaseGoldGain,


        // EFFECT
        AddOneProjectile,
        ProjectileFireOnHit,
        ProjectileFrozenOnHit,
        ProjectilePierce,
        ProjectileBoomerang,
        ExplosiveImpact,
    }
    
    public enum CardTier
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
    public enum CardTierWeight
    {
        Common = 60,
        Rare = 30,
        Epic = 15,
        Legendary = 5
    }
    
    [CreateAssetMenu(fileName = "Card", menuName = "Card", order = 0)]
    public class CardSO : ScriptableObject 
    {        
        public string CardID;
        public string CardName;
        public string CardDescription;

        public CardEffect Effect = CardEffect.None;
        public CardTier CardTier = CardTier.Common;
        public CardTierWeight CardTierWeight = CardTierWeight.Common;
        public float Value = 0f;
    }
}
