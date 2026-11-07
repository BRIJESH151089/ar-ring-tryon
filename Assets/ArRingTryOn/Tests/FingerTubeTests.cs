using System;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class FingerTubeTests
    {
        private const float Tolerance = 1e-5f;

        private static HandLandmarks StraightRingFinger(float aspect = 1f)
        {
            // Upright finger: knuckle at display y 0.5, each bone 0.1 tall, on a square display
            // unless an aspect is given, in which case y is scaled so the finger is the same.
            var points = HandTestData.UprightHand(thumbOnLeft: true);
            points[(int)HandLandmarkId.RingDip] = new Vec3(0.55f, 0.3f, 0f);
            points[(int)HandLandmarkId.RingTip] = new Vec3(0.55f, 0.2f, 0f);
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = new Vec3(points[i].X, 0.5f + (points[i].Y - 0.5f) * aspect, points[i].Z);
            }

            return new HandLandmarks(points, HandSide.Right);
        }

        [Test]
        public void FollowsRingFingerJointsThirteenToSixteen()
        {
            var tube = FingerTubeCalculator.Calculate(StraightRingFinger(), 1f, 0.09f);

            Assert.That(tube.Centres.Count, Is.EqualTo(5));
            AssertVector(tube.Centres[1], new Vec3(0.05f, 0f, 0f));
            AssertVector(tube.Centres[2], new Vec3(0.05f, 0.1f, 0f));
            AssertVector(tube.Centres[3], new Vec3(0.05f, 0.2f, 0f));
            AssertVector(tube.Centres[4], new Vec3(0.05f, 0.3f, 0f));
        }

        [Test]
        public void ReachesBackPastTheKnuckle()
        {
            var tube = FingerTubeCalculator.Calculate(StraightRingFinger(), 1f, 0.09f);

            AssertVector(tube.Centres[0], new Vec3(0.05f, -0.1f * FingerTubeCalculator.BaseExtension, 0f));
        }

        [Test]
        public void RadiusFitsInsideTheRingAndTapersToTheTip()
        {
            const float fingerWidth = 0.09f;
            var tube = FingerTubeCalculator.Calculate(StraightRingFinger(), 1f, fingerWidth);

            for (var i = 0; i < tube.Radii.Count; i++)
            {
                Assert.That(tube.Radii[i], Is.LessThan(0.5f * fingerWidth), $"centre {i} would hide the ring's inner surface");
                Assert.That(tube.Radii[i], Is.GreaterThan(0f));
                if (i > 0)
                {
                    Assert.That(tube.Radii[i], Is.LessThanOrEqualTo(tube.Radii[i - 1]), $"centre {i} widens");
                }
            }
        }

        [Test]
        public void RadiusScalesWithFingerWidth()
        {
            var narrow = FingerTubeCalculator.Calculate(StraightRingFinger(), 1f, 0.05f);
            var wide = FingerTubeCalculator.Calculate(StraightRingFinger(), 1f, 0.1f);

            Assert.That(wide.Radii[1], Is.EqualTo(2f * narrow.Radii[1]).Within(Tolerance));
        }

        [Test]
        public void UsesTheSameSpaceAsTheRingPose()
        {
            // On a 9:16 display the tube must still run through the ring's centre.
            const float aspect = 9f / 16f;
            var hand = StraightRingFinger(aspect);
            RingPoseCalculator.TryCalculate(hand, aspect, false, out var pose);

            var tube = FingerTubeCalculator.Calculate(hand, aspect, pose.Scale);

            var along = Vec3.Lerp(tube.Centres[1], tube.Centres[2], RingPoseCalculator.DefaultPositionAlongSegment);
            AssertVector(along, pose.Position);
        }

        [Test]
        public void RejectsNullHand()
        {
            Assert.Throws<ArgumentNullException>(() => FingerTubeCalculator.Calculate(null, 1f, 0.1f));
        }

        [Test]
        public void Tube_RejectsMismatchedOrTooFewPoints()
        {
            Assert.Throws<ArgumentException>(() => new FingerTube(new[] { new Vec3(0f, 0f, 0f) }, new[] { 1f }));
            Assert.Throws<ArgumentException>(() => new FingerTube(new Vec3[3], new float[2]));
        }

        private static void AssertVector(Vec3 actual, Vec3 expected)
        {
            Assert.That(actual.X, Is.EqualTo(expected.X).Within(Tolerance), "x");
            Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(Tolerance), "y");
            Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(Tolerance), "z");
        }
    }
}
