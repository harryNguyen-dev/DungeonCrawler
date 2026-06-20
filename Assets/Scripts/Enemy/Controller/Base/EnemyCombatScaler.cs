using SO;
using UnityEngine;

namespace EnemyController
{
    public class EnemyCombatScaler : MonoBehaviour
    {
        public float DamageMultiplier { get; private set; } = 1f;

        public void SetDamageMultiplier(float mult) => DamageMultiplier = mult > 0f ? mult : 1f;

        public static int GetEffectiveDamage(EnemySO data, EnemyCombatScaler scaler)
        {
            if (data == null)
                return 1;

            var mult = scaler != null ? scaler.DamageMultiplier : 1f;
            return Mathf.Max(1, Mathf.RoundToInt(data.Damage * mult));
        }

        public static EnemyCombatScaler Ensure(GameObject enemy)
        {
            if (enemy == null)
                return null;

            if (!enemy.TryGetComponent(out EnemyCombatScaler scaler))
                scaler = enemy.AddComponent<EnemyCombatScaler>();

            return scaler;
        }
    }
}
