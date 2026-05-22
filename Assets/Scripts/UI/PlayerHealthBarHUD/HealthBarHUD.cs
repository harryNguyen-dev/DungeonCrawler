using PlayerController;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class HealthBarHUD : MonoBehaviour
    {
        [SerializeField] private Image healthBarFill;

        private PlayerEvents playerEvents;
        private void Start()
        {
            Global.GlobalEvents.OnPlayerJoin += InitHUD;
        }
        private void InitHUD()
        {
            if(playerEvents == null)
            {
                playerEvents = Global.GlobalEntities.Instance.PlayerEvents;
            }
            healthBarFill.fillAmount = 1;
            playerEvents.OnHealthChanged += SetHealthBar;
        }
        private void SetHealthBar(int health, int maxHealth)
        {
            Debug.Log("[HealthBarHUD] SetHealthBar health: " + health + " maxHealth: " + maxHealth);
            healthBarFill.fillAmount = health * 1.0f/ maxHealth * 1.0f;
        }
    }
}
