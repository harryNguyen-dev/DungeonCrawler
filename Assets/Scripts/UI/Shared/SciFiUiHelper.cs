using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUI.SciFi
{
    public static class SciFiUiHelper
    {
        private const string StyledFlag = "scifi-styled";

        private static readonly Color ButtonTextColor = new(0.96f, 0.97f, 1f);

        public static void StyleSciFiDocument(VisualElement root)
        {
            StyleSciFiPanels(root);
            StyleSciFiButtons(root);
        }

        public static void StyleSciFiButtons(VisualElement root, string className = "scifi-button")
        {
            if (root == null)
                return;

            root.Query<Button>(className: className).ForEach(AttachButtonChrome);
        }

        public static void StyleSciFiPanels(VisualElement root, string className = "scifi-panel")
        {
            if (root == null)
                return;

            root.Query<VisualElement>(className: className).ForEach(AttachPanelChrome);
        }

        public static void StyleLobbyUi(VisualElement root)
        {
            StyleSciFiButtons(root, "menu-button");
            StyleSciFiPanels(root, "resource-chip");
        }

        private static void AttachButtonChrome(Button button)
        {
            if (button.ClassListContains(StyledFlag) || button.ClassListContains("scifi-button-compact"))
                return;

            button.AddToClassList(StyledFlag);
            ClearButtonChrome(button);

            var variant = ResolveVariant(button);
            var hovered = false;

            button.RegisterCallback<MouseEnterEvent>(_ =>
            {
                hovered = true;
                button.MarkDirtyRepaint();
            });
            button.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                hovered = false;
                button.MarkDirtyRepaint();
            });
            button.generateVisualContent += context =>
                SciFiUiPainter.DrawHexButton(context.painter2D, button.contentRect, variant, hovered);

            ApplyButtonTextStyle(button);
            button.RegisterCallback<GeometryChangedEvent>(_ => ApplyHexButtonPadding(button));
        }

        private static void AttachPanelChrome(VisualElement panel)
        {
            if (panel.ClassListContains(StyledFlag))
                return;

            panel.AddToClassList(StyledFlag);
            panel.generateVisualContent += context =>
                SciFiUiPainter.DrawPanelFrame(context.painter2D, panel.contentRect);
        }

        private static void ClearButtonChrome(Button button)
        {
            button.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
        }

        private static void ApplyButtonTextStyle(Button button)
        {
            button.style.color = ButtonTextColor;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.style.textOverflow = TextOverflow.Ellipsis;
            button.style.overflow = Overflow.Hidden;
            ApplyHexButtonPadding(button);
        }

        private static void ApplyHexButtonPadding(Button button)
        {
            var height = button.resolvedStyle.height;
            if (height <= 1f)
                height = 48f;

            var horizontalPad = Mathf.Max(36f, height * 0.34f);
            button.style.paddingLeft = horizontalPad;
            button.style.paddingRight = horizontalPad;
        }

        private static SciFiButtonVariant ResolveVariant(Button button)
        {
            if (button.ClassListContains("menu-button--exit")
                || button.ClassListContains("exit-btn")
                || button.name == "exit-button"
                || button.name == "exit-btn")
                return SciFiButtonVariant.Exit;

            if (button.ClassListContains("menu-button--continue")
                || button.ClassListContains("primary-btn")
                || button.ClassListContains("menu-button--primary"))
                return SciFiButtonVariant.Primary;

            return SciFiButtonVariant.Default;
        }
    }
}
