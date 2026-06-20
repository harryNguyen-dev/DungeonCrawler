#if UNITY_EDITOR
using SO;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    public static class HeroDataBootstrap
    {
        private const string HeroFolder = "Assets/SO/Hero";
        private const string HeroAssetsFolder = "Assets/SO/Hero/Heroes";
        private const string SkillFolder = "Assets/SO/Hero/Skills";
        private const string WeaponFolder = "Assets/SO/Weapon/Weapons";
        private const string VisualFolder = "Assets/Prefabs/Characters/Hero/Visuals";
        private const string CatalogPath = "Assets/SO/Hero/HeroCatalog_Global.asset";
        private const string DashConfigPath = "Assets/SO/Hero/DashConfig_Default.asset";
        private const string PlayerPrefabPath = "Assets/Prefabs/Characters/Hero/Player.prefab";
        private const string FinnProjectilePath = "Assets/Prefabs/Projectile/FinnAttack.prefab";
        private const string OthaProjectilePath = "Assets/Prefabs/Projectile/OThaAttack.prefab";
        private const string LunaProjectilePath = "Assets/Prefabs/Projectile/LunaAttack.prefab";

        [MenuItem("DungeonCrawler/Bootstrap Hero Data")]
        public static void Bootstrap()
        {
            EnsureFolders();

            var dashConfig = LoadOrCreateAsset<DashConfigSO>(DashConfigPath, "DashConfig_Default");
            var finnProjectile = AssetDatabase.LoadAssetAtPath<GameObject>(FinnProjectilePath);
            var othaProjectile = AssetDatabase.LoadAssetAtPath<GameObject>(OthaProjectilePath);
            var lunaProjectile = AssetDatabase.LoadAssetAtPath<GameObject>(LunaProjectilePath);
            var finnVisual = LoadVisualPrefab("HeroVisual_Finn") ?? EnsureDefaultVisualPrefab();
            var othaVisual = LoadVisualPrefab("HeroVisual_OTha") ?? finnVisual;
            var lunaVisual = LoadVisualPrefab("HeroVisual_Luna") ?? finnVisual;

            var finnWeapon = EnsureWeapon("Weapon_Starter", "weapon_starter", "Pulse Rifle",
                finnProjectile, WeaponEffectType.NumberOfProjectiles, 1);
            var othaWeapon = EnsureWeapon("Weapon_Otha", "weapon_otha", "Pyro Cannon",
                othaProjectile,
                WeaponEffectType.NumberOfProjectiles, 5,
                WeaponEffectType.FireDamage, 6);
            var lunaWeapon = EnsureWeapon("Weapon_Luna", "weapon_luna", "Moonbow",
                lunaProjectile,
                WeaponEffectType.NumberOfProjectiles, 1,
                WeaponEffectType.PierceCount, 1);

            var finnSkill = CreateConeSkill("Skill_Starter", "skill_starter", "Pulse Burst", 3f, finnProjectile);
            var othaSkill = CreateOverdriveSkill("Skill_Otha_Overdrive", "skill_otha_overdrive", "Inferno Overdrive");
            var lunaSkill = CreateSelfBuffSkill("Skill_Luna_FrostVeil", "skill_luna_frost_veil", "Frost Veil",
                8f, 6f, StatModifierType.FrozenDurationFlat, 2.5f);

            var heroFinn = CreateHero("Hero_Finn", "hero_starter", "Finn",
                finnWeapon, finnSkill, finnVisual, true, 0, 0,
                maxHealth: 240, moveSpeed: 10, attackDamage: 16, attackCooldown: 0.4f, critChance: 0.3f,
                CreateFinnUpgrades());
            var heroOtha = CreateHero("Hero_OTha", "hero_otha", "Otha",
                othaWeapon, othaSkill, othaVisual, false, 1, 350,
                maxHealth: 150, moveSpeed: 10, attackDamage: 18, attackCooldown: 0.5f, critChance: 0.1f,
                CreateOthaUpgrades());
            var heroLuna = CreateHero("Hero_Luna", "hero_luna", "Luna",
                lunaWeapon, lunaSkill, lunaVisual, false, 2, 350,
                maxHealth: 170, moveSpeed: 11, attackDamage: 22, attackCooldown: 0.45f, critChance: 0.1f,
                CreateLunaUpgrades());

            var catalog = LoadOrCreateAsset<HeroCatalogSO>(CatalogPath, "HeroCatalog_Global");
            catalog.heroes = new System.Collections.Generic.List<HeroSO>
            {
                heroFinn, heroOtha, heroLuna
            };

            EditorUtility.SetDirty(catalog);
            EditorUtility.SetDirty(dashConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WirePlayerPrefab(dashConfig);
            Debug.Log("[HeroDataBootstrap] Finn / Otha / Luna hero data created/updated.");
        }

        private static HeroUpgradeStep[] CreateFinnUpgrades() => new[]
        {
            new HeroUpgradeStep { cost = 120, damageBonus = 3, healthBonus = 30, cooldownReduction = 0.03f, critChanceBonus = 0.02f },
            new HeroUpgradeStep { cost = 150, damageBonus = 3, healthBonus = 30, cooldownReduction = 0.03f, critChanceBonus = 0.03f },
            new HeroUpgradeStep { cost = 180, damageBonus = 3, healthBonus = 30, cooldownReduction = 0.02f, critChanceBonus = 0.03f },
            new HeroUpgradeStep { cost = 220, damageBonus = 3, healthBonus = 30, cooldownReduction = 0.02f, critChanceBonus = 0.02f },
        };

        private static HeroUpgradeStep[] CreateOthaUpgrades() => new[]
        {
            new HeroUpgradeStep { cost = 140, damageBonus = 2, healthBonus = 15, cooldownReduction = 0.02f, fireDamageBonus = 1 },
            new HeroUpgradeStep { cost = 170, damageBonus = 2, healthBonus = 15, cooldownReduction = 0.02f, fireDamageBonus = 1 },
            new HeroUpgradeStep { cost = 200, damageBonus = 1, healthBonus = 15, cooldownReduction = 0.02f, critChanceBonus = 0.02f },
            new HeroUpgradeStep { cost = 230, damageBonus = 1, healthBonus = 15, cooldownReduction = 0.02f, critChanceBonus = 0.01f },
        };

        private static HeroUpgradeStep[] CreateLunaUpgrades() => new[]
        {
            new HeroUpgradeStep { cost = 130, damageBonus = 2, healthBonus = 25, cooldownReduction = 0.02f, critChanceBonus = 0.02f },
            new HeroUpgradeStep { cost = 160, damageBonus = 2, healthBonus = 25, cooldownReduction = 0.02f, critChanceBonus = 0.02f },
            new HeroUpgradeStep { cost = 190, damageBonus = 2, healthBonus = 25, cooldownReduction = 0.02f, critChanceBonus = 0.01f },
            new HeroUpgradeStep { cost = 220, damageBonus = 2, healthBonus = 25, cooldownReduction = 0.02f, critChanceBonus = 0.01f },
        };

        private static void EnsureFolders()
        {
            CreateFolder("Assets/SO", "Hero");
            CreateFolder(HeroFolder, "Heroes");
            CreateFolder(HeroFolder, "Skills");
            CreateFolder("Assets/Prefabs/Characters/Hero", "Visuals");
        }

        private static void CreateFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static WeaponSO EnsureWeapon(string fileName, string weaponId, string displayName,
            GameObject projectilePrefab, params object[] effectPairs)
        {
            var path = $"{WeaponFolder}/{fileName}.asset";
            var weapon = LoadOrCreateAsset<WeaponSO>(path, fileName);
            weapon.weaponId = weaponId;
            weapon.displayName = displayName;
            weapon.description = displayName;
            weapon.projectilePrefab = projectilePrefab;
            weapon.intrinsicEffects = new System.Collections.Generic.List<WeaponEffectModifier>();

            for (var i = 0; i + 1 < effectPairs.Length; i += 2)
            {
                weapon.intrinsicEffects.Add(new WeaponEffectModifier
                {
                    EffectType = (WeaponEffectType)effectPairs[i],
                    Value = System.Convert.ToSingle(effectPairs[i + 1])
                });
            }

            EditorUtility.SetDirty(weapon);
            return weapon;
        }

        private static HeroSkillSO CreateConeSkill(string fileName, string skillId, string displayName,
            float cooldown, GameObject projectilePrefab)
        {
            var path = $"{SkillFolder}/{fileName}.asset";
            var skill = LoadOrCreateAsset<HeroSkillSO>(path, fileName);
            skill.skillId = skillId;
            skill.displayName = displayName;
            skill.description = displayName;
            skill.cooldown = cooldown;
            skill.damageMode = SkillDamageMode.RollAttackDamage;
            skill.damagePercent = 0.45f;
            skill.deliveryType = SkillDeliveryType.Cone;
            skill.skillProjectilePrefab = projectilePrefab;
            skill.projectileSpeed = 20f;
            skill.range = 12f;
            skill.coneConfig = new ConeSkillConfig
            {
                projectileCount = 8,
                coneAngle = 50f,
                centerPelletPierce = true
            };
            EditorUtility.SetDirty(skill);
            return skill;
        }

        private static HeroSkillSO CreateOverdriveSkill(string fileName, string skillId, string displayName)
        {
            var path = $"{SkillFolder}/{fileName}.asset";
            var skill = LoadOrCreateAsset<HeroSkillSO>(path, fileName);
            skill.skillId = skillId;
            skill.displayName = displayName;
            skill.description = displayName;
            skill.cooldown = 12f;
            skill.damage = 0;
            skill.deliveryType = SkillDeliveryType.SelfBuff;
            skill.buffConfig = new BuffSkillConfig
            {
                duration = 5f,
                refreshOnReuse = true,
                modifiers = new System.Collections.Generic.List<StatModifier>
                {
                    new StatModifier { type = StatModifierType.AttackCooldownFlat, value = -0.15f },
                    new StatModifier { type = StatModifierType.AttackDamagePercent, value = 0.35f },
                    new StatModifier { type = StatModifierType.FireDamageFlat, value = 4f },
                }
            };
            EditorUtility.SetDirty(skill);
            return skill;
        }

        private static HeroSkillSO CreateSelfBuffSkill(string fileName, string skillId, string displayName,
            float cooldown, float duration, StatModifierType modifierType, float modifierValue)
        {
            var path = $"{SkillFolder}/{fileName}.asset";
            var skill = LoadOrCreateAsset<HeroSkillSO>(path, fileName);
            skill.skillId = skillId;
            skill.displayName = displayName;
            skill.description = displayName;
            skill.cooldown = cooldown;
            skill.damage = 0;
            skill.deliveryType = SkillDeliveryType.SelfBuff;
            skill.buffConfig = new BuffSkillConfig
            {
                duration = duration,
                refreshOnReuse = true,
                modifiers = new System.Collections.Generic.List<StatModifier>
                {
                    new StatModifier { type = modifierType, value = modifierValue }
                }
            };
            EditorUtility.SetDirty(skill);
            return skill;
        }

        private static HeroSO CreateHero(string fileName, string heroId, string displayName,
            WeaponSO weapon, HeroSkillSO skill, GameObject visualPrefab,
            bool unlockedByDefault, int sortOrder, int unlockCost,
            int maxHealth, int moveSpeed, int attackDamage, float attackCooldown, float critChance,
            HeroUpgradeStep[] upgrades)
        {
            var path = $"{HeroAssetsFolder}/{fileName}.asset";
            var hero = LoadOrCreateAsset<HeroSO>(path, fileName);
            hero.heroId = heroId;
            hero.displayName = displayName;
            hero.description = displayName;
            hero.sortOrder = sortOrder;
            hero.boundWeapon = weapon;
            hero.skill = skill;
            if (visualPrefab != null)
                hero.visualPrefab = visualPrefab;
            hero.unlockedByDefault = unlockedByDefault;
            hero.unlockCost = unlockCost;
            hero.maxHealth = maxHealth;
            hero.moveSpeed = moveSpeed;
            hero.attackDamage = attackDamage;
            hero.attackCooldown = attackCooldown;
            hero.critChance = critChance;
            hero.upgrades = upgrades;

            EditorUtility.SetDirty(hero);
            return hero;
        }

        private static GameObject LoadVisualPrefab(string fileName)
        {
            var path = $"{VisualFolder}/{fileName}.prefab";
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject EnsureDefaultVisualPrefab()
        {
            var visualPath = $"{VisualFolder}/HeroVisual_Default.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(visualPath);
            if (existing != null)
                return existing;

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab == null)
            {
                Debug.LogWarning("[HeroDataBootstrap] Player prefab not found; visual prefab skipped.");
                return null;
            }

            var playerInstance = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            if (playerInstance == null)
                return null;

            StripGameplayComponents(playerInstance);

            while (playerInstance.transform.childCount > 0)
            {
                var child = playerInstance.transform.GetChild(0);
                if (child.name is "Fire point" or "FirePoint" or "VisualContainer")
                {
                    child.SetParent(null);
                    Object.DestroyImmediate(child.gameObject);
                    continue;
                }
                Object.DestroyImmediate(child.gameObject);
            }

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(playerInstance.transform, false);
            firePoint.transform.localPosition = new Vector3(0f, 1f, 0.5f);

            PrefabUtility.SaveAsPrefabAsset(playerInstance, visualPath);
            Object.DestroyImmediate(playerInstance);
            return AssetDatabase.LoadAssetAtPath<GameObject>(visualPath);
        }

        private static void StripGameplayComponents(GameObject root)
        {
            foreach (var component in root.GetComponents<Component>())
            {
                if (component is Transform)
                    continue;
                Object.DestroyImmediate(component);
            }
        }

        private static void WirePlayerPrefab(DashConfigSO dashConfig)
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (root == null)
                return;

            var visualContainer = root.transform.Find("VisualContainer");
            if (visualContainer == null)
            {
                var containerGo = new GameObject("VisualContainer");
                visualContainer = containerGo.transform;
                visualContainer.SetParent(root.transform, false);
            }

            var fallbackFirePoint = root.transform.Find("Fire point");

            var visualController = root.GetComponent<PlayerController.HeroVisualController>();
            if (visualController == null)
                visualController = root.AddComponent<PlayerController.HeroVisualController>();

            var so = new SerializedObject(visualController);
            so.FindProperty("visualContainer").objectReferenceValue = visualContainer;
            so.FindProperty("fallbackFirePoint").objectReferenceValue = fallbackFirePoint;
            so.ApplyModifiedPropertiesWithoutUndo();

            if (root.GetComponent<PlayerController.PlayerDash>() == null)
            {
                var dash = root.AddComponent<PlayerController.PlayerDash>();
                var dashSo = new SerializedObject(dash);
                dashSo.FindProperty("dashConfig").objectReferenceValue = dashConfig;
                dashSo.ApplyModifiedPropertiesWithoutUndo();
            }

            if (root.GetComponent<PlayerController.PlayerSkill>() == null)
                root.AddComponent<PlayerController.PlayerSkill>();

            if (root.GetComponent<PlayerController.Skill.PlayerTimedBuffTracker>() == null)
                root.AddComponent<PlayerController.Skill.PlayerTimedBuffTracker>();

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static T LoadOrCreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            asset.name = assetName;
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
#endif
