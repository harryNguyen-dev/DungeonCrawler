using Core;
using Global;
using SO;
using UnityEngine;

namespace PlayerController
{
    public class PlayerStats : MonoBehaviour
    {
        public PlayerSO configData;
        public PlayerSO runtimeStats;

        public int currentLevel = 1;
        public int currentExp = 0;
        public int expToNextLevel = 100;

        public HeroSO EquippedHero { get; private set; }
        public WeaponSO EquippedWeapon { get; private set; }
        public HeroSkillSO ActiveSkill { get; private set; }

        private PlayerEvents events;
        private HeroVisualController heroVisual;
        private PlayerSkill playerSkill;
        private Attack attack;

        private void Awake()
        {
            events = GetComponent<PlayerEvents>();
            heroVisual = GetComponent<HeroVisualController>();
            playerSkill = GetComponent<PlayerSkill>();
            attack = GetComponent<Attack>();
            ApplyEquippedHeroConfig();
        }

        public void ApplyEquippedHeroConfig()
        {
            var loadout = HeroLoadoutBuilder.BuildForEquippedHero();
            if (loadout.Stats != null)
                configData = loadout.Stats;

            EquippedHero = loadout.Hero;
            EquippedWeapon = loadout.Weapon;
            ActiveSkill = loadout.Skill;

            runtimeStats = Instantiate(configData);
            runtimeStats.InitializeRuntimeDictionary();

            heroVisual?.ApplyHeroVisual(EquippedHero);
            playerSkill?.SetActiveSkill(ActiveSkill);
            attack?.ApplyWeapon(EquippedWeapon);
        }

        public void CollectExp(int baseAmount)
        {
            if (baseAmount <= 0) return;

            currentExp += Mathf.RoundToInt(baseAmount * runtimeStats.DefaultExpGainMultiplier);
            events.InvokeExpChanged(currentExp, expToNextLevel);

            if (currentExp >= expToNextLevel)
                LevelUp();
        }

        private void LevelUp()
        {
            currentLevel++;
            currentExp = 0;
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.2f);
            events.InvokeExpChanged(currentExp, expToNextLevel);

            Debug.Log($"<color=yellow>LEVEL UP! Current Level: {currentLevel}</color>");

            GlobalEvents.RaiseLevelUp(currentLevel);
            GlobalEvents.RaiseRequestBattleCard();
            Time.timeScale = 0f;
        }

        public void RestartGame()
        {
            currentLevel = 1;
            currentExp = 0;
            expToNextLevel = 100;
            Time.timeScale = 1f;
            ApplyEquippedHeroConfig();
            events.InvokeExpChanged(currentExp, expToNextLevel);
        }

        public float GetAttackCooldown() => runtimeStats.AttackCooldown;
        public int GetAttackDamage() => runtimeStats.AttackDamage;
        public float GetCritChance() => runtimeStats.CritChance;
        public int GetMoveSpeed() => runtimeStats.MoveSpeed;
        public int GetMaxHealth() => runtimeStats.MaxHealth;

        public int RollAttackDamage()
        {
            var baseDamage = GetAttackDamage();
            if (Random.value < GetCritChance())
                return Mathf.RoundToInt(baseDamage * HeroSO.CritDamageMultiplier);

            return baseDamage;
        }

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
            var weaponModify = new WeaponEffectModifier
            {
                EffectType = WeaponEffectType.NumberOfProjectiles,
                Value = amount
            };
            runtimeStats.AddpendWeaponModifier(weaponModify);
            events.InvokeNumberOfProjectileChanged(Mathf.RoundToInt(runtimeStats.RuntimeEffects[WeaponEffectType.NumberOfProjectiles]));
        }

        public void AddProjectileFireOnHit(int amount)
        {
            runtimeStats.AddpendWeaponModifier(new WeaponEffectModifier
            {
                EffectType = WeaponEffectType.FireDamage,
                Value = amount
            });
        }

        public void AddProjectileFrozenOnHit(int amount)
        {
            runtimeStats.AddpendWeaponModifier(new WeaponEffectModifier
            {
                EffectType = WeaponEffectType.FrozenDuration,
                Value = amount
            });
        }

        public void AddProjectilePierce()
        {
            runtimeStats.AddpendWeaponModifier(new WeaponEffectModifier
            {
                EffectType = WeaponEffectType.PierceCount,
                Value = 1
            });
        }

        public void AddProjectileBoomerange()
        {
            runtimeStats.AddpendWeaponModifier(new WeaponEffectModifier
            {
                EffectType = WeaponEffectType.BoomerangMode,
                Value = 1
            });
        }
    }
}
