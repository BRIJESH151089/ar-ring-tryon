using System;

namespace ArRingTryOn.Core
{
    /// <summary>Works out where a ring sits on the ring finger from hand landmarks.</summary>
    public static class RingPoseCalculator
    {
        /// <summary>How far along the segment from the base joint to the next joint the ring sits.</summary>
        public const float PositionAlongSegment = 0.35f;

        /// <summary>Finger diameter as a fraction of the spacing between neighbouring knuckles.</summary>
        public const float FingerWidthPerKnuckleSpacing = 0.9f;

        private const float MinLength = 1e-6f;

        /// <param name="hand">The hand to place the ring on.</param>
        /// <param name="displayAspect">Width over height of the display space.</param>
        /// <param name="isImageMirrored">True if the landmarks come from a mirrored image.</param>
        /// <param name="pose">The result, when this returns true.</param>
        /// <returns>False if the landmarks are too degenerate to place a ring.</returns>
        public static bool TryCalculate(HandLandmarks hand, float displayAspect, bool isImageMirrored, out RingPose pose)
        {
            pose = default;
            if (!FingerGeometry.TryGetPose(hand, Finger.Ring, displayAspect, out var finger))
            {
                return false;
            }

            var knuckle = ViewSpace.FromDisplay(finger.BasePosition, displayAspect);
            var fingerSpacing = KnuckleSpacing(hand, knuckle, displayAspect);
            if (fingerSpacing < MinLength)
            {
                return false;
            }

            // Finger direction is in display widths with y down; view space has y up.
            var axis = new Vec3(finger.Direction.X, -finger.Direction.Y, finger.Direction.Z);
            var position = knuckle + axis * (finger.SegmentLength * PositionAlongSegment);
            var rotation = BuildRotation(axis, BackOfFinger(hand, axis, displayAspect, isImageMirrored));

            pose = new RingPose(position, rotation, fingerSpacing * FingerWidthPerKnuckleSpacing);
            return true;
        }

        private static float KnuckleSpacing(HandLandmarks hand, Vec3 knuckle, float displayAspect)
        {
            var middle = ViewSpace.FromDisplay(hand[HandLandmarkId.MiddleMcp], displayAspect);
            var pinky = ViewSpace.FromDisplay(hand[HandLandmarkId.PinkyMcp], displayAspect);
            return ((knuckle - middle).Length + (knuckle - pinky).Length) * 0.5f;
        }

        // The finger bends about the line of knuckles, so crossing the finger with that line
        // gives the back of the finger even when it curls. Which of the two sides that is depends
        // on the hand's shape in the image: a right hand looks like a mirrored left hand.
        private static Vec3 BackOfFinger(HandLandmarks hand, Vec3 axis, float displayAspect, bool isImageMirrored)
        {
            var middle = ViewSpace.FromDisplay(hand[HandLandmarkId.MiddleMcp], displayAspect);
            var pinky = ViewSpace.FromDisplay(hand[HandLandmarkId.PinkyMcp], displayAspect);
            var acrossKnuckles = pinky - middle;
            var side = Vec3.Cross(axis, acrossKnuckles);

            var hasRightHandShape = (hand.Side != HandSide.Left) != isImageMirrored;
            return hasRightHandShape ? side : side * -1f;
        }

        private static Quat BuildRotation(Vec3 axis, Vec3 backOfFinger)
        {
            var top = RemoveComponent(backOfFinger, axis);
            if (top.Length < MinLength)
            {
                // The knuckles give no usable roll, so fall back on any direction across the finger.
                top = RemoveComponent(new Vec3(0f, 0f, -1f), axis);
            }

            if (top.Length < MinLength)
            {
                top = RemoveComponent(new Vec3(0f, 1f, 0f), axis);
            }

            top = top.Normalized;
            return Quat.FromBasis(Vec3.Cross(axis, top), axis, top);
        }

        private static Vec3 RemoveComponent(Vec3 v, Vec3 unitAxis)
        {
            return v - unitAxis * Vec3.Dot(v, unitAxis);
        }
    }
}
