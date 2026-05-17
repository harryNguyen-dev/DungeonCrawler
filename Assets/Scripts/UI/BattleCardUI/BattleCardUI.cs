using System;
using System.Collections.Generic;
using SO;
using UnityEngine;
using UnityEngine.UI;
namespace CustomUI
{
    public class BattleCardUI : MonoBehaviour
    {
        [SerializeField] private GameObject cardPanel;
        [SerializeField] private GameObject gridLayout;
        [SerializeField] private GameObject cardPrefab;

        private void OnEnable()
        {
            Debug.Log("[BattleCardUI] OnEnable");
            Global.GlobalEvents.OnRequestBattleCardUI += ShowPanel;
        }

        private void OnDisable()
        {
            Debug.Log("[BattleCardUI] OnDisable");
            Global.GlobalEvents.OnRequestBattleCardUI -= ShowPanel;
        }

        private void Start()
        {
            cardPanel.SetActive(false);
        }

        private void ShowPanel()
        {
            Debug.Log("[BattleCardUI] ShowPanel");
            cardPanel.SetActive(true);
            BuildCards();
        }
        private void BuildCards()
        {
            List<CardSO> cards = GetRandomCards();
    
            for (int i = gridLayout.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(gridLayout.transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < cards.Count; i++)
            {
                GameObject cardObj = Instantiate(cardPrefab, gridLayout.transform);
                
                CardUI cardScript = cardObj.GetComponent<CardUI>();
                if (cardScript != null)
                {
                    cardScript.SetData(cards[i], this);
                }
            }
        }
        private List<CardSO> GetRandomCards()
        {
            List<CardSO> pool = new List<CardSO>(Global.GlobalEntities.Instance.GetAllCards());
            List<CardSO> pickedCards = new List<CardSO>();

            int totalPick = 3;

            // Giới hạn số lượt pick để tránh crash nếu tổng số Card trong game ít hơn 3
            int actualPickCount = Mathf.Min(totalPick, pool.Count);

            for (int i = 0; i < actualPickCount; i++)
            {
                // 2. Tính lại tổng trọng số của những thẻ còn lại trong Pool
                int totalWeight = 0;
                foreach (CardSO card in pool)
                {
                    totalWeight += (int)card.CardTierWeight;
                }

                if (totalWeight <= 0) break;

                // 3. Tiến hành Roll cho lượt này
                int randomRoll = UnityEngine.Random.Range(0, totalWeight);
                int currentWeightWindow = 0;

                foreach (CardSO card in pool)
                {
                    currentWeightWindow += (int)card.CardTierWeight;
                    if (randomRoll < currentWeightWindow)
                    {
                        // Tìm thấy thẻ trúng tuyển
                        pickedCards.Add(card);

                        // 4. CHÌA KHÓA: Xóa thẻ này khỏi pool để lượt sau không bị trùng
                        pool.Remove(card);
                        break; // Thoát foreach để chạy lượt for tiếp theo với Pool mới
                    }
                }
            }

            return pickedCards;
        }
        public void HideCardPanelAndContinueGame()
        {
            cardPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

}