using UnityEngine;
using UnityEngine.UI;

namespace EnemyController
{
    public class EnemyHUD : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private GameObject HUDCanvas;
        [SerializeField] private Image healthBar;

        private EnemyEvents instance;
        private float lastShowHUDTime = 0f;
        private float maxTimeShowHUD = 3f;
        private bool isShowHUD = false;
        private void Start()
        {
            instance = GetComponent<EnemyEvents>();
            instance.OnHealthChange += HandleHealthChange;
        }
        private void Update()
        {
            if(isShowHUD)
            {
                if(Time.time >= lastShowHUDTime + maxTimeShowHUD)
                {
                    HUDCanvas.SetActive(false);
                    isShowHUD = false;
                    lastShowHUDTime = 0f;
                }
            }
        }
        private void HandleHealthChange(int health)
        {
            HUDCanvas.SetActive(true);
            isShowHUD = true;
            lastShowHUDTime = Time.time;
            healthBar.fillAmount = health / 100f;
        }
        private void OnDisable()
        {
            instance.OnHealthChange -= HandleHealthChange;            
        }
    }
}
