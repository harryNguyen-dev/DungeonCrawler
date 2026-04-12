namespace Enemy
{
    public class EnemyHealth : HealthSystem
    {
        public override void Die()
        {
            base.Die();
            Destroy(gameObject); // TODO: using object pool
        }
    }
}