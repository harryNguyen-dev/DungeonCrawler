using Core;
using UnityEngine;

namespace EnemyController
{
    public class EnemyProjectile : MonoBehaviour, IPoolable
    {
        [Header("Projectile Movement")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private float maxLifetime = 5f;

        private int damage;
        private float currentLifetime;
        private bool isInitialized = false;

        // Hàm thiết lập chỉ số viên đạn được gọi từ EnemyRangedAttack khi bắn
        public void Setup(int damageValue)
        {
            damage = damageValue;
            currentLifetime = 0f;
            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized) return;

            // Di chuyển viên đạn tịnh tiến về phía trước theo trục Z của chính nó
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            // Cơ chế tự hủy theo thời gian nếu bay lạc không trúng mục tiêu (Tránh lọt bộ nhớ)
            currentLifetime += Time.deltaTime;
            if (currentLifetime >= maxLifetime)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Kiểm tra va chạm với Player
            if (other.CompareTag("Player"))
            {
                // Tìm component quản lý máu của Player từ kiến trúc toàn cục Global
                var playerHealth = Global.GlobalEntities.Instance?.PlayerHealth;
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }

                // Có thể sinh thêm VFX nổ trúng Player tại đây:
                // var hitEffect = Global.GlobalEntities.Instance.playerHitEffect;
                // if (hitEffect != null) hitEffect.PlayHitEffect(transform.position, Quaternion.identity);

                // Biến mất ngay lập tức sau khi chạm mục tiêu
                ReturnToPool();
            }
            // // Tùy chọn: Hủy đạn nếu đập vào tường/vật cản môi trường
            // else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
            // {
            //     ReturnToPool();
            // }
        }

        private void ReturnToPool()
        {
            isInitialized = false;
            if (ObjectPoolingManager.Instance != null)
            {
                ObjectPoolingManager.Instance.Return(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OnSpawnedFromPool()
        {
            currentLifetime = 0f;
            isInitialized = false;
        }

        public void OnReturnedToPool()
        {
            isInitialized = false;
        }
    }
}