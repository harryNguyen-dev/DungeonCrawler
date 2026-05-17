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
            Global.GlobalEntities.Instance.PlayerEffect.BuildEffectForPlayer(cardData);
            battleCardUI.HideCardPanelAndContinueGame();
        }
        private void SetColorForCard()
        {
            var tier = cardData.CardTier;
            BackgroundImage.color = Global.GlobalVariable.CardBackgroundColor[tier]; 
            Title.color = Global.GlobalVariable.CardTextColor[tier];
            Description.color = Global.GlobalVariable.CardTextColor[tier];
        }
    }
}
