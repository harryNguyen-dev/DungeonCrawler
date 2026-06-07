using System.Collections.Generic;
using Global;
using SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class CardBattleUI : MonoBehaviour
    {
        private const int CardsToPick = 3;
        private const string CardDescChildName = "CardDesTxt";

        [SerializeField] private GameObject cardContainer;
        [SerializeField] private GameObject cardPrefab;

        private readonly List<GameObject> spawnedCards = new();

        private void OnEnable()
        {
            GlobalEvents.OnRequestBattleCardUI += ShowPanel;
            HidePanel();
        }

        private void OnDisable()
        {
            GlobalEvents.OnRequestBattleCardUI -= ShowPanel;
        }

        private void ShowPanel()
        {
            if (cardContainer == null || cardPrefab == null)
                return;

            ClearSpawnedCards();

            foreach (var card in GetRandomCards())
            {
                spawnedCards.Add(SpawnCard(card));
            }

            cardContainer.SetActive(true);
        }

        private void HidePanel()
        {
            ClearSpawnedCards();

            if (cardContainer != null)
                cardContainer.SetActive(false);

            GlobalEvents.RaiseBattleCardUIDismissed();
        }

        private GameObject SpawnCard(CardSO cardData)
        {
            var instance = Instantiate(cardPrefab, cardContainer.transform);
            ApplyCardVisuals(instance, cardData);

            var button = instance.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnCardSelected(cardData));
            }

            return instance;
        }

        private static void ApplyCardVisuals(GameObject cardInstance, CardSO cardData)
        {
            var tier = cardData.CardTier;
            var bgColor = GlobalVariable.CardBackgroundColor[tier];
            var textColor = GlobalVariable.CardTextColor[tier];

            if (cardInstance.TryGetComponent<Image>(out var bgImage))
                bgImage.color = bgColor;

            var descTransform = cardInstance.transform.Find(CardDescChildName);
            if (descTransform != null && descTransform.TryGetComponent<TMP_Text>(out var descText))
            {
                descText.text = $"{cardData.CardName}\n\n{cardData.CardDescription}";
                descText.color = textColor;
            }
        }

        private void OnCardSelected(CardSO cardData)
        {
            var playerEffect = GlobalEntities.Instance?.PlayerEffect;
            if (playerEffect != null)
                playerEffect.BuildEffectForPlayer(cardData);

            HidePanel();
            Time.timeScale = 1f;
        }

        private void ClearSpawnedCards()
        {
            foreach (var card in spawnedCards)
            {
                if (card != null)
                    Destroy(card);
            }

            spawnedCards.Clear();
        }

        private static List<CardSO> GetRandomCards()
        {
            var pool = new List<CardSO>();
            foreach (var card in GlobalEntities.Instance.GetAllCards())
            {
                if (card != null)
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

                if (totalWeight <= 0)
                    break;

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
