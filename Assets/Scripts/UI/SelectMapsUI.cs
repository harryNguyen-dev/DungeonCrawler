using System.Collections;
using System.Collections.Generic;
using Components;
using Core;
using Core.Save;
using DG.Tweening;
using Global;
using SO;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class SelectMapsUI : MonoBehaviour
    {
        private const float OpenStartPosX = 1000f;

        [SerializeField] private GameObject container;
        [SerializeField] private GameObject mapSelectUIPrefab;
        [SerializeField] private Transform scrollViewContent;
        [SerializeField] private Button closeBtn;
        [SerializeField] private float openDuration = 0.35f;

        private readonly List<GameObject> spawnedRows = new();
        private RectTransform containerRect;
        private ScrollRect scrollRect;
        private RectTransform contentRect;
        private Tween openTween;

        private void Awake()
        {
            if (container != null)
            {
                containerRect = container.GetComponent<RectTransform>();
                scrollRect = container.GetComponent<ScrollRect>();
            }

            if (scrollViewContent is RectTransform content)
                contentRect = content;

            closeBtn?.onClick.AddListener(OnCloseClicked);
        }

        private void OnDestroy()
        {
            openTween?.Kill();
            closeBtn?.onClick.RemoveListener(OnCloseClicked);
        }

        private void OnEnable()
        {
            GlobalEvents.OnRequestLevelSelectUI += Open;
            GlobalEvents.OnLobbyReady += HandleLobbyReady;
        }

        private void OnDisable()
        {
            GlobalEvents.OnRequestLevelSelectUI -= Open;
            GlobalEvents.OnLobbyReady -= HandleLobbyReady;
        }

        private void HandleLobbyReady()
        {
            if (container != null && container.activeSelf)
                PopulateMaps();

            if (!GlobalVariable.OpenLevelSelectOnLobbyReturn)
                return;

            GlobalVariable.OpenLevelSelectOnLobbyReturn = false;
            Open();
        }

        public void Open()
        {
            GameAudio.PlayUiConfirm();

            if (container == null)
                return;

            this.gameObject.SetActive(true);
            container.SetActive(true);
            PopulateMaps();
            PlayOpenTween();
        }

        private void OnCloseClicked()
        {
            GameAudio.PlayUiBack();
            Close();
        }

        public void Close()
        {
            openTween?.Kill();

            if (container != null)
                container.SetActive(false);

            this.gameObject.SetActive(false);
        }

        private void PlayOpenTween()
        {
            if (containerRect == null)
                return;

            openTween?.Kill();

            var pos = containerRect.anchoredPosition;
            containerRect.anchoredPosition = new Vector2(OpenStartPosX, pos.y);
            openTween = containerRect
                .DOAnchorPosX(0f, openDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }

        private void PopulateMaps()
        {
            ClearRows();

            var catalog = GlobalEntities.Instance?.GetChapter1Catalog();
            if (catalog == null || scrollViewContent == null || mapSelectUIPrefab == null)
            {
                Debug.LogWarning("[SelectMapsUI] Missing catalog, scroll content, or row prefab.");
                return;
            }

            for (var i = 0; i < catalog.LevelCount; i++)
            {
                var level = catalog.GetLevel(i);
                if (level == null)
                    continue;

                var rowObject = Instantiate(mapSelectUIPrefab, scrollViewContent);
                spawnedRows.Add(rowObject);

                if (!rowObject.TryGetComponent(out MapSelectUIPrefab row))
                    continue;

                var mapIndex = i;
                var mapLevel = level;
                var isUnlocked = LevelProgressService.IsUnlocked(mapIndex, catalog.LevelCount);
                var bestStars = LevelProgressService.GetBestStars(mapLevel.levelId);
                row.Bind(mapLevel, mapIndex, isUnlocked, bestStars, () => OnLevelSelected(mapLevel, mapIndex));
            }

            RefreshScrollContent();
        }

        private void RefreshScrollContent()
        {
            if (contentRect == null)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
            {
                scrollRect.horizontalNormalizedPosition = 0f;
                scrollRect.velocity = Vector2.zero;
            }

            StartCoroutine(RefreshScrollContentEndOfFrame());
        }

        private IEnumerator RefreshScrollContentEndOfFrame()
        {
            yield return null;

            if (contentRect == null)
                yield break;

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            Canvas.ForceUpdateCanvases();
        }

        private void OnLevelSelected(LevelSO level, int index)
        {
            if (level == null)
            {
                Debug.LogWarning("[SelectMapsUI] Level data is missing.");
                return;
            }

            var catalog = GlobalEntities.Instance?.GetChapter1Catalog();
            var levelCount = catalog?.LevelCount ?? 0;

            if (!LevelProgressService.IsUnlocked(index, levelCount))
            {
                Debug.LogWarning($"[SelectMapsUI] Map {index + 1} is locked (unlocked up to index {LevelProgressService.GetHighestUnlockedIndex(levelCount)}).");
                return;
            }

            GlobalVariable.CurrentLevel = level;
            GlobalVariable.CurrentLevelIndex = index;
            Close();
            SceneManagerCustom.LoadDungeon();
        }

        private void ClearRows()
        {
            foreach (var row in spawnedRows)
            {
                if (row != null)
                    Destroy(row);
            }

            spawnedRows.Clear();
        }
    }
}
