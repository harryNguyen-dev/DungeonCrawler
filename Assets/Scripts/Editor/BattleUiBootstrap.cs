#if UNITY_EDITOR
using Core;
using CustomUI;
using Global;
using SO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTools
{
    public static class BattleUiBootstrap
    {
        [MenuItem("DungeonCrawler/Bootstrap Battle UI")]
        public static void Bootstrap()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/BattleScene.unity");
            var battleUi = Object.FindFirstObjectByType<BattleUI>();
            if (battleUi == null)
            {
                Debug.LogError("[BattleUiBootstrap] BattleUI not found.");
                return;
            }

            var hudRoot = battleUi.transform;
            if (Object.FindFirstObjectByType<Joystick>() == null)
            {
                Debug.LogError("[BattleUiBootstrap] Joystick not found in scene.");
                return;
            }

            RemoveSkillAimJoystick(hudRoot);
            var dashBtn = EnsureButton(hudRoot, "DashButton", "Dash", new Vector2(-320f, 120f));
            var skillBtn = EnsureButton(hudRoot, "SkillButton", "Skill", new Vector2(-320f, 220f));

            var battleSo = new SerializedObject(battleUi);
            battleSo.FindProperty("dashButton").objectReferenceValue = dashBtn;
            battleSo.FindProperty("skillButton").objectReferenceValue = skillBtn;
            battleSo.ApplyModifiedPropertiesWithoutUndo();

            WireGlobalEntitiesInOpenScenes();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[BattleUiBootstrap] Battle UI wired.");
        }

        [MenuItem("DungeonCrawler/Wire Hero Catalog On GlobalEntities")]
        public static void WireGlobalEntitiesInOpenScenes()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<HeroCatalogSO>("Assets/SO/Hero/HeroCatalog_Global.asset");
            var dash = AssetDatabase.LoadAssetAtPath<DashConfigSO>("Assets/SO/Hero/DashConfig_Default.asset");
            var scenes = new[]
            {
                "Assets/Scenes/Boost.unity",
                "Assets/Scenes/Lobby.unity",
                "Assets/Scenes/BattleScene.unity"
            };

            var activeScenePath = EditorSceneManager.GetActiveScene().path;

            foreach (var scenePath in scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath);
                foreach (var ge in Object.FindObjectsByType<GlobalEntities>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    var geSo = new SerializedObject(ge);
                    geSo.FindProperty("HeroCatalog").objectReferenceValue = catalog;
                    geSo.FindProperty("DefaultDashConfig").objectReferenceValue = dash;
                    geSo.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(ge);
                }

                EditorSceneManager.SaveScene(scene);
            }

            if (!string.IsNullOrEmpty(activeScenePath))
                EditorSceneManager.OpenScene(activeScenePath);
        }

        private static void RemoveSkillAimJoystick(Transform hudRoot)
        {
            var existing = hudRoot.Find("SkillAimJoystick");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);
        }

        private static Button EnsureButton(Transform hudRoot, string name, string label, Vector2 anchoredPos)
        {
            var existing = hudRoot.Find(name);
            if (existing != null)
            {
                var existingTxt = existing.GetComponentInChildren<TMP_Text>();
                if (existingTxt != null)
                    existingTxt.text = label;
                return existing.GetComponent<Button>();
            }

            var attackTransform = hudRoot.Find("NormalAttack");
            if (attackTransform == null)
                return null;

            var go = Object.Instantiate(attackTransform.gameObject, hudRoot);
            go.name = name;

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;

            var txt = go.GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = label;

            return go.GetComponent<Button>();
        }
    }
}
#endif
