using UnityEngine;

namespace Core
{
    /// <summary>
    /// Attached by pool managers to instances so <see cref="PoolReturn.SafeReturn"/> can resolve the correct pool.
    /// </summary>
    public sealed class PooledObject : MonoBehaviour
    {
        // [HideInInspector]
        [SerializeField] PoolId _poolId;

        public PoolId PoolId => _poolId;

        internal void SetPoolId(PoolId id) => _poolId = id;
    }
}
