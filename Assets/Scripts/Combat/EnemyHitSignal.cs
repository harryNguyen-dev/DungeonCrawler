using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Ghi nhận đòn vừa nhận (nguồn từ <see cref="DamageHitInfo.Attacker"/>) để Behavior Tree / BeAttackCondition kiểm tra.
    /// Sau khi xử lý xong nhánh hit, gọi <see cref="ClearPending"/> (vd. từ node Action trong graph).
    /// </summary>
    public class EnemyHitSignal : MonoBehaviour
    {
        bool _pending;
        GameObject _attackerRoot;

        public void RegisterHit(GameObject attacker)
        {
            _pending = true;
            _attackerRoot = attacker != null ? attacker.transform.root.gameObject : null;
        }

        /// <summary>True khi có hit chưa clear và attacker cùng root với <paramref name="player"/> (blackboard Player).</summary>
        public bool IsPendingHitFrom(GameObject player)
        {
            if (!_pending || player == null) return false;
            if (_attackerRoot == null) return true;
            return _attackerRoot == player.transform.root.gameObject;
        }

        public void ClearPending()
        {
            _pending = false;
            _attackerRoot = null;
        }
    }
}
