#if UNITY_EDITOR
using Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EditorTools
{
    public static class DungeonEnvironmentBootstrap
    {
        const string BattleScenePath = "Assets/Scenes/BattleScene.unity";
        const string SkyboxPath = "Assets/Materials/DungeonInteriorSkybox.mat";
        const string VolumeProfilePath = "Assets/Settings/DungeonVolumeProfile.asset";

        [MenuItem("DungeonCrawler/Setup Dungeon Environment Lighting")]
        public static void Setup()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "BattleScene")
                scene = EditorSceneManager.OpenScene(BattleScenePath);

            var skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxPath);
            var volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (skybox == null || volumeProfile == null)
            {
                Debug.LogError("[DungeonEnvironmentBootstrap] Missing skybox or volume profile assets.");
                return;
            }

            ApplyRenderSettings(skybox);
            EnsureEnvironmentLighting(skybox);
            EnsureGlobalVolume(volumeProfile);
            ConfigureDirectionalLight();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[DungeonEnvironmentBootstrap] Dungeon environment lighting applied to BattleScene.");
        }

        static void ApplyRenderSettings(Material skybox)
        {
            RenderSettings.skybox = skybox;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.15f, 0.16f, 0.20f);
            RenderSettings.ambientEquatorColor = new Color(0.18f, 0.19f, 0.24f);
            RenderSettings.ambientGroundColor = new Color(0.10f, 0.10f, 0.12f);
            RenderSettings.ambientIntensity = 2.2f;
            RenderSettings.subtractiveShadowColor = new Color(0.28f, 0.32f, 0.38f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.06f, 0.07f, 0.10f);
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.reflectionIntensity = 0.6f;
        }

        static void EnsureEnvironmentLighting(Material skybox)
        {
            var root = GameObject.Find("DungeonEnvironment");
            if (root == null)
                root = new GameObject("DungeonEnvironment");

            var lighting = root.GetComponent<DungeonEnvironmentLighting>();
            if (lighting == null)
                lighting = root.AddComponent<DungeonEnvironmentLighting>();

            var so = new SerializedObject(lighting);
            so.FindProperty("skyboxMaterial").objectReferenceValue = skybox;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureGlobalVolume(VolumeProfile profile)
        {
            var volumeGo = GameObject.Find("Global Volume");
            if (volumeGo == null)
                volumeGo = new GameObject("Global Volume");

            var volume = volumeGo.GetComponent<Volume>();
            if (volume == null)
                volume = volumeGo.AddComponent<Volume>();

            volume.isGlobal = true;
            volume.priority = 0;
            volume.weight = 1f;
            volume.sharedProfile = profile;
        }

        static void ConfigureDirectionalLight()
        {
            Light mainLight = null;
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var light in lights)
            {
                if (light.type != LightType.Directional || !light.enabled)
                    continue;

                mainLight = light;
                break;
            }

            if (mainLight == null)
            {
                var lightGo = new GameObject("Directional Light");
                mainLight = lightGo.AddComponent<Light>();
                mainLight.type = LightType.Directional;
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            mainLight.intensity = 0.4f;
            mainLight.useColorTemperature = true;
            mainLight.colorTemperature = 4200f;
            mainLight.color = new Color(0.75f, 0.88f, 1f);
            mainLight.shadows = LightShadows.Soft;

            var urpData = mainLight.GetComponent<UniversalAdditionalLightData>();
            if (urpData == null)
                urpData = mainLight.gameObject.AddComponent<UniversalAdditionalLightData>();
            urpData.softShadowQuality = SoftShadowQuality.Medium;
        }
    }
}
#endif
