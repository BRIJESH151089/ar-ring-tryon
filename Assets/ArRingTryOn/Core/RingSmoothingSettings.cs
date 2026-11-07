using System;

namespace ArRingTryOn.Core
{
    /// <summary>Tuning for how the ring follows the hand. Position speeds are in display widths per
    /// second, rotation speeds in radians per second.</summary>
    [Serializable]
    public struct RingSmoothingSettings
    {
        public OneEuroSettings Position;
        public OneEuroSettings Rotation;
        public OneEuroSettings Scale;

        /// <summary>
        /// Largest change in ring size per second, as a natural-log ratio (0.5 is about 65% per
        /// second). Damps the brief shrink when the hand turns edge-on. Zero or less disables it.
        /// </summary>
        public float MaxScaleChangePerSecond;

        public static RingSmoothingSettings Default => new RingSmoothingSettings
        {
            Position = new OneEuroSettings(1.5f, 4f),
            Rotation = new OneEuroSettings(1.5f, 1.5f),
            Scale = new OneEuroSettings(0.8f, 2f),
            MaxScaleChangePerSecond = 0.6f,
        };
    }
}
