using System;

namespace ArRingTryOn.Core
{
    public static class DisplayFit
    {
        /// <summary>Scale that makes content cover the container, cropping any overflow.</summary>
        public static float FillScale(float contentWidth, float contentHeight, float containerWidth, float containerHeight)
        {
            if (contentWidth <= 0f || contentHeight <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(contentWidth), "Content size must be positive.");
            }

            return Math.Max(containerWidth / contentWidth, containerHeight / contentHeight);
        }
    }
}
