#if UNITY_EDITOR
using Core.Minimap;
using CustomUI.Minimap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTools
{
    public static class MinimapUiBootstrap
    {
        private const string CellPrefabPath = "Assets/Prefabs/UI/MinimapCell.prefab";

        [MenuItem("DungeonCrawler/Bootstrap Minimap UI")]
        public static void Bootstrap()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "BattleScene")
                scene = EditorSceneManager.OpenScene("Assets/Scenes/BattleScene.unity");

            EnsureCellPrefab();
            MinimapCellView cellPrefab = AssetDatabase.LoadAssetAtPath<MinimapCellView>(CellPrefabPath);

            var minimapRoot = GameObject.Find("MiniMap");
            if (minimapRoot == null)
            {
                Debug.LogError("[MinimapUiBootstrap] MiniMap GameObject not found.");
                return;
            }

            RemoveLegacyComponents(minimapRoot);

            var service = Object.FindFirstObjectByType<MinimapService>();
            if (service == null)
            {
                var serviceGo = new GameObject("MinimapService");
                service = serviceGo.AddComponent<MinimapService>();
            }

            var canvasGroup = minimapRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = minimapRoot.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            var viewportGo = GetOrCreateChild(minimapRoot.transform, "Viewport");
            var viewportRect = viewportGo.GetComponent<RectTransform>();
            StretchFull(viewportRect);
            viewportRect.pivot = Vector2.zero;
            if (viewportGo.GetComponent<RectMask2D>() == null)
                viewportGo.AddComponent<RectMask2D>();

            var cellContainerGo = GetOrCreateChild(viewportGo.transform, "CellContainer");
            var cellContainerRect = cellContainerGo.GetComponent<RectTransform>();
            StretchFull(cellContainerRect);
            cellContainerRect.pivot = Vector2.zero;

            var dotGo = GetOrCreateChild(viewportGo.transform, "PlayerDot");
            var dotImage = dotGo.GetComponent<Image>();
            if (dotImage == null)
                dotImage = dotGo.AddComponent<Image>();
            var dotRect = dotGo.GetComponent<RectTransform>();
            dotRect.anchorMin = Vector2.zero;
            dotRect.anchorMax = Vector2.zero;
            dotRect.pivot = new Vector2(0.5f, 0.5f);
            dotImage.raycastTarget = false;
            dotImage.color = Color.white;
            dotRect.sizeDelta = new Vector2(10f, 10f);
            dotRect.anchoredPosition = Vector2.zero;
            if (dotImage.sprite == null)
            {
                dotImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                dotImage.type = Image.Type.Simple;
            }

            var minimapView = minimapRoot.GetComponent<MinimapView>();
            if (minimapView == null)
                minimapView = minimapRoot.AddComponent<MinimapView>();

            var so = new SerializedObject(minimapView);
            so.FindProperty("viewport").objectReferenceValue = viewportRect;
            so.FindProperty("cellContainer").objectReferenceValue = cellContainerRect;
            so.FindProperty("playerDot").objectReferenceValue = dotRect;
            so.FindProperty("cellPrefab").objectReferenceValue = cellPrefab;
            so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            so.ApplyModifiedPropertiesWithoutUndo();

            minimapRoot.SetActive(true);
            dotGo.transform.SetAsLastSibling();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MinimapUiBootstrap] Production minimap wired.");
        }

        private static void RemoveLegacyComponents(GameObject minimapRoot)
        {
            foreach (var behaviour in minimapRoot.GetComponents<MonoBehaviour>())
            {
                if (behaviour != null && behaviour.GetType().Name == "MinimapUI")
                    Object.DestroyImmediate(behaviour);
            }

            var mapImage = minimapRoot.transform.Find("MapImage");
            if (mapImage != null)
                Object.DestroyImmediate(mapImage.gameObject);

            var oldDot = minimapRoot.transform.Find("PlayerDot");
            if (oldDot != null && oldDot.parent == minimapRoot.transform)
                Object.DestroyImmediate(oldDot.gameObject);
        }

        private static void EnsureCellPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<MinimapCellView>(CellPrefabPath);
            if (existing != null)
                return;

            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/UI");

            var go = new GameObject("MinimapCell", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(MinimapCellView));
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            image.preserveAspect = true;
            image.color = Color.white;

            PrefabUtility.SaveAsPrefabAsset(go, CellPrefabPath);
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            string folder = path.Substring(slash + 1);
            AssetDatabase.CreateFolder(parent, folder);
        }

        private static GameObject GetOrCreateChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
                return child.gameObject;

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void SetupBottomLeftContainer(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(240f, 240f);
        }
    }
}
#endif
