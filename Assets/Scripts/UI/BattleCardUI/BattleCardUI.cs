using UnityEngine;
using UnityEngine.UI;
namespace CustomUI
{
    public class BattleCardUI : MonoBehaviour
    {
        [SerializeField] private GameObject cardPanel;
        [SerializeField] private Button upgradeAtkSpeedBtn;
        [SerializeField] private Button upgradeDamageBtn;

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

            // Gán chức năng cho nút
            upgradeAtkSpeedBtn.onClick.AddListener(() => SelectCard("Speed"));
            upgradeDamageBtn.onClick.AddListener(() => SelectCard("Damage"));
        }

        private void ShowPanel()
        {
            Debug.Log("[BattleCardUI] ShowPanel");
            cardPanel.SetActive(true);
        }

        private void SelectCard(string type)
        {
            if (type == "Speed")
            {
                Debug.Log("[BattleCardUI] Upgrade Attack Speed");
                Global.GlobalEntities.Instance.PlayerStats.UpgradeAttackSpeed(0.1f);
            }
            else if (type == "Damage")
            {
                Debug.Log("[BattleCardUI] Upgrade Attack Damage");
                Global.GlobalEntities.Instance.PlayerStats.UpgradeAttackDamage(10f);
            }
            // Tiếp tục game
            cardPanel.SetActive(false);
            Time.timeScale = 1f;
            Debug.Log($"Selected Card: {type}");
        }
    }

}