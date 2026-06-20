#if UNITY_EDITOR
using Core;
using SO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditorTools
{
    public static class GameAudioBootstrap
    {
        const string CatalogPath = "Assets/SO/Audio/GameAudioCatalog.asset";
        const string BoostScenePath = "Assets/Scenes/Boost.unity";

        [MenuItem("DungeonCrawler/Setup Game Audio In Boost")]
        public static void Setup()
        {
            var scene = EditorSceneManager.OpenScene(BoostScenePath, OpenSceneMode.Single);
            var audioGo = GameObject.Find("AudioManager");
            if (audioGo == null)
            {
                Debug.LogError("[GameAudioBootstrap] AudioManager not found in Boost scene.");
                return;
            }

            var catalog = AssetDatabase.LoadAssetAtPath<GameAudioCatalogSO>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError($"[GameAudioBootstrap] Missing catalog at {CatalogPath}");
                return;
            }

            var controller = audioGo.GetComponent<GameAudioController>();
            if (controller == null)
                controller = audioGo.AddComponent<GameAudioController>();

            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("catalog").objectReferenceValue = catalog;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            var audio = audioGo.GetComponent<AudioManager>();
            if (audio != null)
                EnsureUiTag(audio);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[GameAudioBootstrap] Wired GameAudioController + catalog on AudioManager.");
        }

        static void EnsureUiTag(AudioManager audio)
        {
            var audioSo = new SerializedObject(audio);
            var tags = audioSo.FindProperty("customTagVolumes");
            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).FindPropertyRelative("tag").stringValue == GameAudio.TagUi)
                    return;
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            var elem = tags.GetArrayElementAtIndex(tags.arraySize - 1);
            elem.FindPropertyRelative("tag").stringValue = GameAudio.TagUi;
            elem.FindPropertyRelative("volume").floatValue = 1f;
            audioSo.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
