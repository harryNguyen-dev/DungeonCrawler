using UnityEngine;

namespace Projectile 
{

    public class ProjectileMove : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 2f;
        private float damage;
        private void Start()
        {
            // Tự hủy sau một khoảng thời gian để tránh tràn bộ nhớ
            Destroy(gameObject, lifeTime);
        }
        public void SetDamage(float damage) 
        {
            this.damage = damage;
        }

        private void Update()
        {
            // Di chuyển về phía trước theo hướng của Object
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Kiểm tra nếu chạm vào Quái
            if (other.CompareTag("Enemy"))
            {
                var health = other.GetComponent<EnemyController.Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
                Destroy(gameObject); // Hủy sóng kiếm khi trúng đích
            }
        }
    }

}