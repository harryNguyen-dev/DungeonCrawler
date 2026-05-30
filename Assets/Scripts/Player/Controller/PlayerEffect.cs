using SO;
using UnityEngine;

namespace PlayerController
{
    public class PlayerEffect : MonoBehaviour
    {   
        public void BuildEffectForPlayer(CardSO cardData)
        {
            CardEffect effect = cardData.Effect;
            var playerStats = GetComponent<PlayerStats>();
            switch (effect)
            {
                case CardEffect.None: 
                    break;

                case CardEffect.IncreaseDamage:
                    playerStats.UpgradeAttackDamage(Mathf.RoundToInt(cardData.Value));
                    break;
                case CardEffect.IncreaseAttackSpeed:
                    playerStats.UpgradeAttackSpeed(cardData.Value);
                    break;

                case CardEffect.IncreaseMaxHealth:
                    playerStats.UpgradeMaxHealth(Mathf.RoundToInt(cardData.Value));
                    break;
                case CardEffect.HealHealth:
                    playerStats.HealHealth(Mathf.RoundToInt(cardData.Value));
                    break;

                case CardEffect.IncreaseAmor:
                    playerStats.UpgradeIncreaseAmor(Mathf.RoundToInt(cardData.Value));
                    break;
                case CardEffect.ThornArmor:
                    break;

                case CardEffect.IncreaseRunSpeed:
                    playerStats.UpgradeIncreaseRunSpeed(cardData.Value);
                    break;
                case CardEffect.InceaseHealSpeed:
                    break;
                case CardEffect.IncreaseExpGain:
                    playerStats.UpgradeIncreaseExpGain(cardData.Value);
                    break;
                case CardEffect.IncreaseGoldGain:
                    playerStats.UpgradeIncreaseGoldGain(cardData.Value);
                    break;
                
                case CardEffect.AddOneProjectile:
                    playerStats.AddOneProjectile(Mathf.RoundToInt(cardData.Value));
                    break;
                case CardEffect.ProjectileFireOnHit:
                case CardEffect.ProjectileFrozenOnHit:
                case CardEffect.ProjectilePierce:
                case CardEffect.ExplosiveImpact:
                    Debug.LogWarning($"[PlayerEffect] Card {cardData.CardID} is a weapon trait and should not be picked.");
                    break;
                case CardEffect.ProjectileBoomerang:
                    playerStats.AddProjectileBoomerange();
                    break;

                default:
                    break;
            }
        }
    }
}