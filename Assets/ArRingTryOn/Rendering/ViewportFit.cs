using ArRingTryOn.Core;
using UnityEngine;

namespace ArRingTryOn.Rendering
{
    internal static class ViewportFit
    {
        /// <summary>Centres a rect in its parent and returns the scale that fills the parent with the display image.</summary>
        public static float Centre(RectTransform rectTransform, CameraView view)
        {
            var centre = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = centre;
            rectTransform.anchorMax = centre;
            rectTransform.pivot = centre;
            rectTransform.anchoredPosition = Vector2.zero;

            var parent = ((RectTransform)rectTransform.parent).rect;
            return DisplayFit.FillScale(view.DisplayWidth, view.DisplayHeight, parent.width, parent.height);
        }
    }
}
