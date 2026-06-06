#if UNITY_EDITOR
using Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditorTools
{
    public static class PoolManagerSetup
    {
        const string PrefabPath = "Assets/Prefabs/Core/PoolManager.prefab";
        const string BoostScenePath = "Assets/Scenes/Boost.unity";

        [MenuItem("DungeonCrawler/Setup Pool Manager In Boost")]
        public static void Setup()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Core"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "Core");

            var enemyConfig = FindReferencePool<EnemyPool>("Assets/Scenes/Lobby.unity")
                              ?? FindReferencePool<EnemyPool>(BoostScenePath);
            var projectileConfig = FindReferencePool<ProjectilePool>("Assets/Scenes/Lobby.unity")
                                   ?? FindReferencePool<ProjectilePool>(BoostScenePath);
            var dropConfig = FindReferencePool<DropPool>(BoostScenePath)
                             ?? FindReferencePool<DropPool>("Assets/Scenes/Lobby.unity");

            var boost = EditorSceneManager.OpenScene(BoostScenePath, OpenSceneMode.Single);
            DestroyIfExists("PoolManager");
            DestroyIfExists("DropPool");

            var root = new GameObject("PoolManager");
            var container = new GameObject("PoolContainer");
            container.transform.SetParent(root.transform, false);

            var enemy = root.AddComponent<EnemyPool>();
            var projectile = root.AddComponent<ProjectilePool>();
            var drop = root.AddComponent<DropPool>();
            root.AddComponent<SoundEffectsPool>();

            if (enemyConfig != null)
                EditorUtility.CopySerializedManagedFieldsOnly(enemyConfig, enemy);
            if (projectileConfig != null)
                EditorUtility.CopySerializedManagedFieldsOnly(projectileConfig, projectile);
            if (dropConfig != null)
                EditorUtility.CopySerializedManagedFieldsOnly(dropConfig, drop);

            SetPoolRoot(enemy, "_poolRoot", container.transform);
            SetPoolRoot(projectile, "_poolRoot", container.transform);
            SetPoolRoot(drop, "poolRoot", container.transform);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            EditorSceneManager.MarkSceneDirty(boost);
            EditorSceneManager.SaveScene(boost);

            RemovePoolsFromScene("Assets/Scenes/Lobby.unity");
            RemovePoolsFromScene("Assets/Scenes/BattleScene.unity");
            RemovePoolsFromScene("Assets/Scenes/Test.unity");

            AssetDatabase.SaveAssets();
            Debug.Log($"[PoolManagerSetup] Created {PrefabPath} and wired {BoostScenePath}.");
        }

        static T FindReferencePool<T>(string scenePath) where T : Component
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            var pool = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
            EditorSceneManager.CloseScene(scene, true);
            return pool;
        }

        static void SetPoolRoot(Object target, string propertyName, Transform root)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).objectReferenceValue = root;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void DestroyIfExists(string name)
        {
            var go = GameObject.Find(name);
            if (go != null)
                Object.DestroyImmediate(go);
        }

        static void RemovePoolsFromScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            DestroyIfExists("PoolManager");
            DestroyIfExists("PoolContainer");
            DestroyIfExists("DropPool");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
#endif
