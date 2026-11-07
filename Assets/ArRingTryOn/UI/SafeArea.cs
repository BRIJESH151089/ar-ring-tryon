using UnityEngine;

namespace ArRingTryOn.UI
{
    internal static class SafeArea
    {
        /// <summary>
        /// The app draws edge to edge, so this is how far up, in canvas units, controls must sit
        /// to clear the gesture or navigation bar.
        /// </summary>
        public static float BottomInset(Component onCanvas)
        {
            var scaleFactor = onCanvas.GetComponentInParent<Canvas>().rootCanvas.scaleFactor;
            return Screen.safeArea.yMin / scaleFactor;
        }

        public static void LiftAboveSystemBar(RectTransform rectTransform, float margin)
        {
            var y = margin + BottomInset(rectTransform);
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y);
        }
    }
}
