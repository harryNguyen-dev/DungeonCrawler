using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public static class StarDisplayHelper
    {
        public const int MaxStars = 3;

        private static readonly Color UnearnedColor = new(ColorUtils.Gray.r, ColorUtils.Gray.g, ColorUtils.Gray.b, 0.35f);

        public static void Apply(Image star1, Image star2, Image star3, int earnedStars)
        {
            earnedStars = Mathf.Clamp(earnedStars, 0, MaxStars);

            SetStar(star1, earnedStars >= 1);
            SetStar(star2, earnedStars >= 2);
            SetStar(star3, earnedStars >= 3);
        }

        public static void Apply(
            Image star1,
            Image star2,
            Image star3,
            TMP_Text label,
            int earnedStars)
        {
            Apply(star1, star2, star3, earnedStars);
            SetLabel(label, earnedStars);
        }

        public static void SetLabel(TMP_Text label, int earnedStars)
        {
            if (label == null)
                return;

            earnedStars = Mathf.Clamp(earnedStars, 0, MaxStars);
            label.text = $"{earnedStars}/{MaxStars}";
        }

        private static void SetStar(Image image, bool earned)
        {
            if (image == null)
                return;

            image.color = earned ? ColorUtils.Yellow : UnearnedColor;
        }
    }
}
