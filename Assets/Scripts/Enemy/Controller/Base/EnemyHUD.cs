using UnityEngine;
using UnityEngine.UI;

namespace EnemyController
{
    [RequireComponent(typeof(EnemyEvents))]
    public class EnemyHUD : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private GameObject HUDCanvas;
        [SerializeField] private Image healthBar;

        private EnemyEvents instance;
        private float lastShowHUDTime = 0f;
        private float maxTimeShowHUD = 3f;
        private bool isShowHUD = false;

        private void Awake()
        {
            instance = GetComponent<EnemyEvents>();
        }

        private void OnEnable()
        {
            if (instance == null)
            {
                instance = GetComponent<EnemyEvents>();
            }

            if (instance != null)
            {
                instance.OnHealthChange += HandleHealthChange;
            }
        }

        private void Start()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (HUDCanvas != null)
            {
                HUDCanvas.SetActive(false);
            }
        }

        private void Update()
        {
            if(isShowHUD)
            {
                if(Time.time >= lastShowHUDTime + maxTimeShowHUD)
                {
                    if (HUDCanvas != null)
                    {
                        HUDCanvas.SetActive(false);
                    }

                    isShowHUD = false;
                    lastShowHUDTime = 0f;
                }
            }
        }

        private void HandleHealthChange(int current)
        {
            if (HUDCanvas == null || healthBar == null) return;

            HUDCanvas.SetActive(true);
            isShowHUD = true;
            lastShowHUDTime = Time.time;

            if (health == null)
                health = GetComponent<Health>();

            var max = health != null && health.MaxHealth > 0 ? health.MaxHealth : 100;
            healthBar.fillAmount = (float)current / max;
        }

        private void OnDisable()
        {
            if (instance != null)
            {
                instance.OnHealthChange -= HandleHealthChange;
            }
        }
    }
}
