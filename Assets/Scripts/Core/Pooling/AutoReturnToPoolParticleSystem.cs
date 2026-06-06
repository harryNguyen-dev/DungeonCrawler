using UnityEngine;

namespace Core
{
    public class AutoReturnToPoolParticleSystem : MonoBehaviour
    {
        private void OnParticleSystemStopped()
        {
            PoolReturn.SafeReturn(gameObject);
        }
    }
}
