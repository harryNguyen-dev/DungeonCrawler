using System;
using System.Collections.Generic;
using SO;
using UnityEngine;

namespace PlayerController
{
    public class PlayerEffect : MonoBehaviour
    {
        private static readonly Dictionary<CardEffect, Action<PlayerStats, CardSO>> Handlers =
            new()
            {
                [CardEffect.IncreaseDamage] = (stats, card) =>
                    stats.UpgradeAttackDamage(Mathf.RoundToInt(card.Value)),

                [CardEffect.IncreaseAttackSpeed] = (stats, card) =>
                    stats.UpgradeAttackSpeed(card.Value),

                [CardEffect.IncreaseMaxHealth] = (stats, card) =>
                    stats.UpgradeMaxHealth(Mathf.RoundToInt(card.Value)),

                [CardEffect.HealHealth] = (stats, card) =>
                    stats.HealHealth(Mathf.RoundToInt(card.Value)),

                [CardEffect.IncreaseAmor] = (stats, card) =>
                    stats.UpgradeIncreaseAmor(Mathf.RoundToInt(card.Value)),

                [CardEffect.ThornArmor] = (stats, card) =>
                    stats.UpgradeThornReflect(card.Value),

                [CardEffect.IncreaseRunSpeed] = (stats, card) =>
                    stats.UpgradeIncreaseRunSpeed(card.Value),

                [CardEffect.InceaseHealSpeed] = (stats, card) =>
                    stats.UpgradeIncreaseHealSpeed(card.Value),

                [CardEffect.IncreaseExpGain] = (stats, card) =>
                    stats.UpgradeIncreaseExpGain(card.Value),

                [CardEffect.IncreaseGoldGain] = (stats, card) =>
                    stats.UpgradeIncreaseGoldGain(card.Value),

                [CardEffect.AddOneProjectile] = (stats, card) =>
                    stats.AddOneProjectile(Mathf.RoundToInt(card.Value)),

                [CardEffect.ProjectileBoomerang] = (stats, _) =>
                    stats.AddProjectileBoomerange(),
            };

        public void BuildEffectForPlayer(CardSO cardData)
        {
            if (cardData == null || cardData.Effect == CardEffect.None)
                return;

            var playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
                return;

            if (Handlers.TryGetValue(cardData.Effect, out var apply))
            {
                apply(playerStats, cardData);
                return;
            }

            Debug.LogWarning($"[PlayerEffect] No handler for card effect {cardData.Effect} ({cardData.CardID}).");
        }
    }
}
