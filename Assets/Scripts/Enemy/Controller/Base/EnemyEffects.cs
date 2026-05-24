using Core;
using UnityEngine;

namespace EnemyController
{

    public class EnemyEffects : MonoBehaviour, IPoolable
    {
        [SerializeField] private GameObject fireEffect;

        public void ShowFireEffect()
        {
            fireEffect.SetActive(true);
        }
        public void HideFireEffect()
        {
            fireEffect.SetActive(false);
        }
        public void OnSpawnedFromPool()
        {
            HideFireEffect();
        }

        public void OnReturnedToPool()
        {
            HideFireEffect();
        }
    }

}