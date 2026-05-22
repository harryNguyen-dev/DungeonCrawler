using System;
using UnityEngine;

namespace PlayerController
{
    public class PlayerEvents : MonoBehaviour
    {
        public event Action<int> OnAttackDamageChanged;
        public void InvokeAttackDamageChanged(int amount) => OnAttackDamageChanged?.Invoke(amount);

        public event Action<float> OnAttackSpeedChanged;
        public void InvokeAttackSpeedChanged(float amount) => OnAttackSpeedChanged?.Invoke(amount);

        public event Action<int> OnMaxHealthChanged;
        public void InvokeMaxHealthChanged(int amount) => OnMaxHealthChanged?.Invoke(amount);
        
        public event Action<int, int> OnHealthChanged;
        public void InvokeChangeHealth(int health, int maxHealth) => OnHealthChanged?.Invoke(health, maxHealth);
        
        public event Action<int> OnHealHealth;
        public void InvokeHealHealth(int amount) => OnHealHealth?.Invoke(amount);

        public event Action<int> OnIncreaseAmor;
        public void InvokeIncreaseAmor(int amount) => OnIncreaseAmor?.Invoke(amount);
        
        public event Action<int> OnIncreaseMoveSpeed;
        public void InvokeIncreaseMoveSpeed(int amount) => OnIncreaseMoveSpeed?.Invoke(amount);
        
        public event Action<int> OnNumberOfProjectileChanged;
        public void InvokeNumberOfProjectileChanged(int amount) => OnNumberOfProjectileChanged?.Invoke(amount);
        
    }
}
