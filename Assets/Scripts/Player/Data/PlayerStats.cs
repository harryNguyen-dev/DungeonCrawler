using System;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

namespace PlayerController
{
    public class PlayerStats : MonoBehaviour
    {

        public SO.PlayerSO configData;
        public SO.PlayerSO runtimeStats; // Bản sao để chạy runtime

        public int currentLevel = 1;
        public int currentExp = 0;
        public int expToNextLevel = 100;

        private PlayerEvents events;
        private PlayerController.Health playerHealth;
        private void Awake()
        {
            playerHealth = GetComponent<Health>();
            events = GetComponent<PlayerEvents>();
            ApplyEquippedWeaponConfig();
        }

        public void ApplyEquippedWeaponConfig()
        {
            var built = Core.WeaponLoadoutBuilder.BuildForEquippedWeapon();
            if (built != null)
                configData = built;

            runtimeStats = Instantiate(configData);
            runtimeStats.InitializeRuntimeDictionary();
        }

        private void OnEnable() => Global.GlobalEvents.OnEnemyDie += AddExperience;
        private void OnDisable() => Global.GlobalEvents.OnEnemyDie -= AddExperience;

        private void AddExperience(int _)
        {
            currentExp += Mathf.RoundToInt(20 * runtimeStats.DefaultExpGainMultiplier); // Mỗi quái cho 20 Exp
            Debug.Log($"Exp: {currentExp}/{expToNextLevel}");
            events.InvokeExpChanged(currentExp, expToNextLevel);

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
            events.InvokeExpChanged(currentExp, expToNextLevel);

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
            ApplyEquippedWeaponConfig();
            events.InvokeExpChanged(currentExp, expToNextLevel);
        }
        // Hàm bổ trợ để các Script khác lấy chỉ số đã được nâng cấp
        public float GetAttackCooldown() => runtimeStats.AttackCooldown;
        public int GetAttackDamage() => runtimeStats.AttackDamage;
        public int GetMoveSpeed() => runtimeStats.MoveSpeed;
        public int GetMaxHealth() => runtimeStats.MaxHealth;

        public void UpgradeAttackSpeed(float amount)
        {
            runtimeStats.AttackCooldown -= amount;
            events.InvokeAttackSpeedChanged(runtimeStats.AttackCooldown);
        }
        
        public void UpgradeAttackDamage(int amount)
        {
            runtimeStats.AttackDamage += amount;
            events.InvokeAttackDamageChanged(runtimeStats.AttackDamage);
        }

        public void UpgradeMaxHealth(int amount)
        {
            runtimeStats.MaxHealth += amount;
            events.InvokeMaxHealthChanged(runtimeStats.MaxHealth);
        }

        public void HealHealth(int amount)
        {
            events.InvokeHealHealth(amount);
        }

        public void UpgradeIncreaseAmor(int amount)
        {
            runtimeStats.Amor += amount;
            events.InvokeIncreaseAmor(runtimeStats.Amor);
        }
        public void UpgradeIncreaseRunSpeed(float amount)
        {
            runtimeStats.MoveSpeed += Mathf.RoundToInt(amount);
            events.InvokeIncreaseMoveSpeed(runtimeStats.MoveSpeed);
        }
        public void UpgradeIncreaseExpGain(float amount)
        {
            runtimeStats.DefaultExpGainMultiplier += amount;
        }
        public void UpgradeIncreaseGoldGain(float amount)
        {
            runtimeStats.DefaultGoldGainMultiplier += amount;
        }
        public void AddOneProjectile(int amount)
        {
            Debug.Log($"[PlayerStats] Add {amount} projectiles");
            var weaponModify = new SO.WeaponEffectModifier()
            {
                EffectType = SO.WeaponEffectType.NumberOfProjectiles,
                Value = amount
            };
            runtimeStats.AddpendWeaponModifier(weaponModify);
            events.InvokeNumberOfProjectileChanged(Mathf.RoundToInt(runtimeStats.RuntimeEffects[SO.WeaponEffectType.NumberOfProjectiles]));
        }
        public void AddProjectileFireOnHit(int amount)
        {
            var weaponModify = new SO.WeaponEffectModifier()
            {
                EffectType = SO.WeaponEffectType.FireDamage,
                Value = amount
            };
            runtimeStats.AddpendWeaponModifier(weaponModify);
        }
        public void AddProjectileFrozenOnHit(int amount)
        {
            var weaponModify = new SO.WeaponEffectModifier()
            {
                EffectType = SO.WeaponEffectType.FrozenDuration,
                Value = amount
            };
            runtimeStats.AddpendWeaponModifier(weaponModify);
        }
        public void AddProjectilePierce()
        {
            var weaponModify = new SO.WeaponEffectModifier()
            {
                EffectType = SO.WeaponEffectType.PierceCount,
                Value = 1
            };
            runtimeStats.AddpendWeaponModifier(weaponModify);
        }
        public void AddProjectileBoomerange()
        {
            var weaponModify = new SO.WeaponEffectModifier()
            {
                EffectType = SO.WeaponEffectType.BoomerangMode,
                Value = 1
            };
            runtimeStats.AddpendWeaponModifier(weaponModify);
        }
    }
}
