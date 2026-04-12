using Combat;

using Enemy;

using UnityEngine;



/// <summary>

/// Điều phối phản ứng khi skeleton bị đánh: HP, knockback, animation; ghi <see cref="EnemyHitSignal"/> cho Behavior Tree.

/// Dừng di chuyển / tấn công sau hit do graph (StopNavmesh, Wait, ClearPending, v.v.) xử lý.

/// </summary>

public class SkeletonController : MonoBehaviour, IDamageReceiver, ICombatEvents

{

    [SerializeField] HealthSystem _health;

    [SerializeField] Rigidbody _rigidbody;

    [SerializeField] SkeletonAnimator _skeletonAnimator;

    [SerializeField] HitboxController _hitbox;



    EnemyHitSignal _hitSignal;



    void Awake()

    {

        if (_health == null)

            _health = GetComponent<HealthSystem>();

        if (_rigidbody == null)

            _rigidbody = GetComponent<Rigidbody>();

        if (_skeletonAnimator == null)

            _skeletonAnimator = GetComponentInChildren<SkeletonAnimator>();

        _hitSignal = GetComponent<EnemyHitSignal>();

        if (_hitSignal == null)

            _hitSignal = gameObject.AddComponent<EnemyHitSignal>();

    }



    public void ReceiveHit(in DamageHitInfo info)

    {

        if (_health != null)

            _health.TakeDamage(info.Damage);



        if (_rigidbody != null)

            Knockback(info);



        if (_skeletonAnimator != null)

            _skeletonAnimator.TriggerTakeDamage();



        _hitSignal?.RegisterHit(info.Attacker);

    }



    void Knockback(DamageHitInfo info)

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



    public void AE_HitboxOn() => _hitbox.SetActive(true);

    public void AE_HitboxOff() => _hitbox.SetActive(false);

    public void AE_AttackEnd() { }



    public void AE_ParryEnd() { }



    public void AE_HitReactionEnd() { }

    public void AE_ParryStart()
    {
    }

    public void AE_ParryStop()
    {
    }
}


