using Combat;
using UnityEngine;
namespace Player
{
    public class PlayerHealth : HealthSystem, IDamageReceiver
    {
        public override void Die()
        {
            base.Die();
            Destroy(gameObject);
        }

        public void ReceiveHit(in DamageHitInfo info)
        {
            TakeDamage(info.Damage);
        }
    }
}