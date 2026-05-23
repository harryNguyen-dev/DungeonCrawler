using PlayerController;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class HealthBarHUD : MonoBehaviour
    {
        [SerializeField] private Image healthBarFill;

        private PlayerEvents playerEvents;

        private void OnEnable()
        {
            Global.GlobalEvents.OnPlayerJoin += InitHUD;
        }

        private void OnDisable()
        {
            Global.GlobalEvents.OnPlayerJoin -= InitHUD;
            UnbindPlayerEvents();
        }

        private void InitHUD()
        {
            UnbindPlayerEvents();

            if (Global.GlobalEntities.Instance == null) return;
            playerEvents = Global.GlobalEntities.Instance.PlayerEvents;
            if (playerEvents == null || healthBarFill == null) return;

            healthBarFill.fillAmount = 1f;
            playerEvents.OnHealthChanged += SetHealthBar;
        }

        private void UnbindPlayerEvents()
        {
            if (playerEvents == null) return;
            playerEvents.OnHealthChanged -= SetHealthBar;
            playerEvents = null;
        }

        private void SetHealthBar(int health, int maxHealth)
        {
            if (healthBarFill == null || maxHealth <= 0) return;

            Debug.Log("[HealthBarHUD] SetHealthBar health: " + health + " maxHealth: " + maxHealth);
            healthBarFill.fillAmount = Mathf.Clamp01((float)health / maxHealth);
        }
    }
}
