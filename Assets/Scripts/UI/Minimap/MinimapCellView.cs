using Core.Minimap;
using UnityEngine;
using UnityEngine.UI;
using WFC;

namespace CustomUI.Minimap
{
    [RequireComponent(typeof(Image))]
    public class MinimapCellView : MonoBehaviour
    {
        private static Sprite _solidSprite;

        [SerializeField] private Image image;

        private RectTransform _rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = transform as RectTransform;
                return _rectTransform;
            }
        }

        private void Awake()
        {
            if (image == null)
                image = GetComponent<Image>();

            EnsureSolidSprite();
        }

        public void SetGridPosition(int viewX, int viewY, float cellPixelSize)
        {
            RectTransform rect = RectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(cellPixelSize, cellPixelSize);
            rect.anchoredPosition = new Vector2(
                (viewX + 0.5f) * cellPixelSize,
                (viewY + 0.5f) * cellPixelSize);
        }

        public void ApplyState(
            MinimapCellState state,
            WFCData tileData,
            Color hiddenColor,
            Color visitedColor)
        {
            if (image == null)
                return;

            EnsureSolidSprite();

            if (state == MinimapCellState.Hidden
                || tileData == null
                || tileData.tileType == TileType.Empty)
            {
                ShowFog(hiddenColor);
                return;
            }

            image.enabled = true;
            image.sprite = tileData.minimapSprite != null ? tileData.minimapSprite : _solidSprite;
            image.preserveAspect = false;
            image.type = Image.Type.Simple;
            image.color = visitedColor;
        }

        private void ShowFog(Color hiddenColor)
        {
            image.enabled = true;
            image.sprite = _solidSprite;
            image.preserveAspect = false;
            image.type = Image.Type.Simple;
            image.color = hiddenColor;
        }

        private static void EnsureSolidSprite()
        {
            if (_solidSprite != null)
                return;

            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();

            _solidSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
