using UnityEngine;

namespace EnemyController
{

    public class EnemyEffects : MonoBehaviour
    {
        [SerializeField] private GameObject fireEffect;

        public void OnDisable()
        {
            HideFireEffect();
        }
        public void ShowFireEffect()
        {
            fireEffect.SetActive(true);
        }
        public void HideFireEffect()
        {
            fireEffect.SetActive(false);
        }
    }

}