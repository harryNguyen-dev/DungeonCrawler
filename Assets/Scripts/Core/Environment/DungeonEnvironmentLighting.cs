using UnityEngine;
using UnityEngine.Rendering;

namespace Core
{
    [DisallowMultipleComponent]
    public sealed class DungeonEnvironmentLighting : MonoBehaviour
    {
        [Header("Sky & Ambient")]
        [SerializeField] Material skyboxMaterial;
        [SerializeField] Color ambientSky = new(0.20f, 0.21f, 0.26f);
        [SerializeField] Color ambientEquator = new(0.24f, 0.25f, 0.30f);
        [SerializeField] Color ambientGround = new(0.14f, 0.14f, 0.17f);
        [SerializeField] float ambientIntensity = 2.7f;

        [Header("Fog")]
        [SerializeField] bool enableFog = true;
        [SerializeField] Color fogColor = new(0.09f, 0.10f, 0.14f);
        [SerializeField] float fogDensity = 0.009f;

        [Header("Reflection")]
        [SerializeField] float reflectionIntensity = 0.7f;

        [Header("Key Light")]
        [SerializeField] Light mainLight;
        [SerializeField] float mainLightIntensity = 0.55f;
        [SerializeField] float mainLightTemperature = 4200f;
        [SerializeField] Color mainLightColorFilter = new(0.75f, 0.88f, 1f);

        void Awake()
        {
            Apply();
        }

        public void Apply()
        {
            if (skyboxMaterial != null)
                RenderSettings.skybox = skyboxMaterial;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ambientSky;
            RenderSettings.ambientEquatorColor = ambientEquator;
            RenderSettings.ambientGroundColor = ambientGround;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.subtractiveShadowColor = new Color(0.38f, 0.42f, 0.48f);

            RenderSettings.fog = enableFog;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;

            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.reflectionIntensity = reflectionIntensity;

            if (mainLight == null)
                mainLight = FindMainDirectionalLight();

            if (mainLight != null)
            {
                mainLight.intensity = mainLightIntensity;
                mainLight.useColorTemperature = true;
                mainLight.colorTemperature = mainLightTemperature;
                mainLight.color = mainLightColorFilter;
                mainLight.shadows = LightShadows.Soft;
            }

            DynamicGI.UpdateEnvironment();
        }

        static Light FindMainDirectionalLight()
        {
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional && light.enabled)
                    return light;
            }

            return null;
        }
    }
}
