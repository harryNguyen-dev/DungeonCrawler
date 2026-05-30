using UnityEngine;

namespace SO
{

    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Enemy Data")]
    public class EnemySO : ScriptableObject
    {
        [Header("Base Stats")]
        public string EnemyName = "Base Enemy";
        public int MaxHealth = 100;
        public float MoveSpeed = 3.5f;

        [Header("Combat Settings")]
        public int Damage = 30;
        public float AttackRange = 2f;
        public float AttackCooldown = 1.5f;

        [Header("Knockback Settings")]
        public float KnockbackForce = 6f;
        public float KnockbackDuration = 0.12f;

        [Header("Boss")]
        [Tooltip("Đánh dấu enemy này là boss (chết → win run).")]
        public bool isBoss;

        [Header("Rewards")]
        [Min(0)] public int GoldDrop = 5;
    }

}