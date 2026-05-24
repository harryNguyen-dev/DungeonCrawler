using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
namespace CombatFeel
{
    public class PlayerHitEffect : MonoBehaviour
    {
        [SerializeField] private GameObject hitEffect;

        private Core.PoolId poolID;

        private void Awake()
        {
            poolID = hitEffect.GetComponent<Core.PooledObject>().PoolId;
        }
        public void PlayHitEffect(Vector3 hitPosition, Quaternion rotation)
        {
            // Chỉ việc gọi lấy ra từ Pool, bản thân Prefab sẽ tự biết cách biến mất
            Core.ObjectPoolingManager.Instance.Get(poolID, hitPosition, rotation);
        }
    }
}
