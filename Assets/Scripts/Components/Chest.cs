using Core;
using DG.Tweening;
using UnityEngine;

namespace Components
{
    public class Chest : MonoBehaviour
    {
        [SerializeField] private Transform chestTop;
        [SerializeField] private int maxHP = 40;
        [SerializeField] private int goldDrop = 100;
        [SerializeField] private int goldPickupCount = 8;
        [SerializeField] private float lidOpenAngle = -110f;
        [SerializeField] private float lidOpenDuration = 0.35f;

        private int _currentHP;
        private bool _isDestroyed;
        private Collider _collider;

        public bool IsDestroyed => _isDestroyed;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _currentHP = Mathf.Max(1, maxHP);
        }

        public void TakeDamage(int damage)
        {
            if (_isDestroyed || damage <= 0) return;

            _currentHP -= damage;
            GameAudio.PlayEnemyHit(transform.position);

            if (_currentHP <= 0)
                DestroyChest();
        }

        private void DestroyChest()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            if (_collider != null)
                _collider.enabled = false;

            OpenLid();
            DropPool.Instance?.SpawnGoldBurst(transform.position, goldDrop, goldPickupCount);
        }

        private void OpenLid()
        {
            if (chestTop == null) return;

            chestTop.DOKill();
            chestTop.DOLocalRotate(new Vector3(lidOpenAngle, 0f, 0f), lidOpenDuration)
                .SetEase(Ease.OutBack);
        }

        private void OnDestroy() => chestTop?.DOKill();
    }
}
