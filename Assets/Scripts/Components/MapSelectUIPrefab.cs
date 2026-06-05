using System;
using Core;
using SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class MapSelectUIPrefab : MonoBehaviour
    {
        [SerializeField] private TMP_Text mapIndexText;
        [SerializeField] private TMP_Text mapNameText;
        [SerializeField] private Button playBtn;

        [Header("Stars")]
        [SerializeField] private Image star1;
        [SerializeField] private Image star2;
        [SerializeField] private Image star3;

        public void Bind(LevelSO level, int index, bool isUnlocked, int bestStars, Action onPlay)
        {
            if (mapIndexText != null)
                mapIndexText.text = $"Map {index + 1}";

            if (mapNameText != null)
                mapNameText.text = level != null ? level.DisplayLabel : $"Stage {index + 1}";

            StarDisplayHelper.Apply(star1, star2, star3, bestStars);

            if (playBtn == null)
                return;

            playBtn.transform.SetAsLastSibling();
            playBtn.onClick.RemoveAllListeners();
            playBtn.transition = Selectable.Transition.None;
            playBtn.interactable = isUnlocked;

            if (playBtn.targetGraphic is Image playImage)
            {
                playImage.color = isUnlocked ? ColorUtils.Yellow : ColorUtils.Gray;
                playImage.raycastTarget = isUnlocked;
            }

            if (playBtn.TryGetComponent<Collider>(out var collider))
                collider.enabled = isUnlocked;

            if (isUnlocked && onPlay != null)
                playBtn.onClick.AddListener(() => onPlay());
        }
    }
}
