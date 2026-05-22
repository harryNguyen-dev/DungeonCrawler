using System;
using UnityEngine;

namespace EnemyController
{
    public class EnemyEvents : MonoBehaviour
    {
        public event Action<int> OnHealthChange;

        public void ChangeHealth(int health)
        {
            OnHealthChange?.Invoke(health);
        }
    }
}