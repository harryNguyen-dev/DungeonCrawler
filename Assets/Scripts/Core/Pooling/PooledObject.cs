using UnityEngine;

namespace Core
{
    /// <summary>
    /// Attached by <see cref="ObjectPoolingManager"/> to instances so <see cref="ObjectPoolingManager.Return"/> can resolve the pool.
    /// </summary>
    public sealed class PooledObject : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField] PoolId _poolId;

        public PoolId PoolId => _poolId;

        internal void SetPoolId(PoolId id) => _poolId = id;
    }
}
