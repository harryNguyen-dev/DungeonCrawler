using UnityEngine;
namespace CombatFeel
{
    public class PlayerHitEffect : MonoBehaviour
    {
        [SerializeField] private GameObject hitEffect;

        public void PlayHitEffect(Vector3 hitPosition, Quaternion rotation)
        {
            Instantiate(hitEffect, hitPosition, rotation);
        }
    }
}
