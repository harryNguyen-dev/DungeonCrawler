using Global;
using SO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUI.Lobby
{
    /// <summary>3D hero preview for loadout panel — RenderTexture + drag yaw.</summary>
    public class HeroPreviewController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public const string PreviewLayerName = "HeroPreview";

        public static HeroPreviewController Instance { get; private set; }

        [SerializeField] private Camera previewCamera;
        [SerializeField] private Transform modelPivot;
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private float dragDegreesPerPixel = 0.4f;
        [SerializeField] private Vector3 modelLocalPosition = new(0f, 0f, 0f);
        [SerializeField] private Vector3 isolatedWorldPosition = new(1000f, 1000f, 1000f);
        [SerializeField] private Color backgroundColor = new(0.05f, 0.08f, 0.14f, 1f);

        private GameObject previewModel;
        private RawImage boundPreviewImage;
        private bool isDragging;
        private int previewLayer = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            previewLayer = LayerMask.NameToLayer(PreviewLayerName);

            if (renderTexture == null)
                renderTexture = new RenderTexture(512, 512, 16);

            ConfigurePreviewRig();

            if (previewCamera != null)
            {
                previewCamera.targetTexture = renderTexture;
                previewCamera.enabled = false;
            }
        }

        private void ConfigurePreviewRig()
        {
            transform.position = isolatedWorldPosition;

            if (previewLayer < 0)
            {
                Debug.LogWarning($"[HeroPreviewController] Layer '{PreviewLayerName}' is missing. Preview may show lobby geometry.");
                return;
            }

            SetLayerRecursively(gameObject, previewLayer);

            if (previewCamera != null)
            {
                previewCamera.clearFlags = CameraClearFlags.SolidColor;
                previewCamera.backgroundColor = backgroundColor;
                previewCamera.cullingMask = 1 << previewLayer;
                previewCamera.useOcclusionCulling = false;
            }

            EnsurePreviewLight();
        }

        private void EnsurePreviewLight()
        {
            if (previewLayer < 0)
                return;

            var existing = GetComponentInChildren<Light>(true);
            if (existing != null)
            {
                existing.cullingMask = 1 << previewLayer;
                return;
            }

            var lightGo = new GameObject("PreviewLight");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localRotation = Quaternion.Euler(35f, -30f, 0f);

            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.cullingMask = 1 << previewLayer;
            SetLayerRecursively(lightGo, previewLayer);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            UnregisterPreview();
            ClearModel();
        }

        public RenderTexture GetRenderTexture() => renderTexture;

        public void ShowPreview()
        {
            if (previewCamera != null)
                previewCamera.enabled = true;

            var hero = GlobalEntities.Instance?.GetHero(Core.Save.HeroProgressService.GetEquippedHeroId());
            ShowHero(hero);
        }

        public void ShowHero(HeroSO hero)
        {
            if (previewCamera != null)
                previewCamera.enabled = true;

            EnsureModel(hero);
            ResetModelRotation();
        }

        public void HidePreview()
        {
            UnregisterPreview();

            if (previewCamera != null)
                previewCamera.enabled = false;

            ClearModel();
        }

        public void RegisterPreview(RawImage previewImage)
        {
            UnregisterPreview();
            boundPreviewImage = previewImage;

            if (boundPreviewImage != null && renderTexture != null)
                boundPreviewImage.texture = renderTexture;
        }

        public void UnregisterPreview()
        {
            boundPreviewImage = null;
            isDragging = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (previewModel == null || boundPreviewImage == null)
                return;

            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || previewModel == null)
                return;

            var deltaX = eventData.delta.x;
            if (Mathf.Abs(deltaX) < 0.01f)
                return;

            previewModel.transform.Rotate(Vector3.up, -deltaX * dragDegreesPerPixel, Space.World);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }

        private void ResetModelRotation()
        {
            if (previewModel != null)
                previewModel.transform.localRotation = Quaternion.identity;
        }

        private void EnsureModel(HeroSO hero)
        {
            ClearModel();

            if (modelPivot == null)
                return;

            GameObject prefab = hero?.visualPrefab;
            if (prefab == null)
                prefab = GlobalEntities.Instance?.PlayerPrefab;

            if (prefab == null)
                return;

            previewModel = Instantiate(prefab, modelPivot);
            previewModel.transform.localPosition = hero != null ? hero.visualLocalPosition : modelLocalPosition;
            previewModel.transform.localRotation = Quaternion.identity;
            previewModel.transform.localScale = hero != null ? hero.visualLocalScale : Vector3.one;

            if (previewLayer >= 0)
                SetLayerRecursively(previewModel, previewLayer);

            DisableGameplayComponents(previewModel);
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            if (root == null)
                return;

            root.layer = layer;
            foreach (Transform child in root.transform)
                SetLayerRecursively(child.gameObject, layer);
        }

        private static void DisableGameplayComponents(GameObject model)
        {
            foreach (var rotate in model.GetComponentsInChildren<PlayerController.Rotate>(true))
                rotate.enabled = false;

            foreach (var movement in model.GetComponentsInChildren<PlayerController.Movement>(true))
                movement.enabled = false;

            foreach (var attack in model.GetComponentsInChildren<PlayerController.Attack>(true))
                attack.enabled = false;

            var rb = model.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            var cc = model.GetComponent<CharacterController>();
            if (cc != null)
                cc.enabled = false;
        }

        private void ClearModel()
        {
            if (previewModel != null)
                Destroy(previewModel);
            previewModel = null;
        }
    }
}
