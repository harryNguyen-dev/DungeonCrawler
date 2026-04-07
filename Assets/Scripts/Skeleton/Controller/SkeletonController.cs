using Combat;
using Enemy;
using UnityEngine;

/// <summary>
/// Điều phối phản ứng khi skeleton bị đánh: HP, knockback, animation.
/// </summary>
public class SkeletonController : MonoBehaviour, IDamageReceiver
{
    [SerializeField] HealthSystem _health;
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] SkeletonAnimator _skeletonAnimator;

    void Awake()
    {
        if (_health == null)
            _health = GetComponent<HealthSystem>();
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();
        if (_skeletonAnimator == null)
            _skeletonAnimator = GetComponentInChildren<SkeletonAnimator>();
    }

    public void ReceiveHit(in DamageHitInfo info)
    {
        if (_health != null)
            _health.TakeDamage(info.Damage);

        if (_rigidbody != null)
        {
            Vector3 dir = info.KnockbackDirection;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                dir.Normalize();
            _rigidbody.linearVelocity = Vector3.zero;
            float forceKnockback = info.Damage * 0.1f;
            dir.y = 0.1f;
            _rigidbody.AddForce(dir * info.KnockbackImpulse + Vector3.up * info.VerticalImpulse * forceKnockback, ForceMode.Impulse);
        }

        if (_skeletonAnimator != null)
            _skeletonAnimator.TriggerTakeDamage();
    }
}
