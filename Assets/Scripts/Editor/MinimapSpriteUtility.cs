#if UNITY_EDITOR
using UnityEngine;
using WFC;

namespace EditorTools
{
    public static class MinimapSpriteUtility
    {
        public const int DefaultCellPixelSize = 64;

        public static Texture2D GenerateTexture(WFCData data, int size = DefaultCellPixelSize)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(0, 0, 0, 0);

            if (data == null || data.tileType == TileType.Empty)
            {
                tex.SetPixels32(pixels);
                tex.Apply();
                return tex;
            }

            int corridorWidth = Mathf.Max(2, size / 3);
            int roomSize = Mathf.RoundToInt(size * 0.7f);
            int center = size / 2;
            int halfCorridor = corridorWidth / 2;
            int halfRoom = roomSize / 2;

            var corridorColor = new Color32(80, 200, 80, 255);
            var roomColor = new Color32(77, 179, 255, 255);

            FillRect(pixels, size, center - halfCorridor, center - halfCorridor, corridorWidth, corridorWidth, corridorColor);

            if (data.north == ConnectorType.Open)
                FillRect(pixels, size, center - halfCorridor, center, corridorWidth, halfCorridor + 1, corridorColor);
            if (data.south == ConnectorType.Open)
                FillRect(pixels, size, center - halfCorridor, 0, corridorWidth, center - halfCorridor + 1, corridorColor);
            if (data.east == ConnectorType.Open)
                FillRect(pixels, size, center, center - halfCorridor, halfCorridor + 1, corridorWidth, corridorColor);
            if (data.west == ConnectorType.Open)
                FillRect(pixels, size, 0, center - halfCorridor, center - halfCorridor + 1, corridorWidth, corridorColor);

            if (data.tileType == TileType.Room)
            {
                FillRect(
                    pixels,
                    size,
                    center - halfRoom,
                    center - halfRoom,
                    roomSize,
                    roomSize,
                    roomColor);
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        private static void FillRect(Color32[] pixels, int size, int x, int y, int width, int height, Color32 color)
        {
            int maxX = Mathf.Min(size, x + width);
            int maxY = Mathf.Min(size, y + height);
            x = Mathf.Max(0, x);
            y = Mathf.Max(0, y);

            for (int py = y; py < maxY; py++)
            {
                for (int px = x; px < maxX; px++)
                    pixels[py * size + px] = color;
            }
        }
    }
}
#endif
