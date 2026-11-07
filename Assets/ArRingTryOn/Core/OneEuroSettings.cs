using System;

namespace ArRingTryOn.Core
{
    /// <summary>
    /// Tuning for a One-Euro filter (Casiez et al., 2012): a low-pass filter whose cutoff rises
    /// with speed, so slow movement is smoothed hard and fast movement follows closely.
    /// </summary>
    [Serializable]
    public struct OneEuroSettings
    {
        /// <summary>Cutoff in Hz when still. Lower removes more jitter but lags more.</summary>
        public float MinCutoff;

        /// <summary>How much the cutoff rises per unit of speed. Higher reduces lag on fast moves.</summary>
        public float Beta;

        /// <summary>Cutoff in Hz for the speed estimate itself.</summary>
        public float DerivativeCutoff;

        public OneEuroSettings(float minCutoff, float beta, float derivativeCutoff = 1f)
        {
            MinCutoff = minCutoff;
            Beta = beta;
            DerivativeCutoff = derivativeCutoff;
        }

        /// <summary>The blend weight for a new sample after <paramref name="dt"/> seconds at this cutoff.</summary>
        internal static float Alpha(double cutoff, double dt)
        {
            var tau = 1.0 / (2.0 * Math.PI * Math.Max(cutoff, 1e-6));
            return (float)(1.0 / (1.0 + tau / dt));
        }

        internal float CutoffAt(float speed) => MinCutoff + Beta * speed;
    }
}
