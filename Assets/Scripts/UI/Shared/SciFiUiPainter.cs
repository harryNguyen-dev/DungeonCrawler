using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUI.SciFi
{
    public enum SciFiButtonVariant
    {
        Default,
        Primary,
        Exit
    }

    public static class SciFiUiPainter
    {
        public static readonly Color LobbyBackground = new(13f / 255f, 23f / 255f, 33f / 255f);

        private static readonly Color FrameOuter = new(18f / 255f, 52f / 255f, 92f / 255f);
        private static readonly Color FrameMid = new(42f / 255f, 118f / 255f, 172f / 255f);
        private static readonly Color FillTop = new(22f / 255f, 40f / 255f, 72f / 255f, 0.94f);
        private static readonly Color FillBottom = new(8f / 255f, 16f / 255f, 32f / 255f, 0.98f);
        private static readonly Color Gloss = new(120f / 255f, 190f / 255f, 255f / 255f, 0.14f);

        private static readonly Color AccentCyan = new(0f, 210f / 255f, 255f / 255f);
        private static readonly Color AccentYellow = new(255f / 255f, 190f / 255f, 70f / 255f);
        private static readonly Color AccentMagenta = new(255f / 255f, 70f / 255f, 120f / 255f);

        public static void DrawHexButton(Painter2D painter, Rect rect, SciFiButtonVariant variant, bool hovered)
        {
            if (rect.width <= 1f || rect.height <= 1f)
                return;

            var accent = variant switch
            {
                SciFiButtonVariant.Primary => AccentYellow,
                SciFiButtonVariant.Exit => AccentMagenta,
                _ => AccentCyan
            };

            var glowAlpha = hovered ? 0.42f : 0.24f;
            DrawHexGlow(painter, rect, accent, glowAlpha);

            DrawHexFill(painter, rect, hovered);

            painter.strokeColor = FrameOuter;
            painter.lineWidth = 3f;
            painter.BeginPath();
            BuildHexPath(painter, rect, 0f);
            painter.Stroke();

            painter.strokeColor = Color.Lerp(FrameMid, accent, hovered ? 0.55f : 0.35f);
            painter.lineWidth = 1.5f;
            painter.BeginPath();
            BuildHexPath(painter, rect, 2f);
            painter.Stroke();

            DrawTopGloss(painter, rect);
        }

        public static void DrawPanelFrame(Painter2D painter, Rect rect)
        {
            if (rect.width <= 1f || rect.height <= 1f)
                return;

            const float inset = 1f;
            var inner = new Rect(rect.x + inset, rect.y + inset, rect.width - inset * 2f, rect.height - inset * 2f);

            painter.fillColor = new Color(8f / 255f, 14f / 255f, 28f / 255f, 0.98f);
            painter.BeginPath();
            painter.MoveTo(new Vector2(inner.xMin, inner.yMin));
            painter.LineTo(new Vector2(inner.xMax, inner.yMin));
            painter.LineTo(new Vector2(inner.xMax, inner.yMax));
            painter.LineTo(new Vector2(inner.xMin, inner.yMax));
            painter.ClosePath();
            painter.Fill();

            const int bands = 10;
            for (var i = 0; i < bands; i++)
            {
                var t = (i + 1) / (float)bands;
                var y0 = inner.yMin + inner.height * (i / (float)bands);
                var y1 = inner.yMin + inner.height * t;
                painter.fillColor = Color.Lerp(FillTop, FillBottom, t);
                painter.BeginPath();
                painter.MoveTo(new Vector2(inner.xMin, y0));
                painter.LineTo(new Vector2(inner.xMax, y0));
                painter.LineTo(new Vector2(inner.xMax, y1));
                painter.LineTo(new Vector2(inner.xMin, y1));
                painter.ClosePath();
                painter.Fill();
            }

            painter.strokeColor = FrameOuter;
            painter.lineWidth = 3f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMax));
            painter.LineTo(new Vector2(rect.xMin, rect.yMax));
            painter.ClosePath();
            painter.Stroke();

            painter.strokeColor = FrameMid;
            painter.lineWidth = 1f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(inner.xMin, inner.yMin));
            painter.LineTo(new Vector2(inner.xMax, inner.yMin));
            painter.LineTo(new Vector2(inner.xMax, inner.yMax));
            painter.LineTo(new Vector2(inner.xMin, inner.yMax));
            painter.ClosePath();
            painter.Stroke();

            DrawCornerAccent(painter, new Vector2(rect.xMin, rect.yMin), 18f, true, true);
            DrawCornerAccent(painter, new Vector2(rect.xMax, rect.yMin), 18f, false, true);
            DrawCornerAccent(painter, new Vector2(rect.xMin, rect.yMax), 18f, true, false);
            DrawCornerAccent(painter, new Vector2(rect.xMax, rect.yMax), 18f, false, false);

            painter.strokeColor = new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.35f);
            painter.lineWidth = 1f;
            var margin = 8f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(inner.xMin + margin, inner.yMin + margin));
            painter.LineTo(new Vector2(inner.xMax - margin, inner.yMin + margin));
            painter.LineTo(new Vector2(inner.xMax - margin, inner.yMax - margin));
            painter.LineTo(new Vector2(inner.xMin + margin, inner.yMax - margin));
            painter.ClosePath();
            painter.Stroke();
        }

        private static void DrawHexGlow(Painter2D painter, Rect rect, Color accent, float alpha)
        {
            painter.fillColor = new Color(accent.r, accent.g, accent.b, alpha);
            painter.BeginPath();
            BuildHexPath(painter, rect, -3f);
            painter.Fill();
        }

        private static void DrawHexFill(Painter2D painter, Rect rect, bool hovered)
        {
            var fillBottom = hovered
                ? Color.Lerp(FillBottom, new Color(0.06f, 0.14f, 0.28f, 0.98f), 0.25f)
                : FillBottom;

            painter.fillColor = fillBottom;
            painter.BeginPath();
            BuildHexPath(painter, rect, 3f);
            painter.Fill();
        }

        private static void DrawTopGloss(Painter2D painter, Rect rect)
        {
            var chamfer = Mathf.Min(rect.height * 0.38f, rect.width * 0.12f);
            var top = rect.yMin + 3f;
            var midY = rect.yMin + rect.height * 0.42f;

            painter.fillColor = Gloss;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin + chamfer + 3f, top));
            painter.LineTo(new Vector2(rect.xMax - chamfer - 3f, top));
            painter.LineTo(new Vector2(rect.xMax - chamfer * 0.5f - 3f, midY));
            painter.LineTo(new Vector2(rect.xMin + chamfer * 0.5f + 3f, midY));
            painter.ClosePath();
            painter.Fill();
        }

        private static void DrawCornerAccent(Painter2D painter, Vector2 corner, float size, bool left, bool top)
        {
            var sx = left ? 1f : -1f;
            var sy = top ? 1f : -1f;
            painter.strokeColor = new Color(AccentYellow.r, AccentYellow.g, AccentYellow.b, 0.85f);
            painter.lineWidth = 2.5f;
            painter.lineCap = LineCap.Round;
            painter.BeginPath();
            painter.MoveTo(corner);
            painter.LineTo(corner + new Vector2(sx * size, 0f));
            painter.Stroke();
            painter.BeginPath();
            painter.MoveTo(corner);
            painter.LineTo(corner + new Vector2(0f, sy * size));
            painter.Stroke();
        }

        private static void BuildHexPath(Painter2D painter, Rect rect, float expand)
        {
            var r = new Rect(rect.x - expand, rect.y - expand, rect.width + expand * 2f, rect.height + expand * 2f);
            var chamfer = Mathf.Min(r.height * 0.38f, r.width * 0.12f);
            var midY = r.yMin + r.height * 0.5f;

            painter.MoveTo(new Vector2(r.xMin, midY));
            painter.LineTo(new Vector2(r.xMin + chamfer, r.yMin));
            painter.LineTo(new Vector2(r.xMax - chamfer, r.yMin));
            painter.LineTo(new Vector2(r.xMax, midY));
            painter.LineTo(new Vector2(r.xMax - chamfer, r.yMax));
            painter.LineTo(new Vector2(r.xMin + chamfer, r.yMax));
            painter.ClosePath();
        }
    }
}
