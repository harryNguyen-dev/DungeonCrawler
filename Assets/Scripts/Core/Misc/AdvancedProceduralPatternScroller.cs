using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Scrolls a texture on a RawImage by modifying its UV rect, creating an infinite scrolling effect.
    /// Automatically resets UV values to prevent floating-point precision errors over long periods.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class RawImageScroller : MonoBehaviour
    {
        [Header("Scroll Speed")]
        [Tooltip("Horizontal scroll speed (units per second). Positive = scroll right, Negative = scroll left.")]
        public float scrollSpeedX = 0.1f;

        [Tooltip("Vertical scroll speed (units per second). Positive = scroll up, Negative = scroll down.")]
        public float scrollSpeedY = 0.0f;

        [Header("UV Reset Settings")]
        [Tooltip("Threshold at which the UV coordinates will be reset to prevent precision loss.")]
        public float uvResetThreshold = 100.0f;

        private RawImage rawImage;
        private Rect currentUVRect;

        /// <summary>
        /// Caches the RawImage component and initial UV rect.
        /// </summary>
        private void Awake()
        {
            rawImage = GetComponent<RawImage>();
            currentUVRect = rawImage.uvRect;
        }

        /// <summary>
        /// Updates the UV rect every frame to produce a seamless scrolling effect.
        /// </summary>
        private void Update()
        {
            float offsetX = scrollSpeedX * Time.deltaTime;
            float offsetY = scrollSpeedY * Time.deltaTime;

            currentUVRect.x += offsetX;
            currentUVRect.y += offsetY;

            // Reset UV coordinates if they exceed the threshold
            if (Mathf.Abs(currentUVRect.x) > uvResetThreshold)
            {
                currentUVRect.x = Mathf.Repeat(currentUVRect.x, 1.0f);
            }

            if (Mathf.Abs(currentUVRect.y) > uvResetThreshold)
            {
                currentUVRect.y = Mathf.Repeat(currentUVRect.y, 1.0f);
            }

            rawImage.uvRect = currentUVRect;
        }
    }
}
