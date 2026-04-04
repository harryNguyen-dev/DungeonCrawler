using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    public class HitboxController : MonoBehaviour
    {
         [SerializeField] Collider _collider;
        public int damage = 30;
        public float knockback = 5f;

        HashSet<Collider> _hitThisSwing = new();   // Tránh hit cùng enemy 2 lần

        void Awake() {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            SetActive(false);
        }

        public void SetActive(bool value) {
            _collider.enabled = value;
            if (!value) _hitThisSwing.Clear();   // Reset khi tắt
        }

        void OnTriggerEnter(Collider other) {
            if (_hitThisSwing.Contains(other)) return;
            // if (!other.TryGetComponent<HealthSystem>(out var hp)) return;

            _hitThisSwing.Add(other);
            // hp.TakeDamage(damage);

            // Knockback
            if (other.TryGetComponent<Rigidbody>(out var rb)) {
                Vector3 dir = (other.transform.position - transform.position).normalized;
                rb.AddForce(dir * knockback + Vector3.up * 2f, ForceMode.Impulse);
            }
        }
    }
}