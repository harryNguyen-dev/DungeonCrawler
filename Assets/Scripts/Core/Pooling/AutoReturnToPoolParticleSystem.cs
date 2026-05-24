using UnityEngine;
namespace Core
{

    public class AutoReturnToPoolParticleSystem : MonoBehaviour
    {
        // Hàm này sẽ tự động chạy khi Particle System trên chính nó kết thúc
        private void OnParticleSystemStopped()
        {
            // Tự trả chính nó về Pool
            if (Core.ObjectPoolingManager.Instance != null)
            {
                Core.ObjectPoolingManager.Instance.Return(gameObject);
            }
        }
    }
}