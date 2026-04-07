using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Dữ liệu một lần trúng đòn — vũ khí/hitbox chỉ gói và gửi, bên nhận xử lý HP, knockback, anim.
    /// </summary>
    public readonly struct DamageHitInfo
    {
        public int Damage { get; }
        public Vector3 KnockbackDirection { get; }
        public float KnockbackImpulse { get; }
        public float VerticalImpulse { get; }

        public DamageHitInfo(int damage, Vector3 knockbackDirection, float knockbackImpulse, float verticalImpulse)
        {
            Damage = damage;
            KnockbackDirection = knockbackDirection;
            KnockbackImpulse = knockbackImpulse;
            VerticalImpulse = verticalImpulse;
        }
    }

    /// <summary>
    /// Quái / vật thể có thể nhận sát thương; logic phản ứng nằm trong class implement.
    /// </summary>
    public interface IDamageReceiver
    {
        void ReceiveHit(in DamageHitInfo info);
    }
}
