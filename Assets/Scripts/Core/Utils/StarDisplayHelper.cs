using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public static class StarDisplayHelper
    {
        public static void Apply(Image star1, Image star2, Image star3, int earnedStars)
        {
            SetStar(star1, earnedStars >= 1);
            SetStar(star2, earnedStars >= 2);
            SetStar(star3, earnedStars >= 3);
        }

        private static void SetStar(Image image, bool earned)
        {
            if (image == null)
                return;

            image.color = earned ? ColorUtils.Yellow : ColorUtils.Black;
        }
    }
}
