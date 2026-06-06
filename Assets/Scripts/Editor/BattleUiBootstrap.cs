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
            var moveJoy = GameObject.Find("MoveJoystick");
            if (moveJoy == null)
            {
                Debug.LogError("[BattleUiBootstrap] MoveJoystick not found.");
                return;
            }

            EnsureSkillAimJoystick(hudRoot, moveJoy);
            RemoveLegacySkillButton(hudRoot);
            var dashBtn = EnsureButton(hudRoot, "DashButton", "Dash", new Vector2(-320f, 120f));

            var battleSo = new SerializedObject(battleUi);
            battleSo.FindProperty("dashButton").objectReferenceValue = dashBtn;
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

        private static GameObject EnsureSkillAimJoystick(Transform hudRoot, GameObject moveJoy)
        {
            var existing = hudRoot.Find("SkillAimJoystick");
            if (existing != null)
                return existing.gameObject;

            var skillJoy = Object.Instantiate(moveJoy, hudRoot);
            skillJoy.name = "SkillAimJoystick";

            var rt = skillJoy.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-270f, 99f);

            var joy = skillJoy.GetComponent<Joystick>();
            var so = new SerializedObject(joy);
            so.FindProperty("role").enumValueIndex = (int)Joystick.JoystickRole.SkillAim;
            so.ApplyModifiedPropertiesWithoutUndo();
            return skillJoy;
        }

        private static void RemoveLegacySkillButton(Transform hudRoot)
        {
            var legacy = hudRoot.Find("SkillButton");
            if (legacy != null)
                Object.DestroyImmediate(legacy.gameObject);
        }

        private static Button EnsureButton(Transform hudRoot, string name, string label, Vector2 anchoredPos)
        {
            var existing = hudRoot.Find(name);
            if (existing != null)
                return existing.GetComponent<Button>();

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
