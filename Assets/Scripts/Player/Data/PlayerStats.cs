using System;
using UnityEngine;

namespace PlayerController
{
    public class PlayerStats : MonoBehaviour
    {
        public event Action<float> OnAttackSpeedChanged;

        public SO.PlayerSO playerSO;
        private SO.PlayerSO runtimeStats; // Bản sao để chạy runtime

        public int currentLevel = 1;
        public int currentExp = 0;
        public int expToNextLevel = 100;

        private void Awake()
        {
            runtimeStats = Instantiate(playerSO);
        }

        private void OnEnable() => Global.GlobalEvents.OnEnemyDie += AddExperience;
        private void OnDisable() => Global.GlobalEvents.OnEnemyDie -= AddExperience;

        private void AddExperience()
        {
            currentExp += 20; // Mỗi quái cho 20 Exp
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
            runtimeStats = Instantiate(playerSO);
        }
        // Hàm bổ trợ để các Script khác lấy chỉ số đã được nâng cấp
        public float GetAttackCooldown() => runtimeStats.AttackCooldown;
        public float GetAttackDamage() => runtimeStats.AttackDamage;
        public float GetMoveSpeed() => runtimeStats.MoveSpeed;
        public float GetHealth() => runtimeStats.MaxHealth;
        public void UpgradeAttackSpeed(float amount)
        {
            runtimeStats.AttackCooldown -= amount;
            OnAttackSpeedChanged?.Invoke(runtimeStats.AttackCooldown);
        }
        public void UpgradeAttackDamage(float amount) => runtimeStats.AttackDamage += amount;
    }
}
