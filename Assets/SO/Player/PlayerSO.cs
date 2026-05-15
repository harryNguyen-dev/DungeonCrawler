using UnityEngine;

namespace SO {

    [CreateAssetMenu(fileName = "Player", menuName = "Player", order = 0)]
    public class PlayerSO : ScriptableObject
    {
        public float AttackCooldown;
        public float AttackDamage;
        public float MoveSpeed;
        public float MaxHealth;
    }
}
