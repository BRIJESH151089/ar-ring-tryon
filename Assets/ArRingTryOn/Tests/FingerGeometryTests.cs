using System;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class FingerGeometryTests
    {
        private const float Tolerance = 1e-5f;

        [Test]
        public void TryGetPose_ReportsFingerAndBaseJoint()
        {
            var hand = HandTestData.Distinct();

            Assert.That(FingerGeometry.TryGetPose(hand, Finger.Ring, 1f, out var pose), Is.True);

            Assert.That(pose.Finger, Is.EqualTo(Finger.Ring));
            AssertVector(pose.BasePosition, hand[HandLandmarkId.RingMcp]);
        }

        [Test]
        public void TryGetPose_PointsFromBaseJointTowardsNextJoint()
        {
            var hand = HandTestData.With(
                (HandLandmarkId.IndexMcp, new Vec3(0.5f, 0.5f, 0f)),
                (HandLandmarkId.IndexPip, new Vec3(0.5f, 0.3f, 0f)));

            FingerGeometry.TryGetPose(hand, Finger.Index, 1f, out var pose);

            AssertVector(pose.Direction, new Vec3(0f, -1f, 0f));
            Assert.That(pose.SegmentLength, Is.EqualTo(0.2f).Within(Tolerance));
        }

        [Test]
        public void TryGetPose_DirectionIsUnitLengthIncludingDepth()
        {
            var hand = HandTestData.With(
                (HandLandmarkId.PinkyMcp, new Vec3(0.1f, 0.2f, 0.3f)),
                (HandLandmarkId.PinkyPip, new Vec3(0.4f, 0.2f, 0.7f)));

            FingerGeometry.TryGetPose(hand, Finger.Pinky, 1f, out var pose);

            Assert.That(pose.Direction.Length, Is.EqualTo(1f).Within(Tolerance));
            AssertVector(pose.Direction, new Vec3(0.6f, 0f, 0.8f));
            Assert.That(pose.SegmentLength, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void TryGetPose_CorrectsForNonSquareDisplay()
        {
            // On a 9:16 display, 0.16 of the width and 0.09 of the height are the same distance,
            // so this finger is at 45 degrees even though its normalized deltas differ.
            var hand = HandTestData.With(
                (HandLandmarkId.MiddleMcp, new Vec3(0.5f, 0.5f, 0f)),
                (HandLandmarkId.MiddlePip, new Vec3(0.66f, 0.41f, 0f)));

            FingerGeometry.TryGetPose(hand, Finger.Middle, 9f / 16f, out var pose);

            var expected = (float)(1.0 / Math.Sqrt(2.0));
            AssertVector(pose.Direction, new Vec3(expected, -expected, 0f));
        }

        [Test]
        public void TryGetPose_UsesThumbMcpAsThumbBase()
        {
            var hand = HandTestData.Distinct();

            FingerGeometry.TryGetPose(hand, Finger.Thumb, 1f, out var pose);

            AssertVector(pose.BasePosition, hand[HandLandmarkId.ThumbMcp]);
        }

        [Test]
        public void TryGetPose_ReturnsFalseWhenJointsCoincide()
        {
            var hand = HandTestData.With();

            Assert.That(FingerGeometry.TryGetPose(hand, Finger.Ring, 1f, out _), Is.False);
        }

        [Test]
        public void TryGetPose_RejectsNullHand()
        {
            Assert.Throws<ArgumentNullException>(() => FingerGeometry.TryGetPose(null, Finger.Ring, 1f, out _));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void TryGetPose_RejectsNonPositiveAspect(float aspect)
        {
            var hand = HandTestData.Distinct();

            Assert.Throws<ArgumentOutOfRangeException>(() => FingerGeometry.TryGetPose(hand, Finger.Ring, aspect, out _));
        }

        private static void AssertVector(Vec3 actual, Vec3 expected)
        {
            Assert.That(actual.X, Is.EqualTo(expected.X).Within(Tolerance), "x");
            Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(Tolerance), "y");
            Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(Tolerance), "z");
        }
    }
}
