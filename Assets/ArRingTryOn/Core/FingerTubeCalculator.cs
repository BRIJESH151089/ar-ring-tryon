using System;

namespace ArRingTryOn.Core
{
    /// <summary>Builds the ring finger's stand-in tube from its joints (13 to 16).</summary>
    public static class FingerTubeCalculator
    {
        /// <summary>How far the tube reaches back past the knuckle, as a fraction of the first bone.</summary>
        public const float BaseExtension = 0.25f;

        /// <summary>
        /// Radius at each centre (extended base, knuckle, then joints 14 to 16) as a fraction of the
        /// finger's radius. Slightly under 1 so the ring's own inner surface, which sits exactly at
        /// the finger radius, is never hidden by the tube; tapering towards the tip.
        /// </summary>
        public static readonly float[] RadiusFractions = { 0.9f, 0.9f, 0.85f, 0.8f, 0.72f };

        private static readonly HandLandmarkId[] Joints =
        {
            HandLandmarkId.RingMcp,
            HandLandmarkId.RingPip,
            HandLandmarkId.RingDip,
            HandLandmarkId.RingTip,
        };

        /// <summary>Number of points in every tube this builds.</summary>
        public static int CentreCount => Joints.Length + 1;

        /// <param name="hand">The hand to read.</param>
        /// <param name="displayAspect">Width over height of the display space.</param>
        /// <param name="fingerWidth">Finger diameter in display widths, normally the ring pose's scale.</param>
        public static FingerTube Calculate(HandLandmarks hand, float displayAspect, float fingerWidth)
        {
            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand));
            }

            var centres = new Vec3[CentreCount];
            var radii = new float[CentreCount];
            for (var i = 0; i < Joints.Length; i++)
            {
                centres[i + 1] = ViewSpace.FromDisplay(hand[Joints[i]], displayAspect);
            }

            centres[0] = centres[1] - (centres[2] - centres[1]) * BaseExtension;
            for (var i = 0; i < radii.Length; i++)
            {
                radii[i] = 0.5f * fingerWidth * RadiusFractions[i];
            }

            return new FingerTube(centres, radii);
        }
    }
}
