using System;
using UnityEngine;

namespace ArRingTryOn.Rendering
{
    /// <summary>
    /// How much metal the band has, as fractions of the finger's diameter. Changing these never
    /// changes the hole, so the ring keeps fitting the finger.
    /// </summary>
    [Serializable]
    public struct RingBandSettings
    {
        public const float MinSize = 0.02f;
        public const float MaxThickness = 0.4f;
        public const float MaxWidth = 0.6f;

        [Tooltip("Metal from the inside of the band to the outside. 0.18 is about 3 mm on an average finger.")]
        [Range(MinSize, MaxThickness)] public float Thickness;

        [Tooltip("Band width along the finger. 0.26 is about 4.7 mm on an average finger.")]
        [Range(MinSize, MaxWidth)] public float Width;

        public RingBandSettings(float thickness, float width)
        {
            Thickness = thickness;
            Width = width;
        }

        public static RingBandSettings Default => new RingBandSettings(0.18f, 0.26f);

        /// <summary>The same settings kept within a buildable range.</summary>
        public RingBandSettings Clamped => new RingBandSettings(
            Mathf.Clamp(float.IsNaN(Thickness) ? Default.Thickness : Thickness, MinSize, MaxThickness),
            Mathf.Clamp(float.IsNaN(Width) ? Default.Width : Width, MinSize, MaxWidth));

        internal float HalfThickness => 0.5f * Thickness;

        internal float HalfWidth => 0.5f * Width;
    }
}
