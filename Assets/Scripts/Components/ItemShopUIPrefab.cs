using System;
using Core.Save;
using SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class ItemShopUIPrefab : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text itemDescription;

        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text buyPriceText;

        private HeroSO boundHero;
        private Action<HeroSO> onBuyClicked;

        private void Awake()
        {
            buyButton?.onClick.AddListener(OnBuyClicked);
        }

        private void OnDestroy()
        {
            buyButton?.onClick.RemoveListener(OnBuyClicked);
        }

        public void Bind(HeroSO hero, Action<HeroSO> onBuyCallback)
        {
            boundHero = hero;
            onBuyClicked = onBuyCallback;

            if (hero == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (itemName != null)
                itemName.text = hero.displayName;

            if (itemDescription != null)
                itemDescription.text = hero.description;

            if (itemIcon != null)
            {
                itemIcon.sprite = hero.icon;
                itemIcon.enabled = hero.icon != null;
            }

            if (buyPriceText != null)
                buyPriceText.text = hero.unlockCost.ToString();

            RefreshBuyState();
        }

        public void RefreshBuyState()
        {
            if (boundHero == null || buyButton == null)
                return;

            buyButton.interactable = LevelProgressService.GetMetaGold() >= boundHero.unlockCost;
        }

        private void OnBuyClicked()
        {
            if (boundHero != null)
                onBuyClicked?.Invoke(boundHero);
        }
    }
}
