using System;
using SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{

    public class CardUI : MonoBehaviour
    {
        SO.CardSO cardData;
        public Button btn;
        public TMP_Text Title;
        public TMP_Text Description;
        public Image BackgroundImage;
        public void SetData(SO.CardSO cardData, BattleCardUI battleCardUI)
        {
            this.cardData = cardData;
            Title.text = cardData.CardName;
            Description.text = cardData.CardDescription;
            SetColorForCard();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnClicked(battleCardUI));
        }
        private void OnClicked(BattleCardUI battleCardUI)
        {
            Debug.Log($"[OnClicked] Choose card: {cardData.CardName} with Effect: {cardData.Effect} and Value: {cardData.Value}");
            BuildEffectForPlayer();
            battleCardUI.HideCardPanelAndContinueGame();
        }
        private void SetColorForCard()
        {
            var tier = cardData.CardTier;
            BackgroundImage.color = Global.GlobalVariable.CardBackgroundColor[tier]; 
            Title.color = Global.GlobalVariable.CardTextColor[tier];
            Description.color = Global.GlobalVariable.CardTextColor[tier];
        }
        private void BuildEffectForPlayer()
        {
            CardEffect effect = cardData.Effect;
            var playerStats = Global.GlobalEntities.Instance.PlayerStats;
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
                    break;
                case CardEffect.ProjectileFireOnHit:
                    break;

                case CardEffect.ProjectileFrozenOnHit:
                    break;
                case CardEffect.ProjectilePierce:
                    break;
                case CardEffect.ProjectileBoomerang:
                    break;
                case CardEffect.ExplosiveImpact:
                    break;

                default:
                    break;
            }
        }
    }
}
