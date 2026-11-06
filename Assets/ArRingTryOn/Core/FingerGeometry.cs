using System;

namespace ArRingTryOn.Core
{
    /// <summary>Derives a finger's base joint and direction from hand landmarks.</summary>
    public static class FingerGeometry
    {
        private const float MinSegmentLength = 1e-6f;

        /// <param name="hand">The hand to read.</param>
        /// <param name="finger">The finger to measure.</param>
        /// <param name="displayAspect">Width over height of the display space.</param>
        /// <param name="pose">The result, when this returns true.</param>
        /// <returns>False if the segment's joints coincide, so it has no direction.</returns>
        public static bool TryGetPose(HandLandmarks hand, Finger finger, float displayAspect, out FingerPose pose)
        {
            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand));
            }

            if (displayAspect <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(displayAspect), displayAspect, "Aspect must be positive.");
            }

            var (baseId, nextId) = HandTopology.RingSegment(finger);
            var basePosition = hand[baseId];
            var segment = ToDisplayWidths(hand[nextId], displayAspect) - ToDisplayWidths(basePosition, displayAspect);
            var length = segment.Length;

            if (length < MinSegmentLength)
            {
                pose = default;
                return false;
            }

            pose = new FingerPose(finger, basePosition, segment * (1f / length), length);
            return true;
        }

        // Display y is normalized by height, so on a non-square image it must be rescaled
        // before angles and lengths mean anything.
        private static Vec3 ToDisplayWidths(Vec3 point, float displayAspect)
        {
            return new Vec3(point.X, point.Y / displayAspect, point.Z);
        }
    }
}
