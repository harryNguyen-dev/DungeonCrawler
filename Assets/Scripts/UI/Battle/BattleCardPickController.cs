using System.Collections.Generic;
using Global;
using SO;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUI.Battle
{
    [RequireComponent(typeof(UIDocument))]
    public class BattleCardPickController : MonoBehaviour
    {
        private const int CardsToPick = 3;

        private UIDocument uiDocument;
        private VisualElement pickRoot;
        private VisualElement cardContainer;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null)
            {
                uiDocument.sortingOrder = 100;
            }
        }

        private void OnEnable()
        {
            GlobalEvents.OnRequestBattleCardUI += ShowPanel;
            CacheElements();
            HidePanel();
        }

        private void OnDisable()
        {
            GlobalEvents.OnRequestBattleCardUI -= ShowPanel;
        }

        private void CacheElements()
        {
            var root = uiDocument?.rootVisualElement;
            if (root == null) return;

            pickRoot = root.Q<VisualElement>("card-pick-root");
            cardContainer = root.Q<VisualElement>("card-container");
        }

        private void ShowPanel()
        {
            if (pickRoot == null || cardContainer == null)
            {
                CacheElements();
            }

            if (cardContainer == null) return;

            cardContainer.Clear();
            foreach (var card in GetRandomCards())
            {
                cardContainer.Add(BuildCardElement(card));
            }

            if (pickRoot != null)
            {
                pickRoot.style.display = DisplayStyle.Flex;
            }
        }

        private void HidePanel()
        {
            if (pickRoot != null)
            {
                pickRoot.style.display = DisplayStyle.None;
            }

            GlobalEvents.RaiseBattleCardUIDismissed();
        }

        private VisualElement BuildCardElement(CardSO cardData)
        {
            var tier = cardData.CardTier;

            var card = new VisualElement();
            card.AddToClassList("card-item");
            card.style.backgroundColor = GlobalVariable.CardBackgroundColor[tier];

            var title = new Label(cardData.CardName);
            title.AddToClassList("card-title");
            title.style.color = GlobalVariable.CardTextColor[tier];

            var description = new Label(cardData.CardDescription);
            description.AddToClassList("card-desc");
            description.style.color = GlobalVariable.CardTextColor[tier];

            card.Add(title);
            card.Add(description);
            card.RegisterCallback<ClickEvent>(_ => OnCardSelected(cardData));

            return card;
        }

        private void OnCardSelected(CardSO cardData)
        {
            var playerEffect = GlobalEntities.Instance?.PlayerEffect;
            if (playerEffect != null)
            {
                playerEffect.BuildEffectForPlayer(cardData);
            }

            HidePanel();
            Time.timeScale = 1f;
        }

        private static List<CardSO> GetRandomCards()
        {
            var pool = new List<CardSO>();
            foreach (var card in GlobalEntities.Instance.GetAllCards())
            {
                if (CardPoolFilter.IsEligibleForPool(card))
                    pool.Add(card);
            }

            var pickedCards = new List<CardSO>();
            var actualPickCount = Mathf.Min(CardsToPick, pool.Count);

            for (var i = 0; i < actualPickCount; i++)
            {
                var totalWeight = 0;
                foreach (var card in pool)
                {
                    totalWeight += (int)card.CardTierWeight;
                }

                if (totalWeight <= 0) break;

                var randomRoll = Random.Range(0, totalWeight);
                var currentWeightWindow = 0;

                foreach (var card in pool)
                {
                    currentWeightWindow += (int)card.CardTierWeight;
                    if (randomRoll < currentWeightWindow)
                    {
                        pickedCards.Add(card);
                        pool.Remove(card);
                        break;
                    }
                }
            }

            return pickedCards;
        }
    }
}
