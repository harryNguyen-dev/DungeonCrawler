namespace SO
{
    /// <summary>Weapon identity effects are not offered as in-run cards.</summary>
    public static class CardPoolFilter
    {
        public static bool IsWeaponTraitCard(CardEffect effect)
        {
            return effect switch
            {
                CardEffect.ProjectilePierce => true,
                CardEffect.ProjectileFireOnHit => true,
                CardEffect.ProjectileFrozenOnHit => true,
                CardEffect.ExplosiveImpact => true,
                _ => false
            };
        }

        public static bool IsEligibleForPool(CardSO card)
        {
            return card != null && !IsWeaponTraitCard(card.Effect);
        }
    }
}
