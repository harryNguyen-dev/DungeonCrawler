using System;
using UnityEngine;

namespace PlayerController
{
    public class PlayerStats : MonoBehaviour
    {
        public event Action<int> OnAttackDamageChanged;
        public event Action<float> OnAttackSpeedChanged;
        public event Action<int> OnMaxHealthChanged;
        public event Action<int> OnHealHealth;
        public event Action<int> OnIncreaseAmor;
        public event Action<int> OnIncreaseMoveSpeed;

        public SO.PlayerSO configData;
        private SO.PlayerSO runtimeStats; // Bản sao để chạy runtime

        public int currentLevel = 1;
        public int currentExp = 0;
        public int expToNextLevel = 100;

        private void Awake()
        {
            runtimeStats = Instantiate(configData);
            runtimeStats.InitializeRuntimeDictionary();
        }

        private void OnEnable() => Global.GlobalEvents.OnEnemyDie += AddExperience;
        private void OnDisable() => Global.GlobalEvents.OnEnemyDie -= AddExperience;

        private void AddExperience()
        {
            currentExp += Mathf.RoundToInt(20 * runtimeStats.DefaultExpGainMultiplier); // Mỗi quái cho 20 Exp
            Debug.Log($"Exp: {currentExp}/{expToNextLevel}");

            if (currentExp >= expToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            currentLevel++;
            currentExp = 0;
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.2f); // Tăng mốc Exp yêu cầu

            Debug.Log($"<color=yellow>LEVEL UP! Current Level: {currentLevel}</color>");

            // Bắn event để UI lắng nghe và hiện bảng chọn Card
            Global.GlobalEvents.RaiseLevelUp(currentLevel);
            Global.GlobalEvents.RaiseRequestBattleCard();

            // Dừng thời gian để người chơi chọn thẻ
            Time.timeScale = 0f;
        }
        public void RestartGame()
        {
            currentLevel = 1;
            currentExp = 0;
            expToNextLevel = 100;
            Time.timeScale = 1f;
            runtimeStats = Instantiate(configData);
        }
        // Hàm bổ trợ để các Script khác lấy chỉ số đã được nâng cấp
        public float GetAttackCooldown() => runtimeStats.AttackCooldown;
        public int GetAttackDamage() => runtimeStats.AttackDamage;
        public int GetMoveSpeed() => runtimeStats.MoveSpeed;
        public int GetMaxHealth() => runtimeStats.MaxHealth;

        public void UpgradeAttackSpeed(float amount)
        {
            runtimeStats.AttackCooldown -= amount;
            OnAttackSpeedChanged?.Invoke(runtimeStats.AttackCooldown);
        }
        
        public void UpgradeAttackDamage(int amount)
        {
            runtimeStats.AttackDamage += amount;
            OnAttackDamageChanged?.Invoke(runtimeStats.AttackDamage);
        }

        public void UpgradeMaxHealth(int amount)
        {
            runtimeStats.MaxHealth += amount;
            OnMaxHealthChanged?.Invoke(runtimeStats.MaxHealth);
        }

        public void HealHealth(int amount)
        {
            OnHealHealth?.Invoke(amount);
        }

        public void UpgradeIncreaseAmor(int amount)
        {
            runtimeStats.Amor += amount;
            OnIncreaseAmor?.Invoke(runtimeStats.Amor);
        }
        public void UpgradeIncreaseRunSpeed(float amount)
        {
            runtimeStats.MoveSpeed += Mathf.RoundToInt(amount);
            OnIncreaseMoveSpeed?.Invoke(runtimeStats.MoveSpeed);
        }
        public void UpgradeIncreaseExpGain(float amount)
        {
            runtimeStats.DefaultExpGainMultiplier += amount;
        }
        public void UpgradeIncreaseGoldGain(float amount)
        {
            runtimeStats.DefaultGoldGainMultiplier += amount;
        }
    }
}
