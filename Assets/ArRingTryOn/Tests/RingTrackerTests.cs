using System;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class RingTrackerTests
    {
        private static HandFrame Frame(long timestampMs, float xOffset = 0f, bool withHand = true)
        {
            if (!withHand)
            {
                return new HandFrame(timestampMs, 1f, false, Array.Empty<HandLandmarks>());
            }

            var points = HandTestData.UprightHand(thumbOnLeft: true);
            points[(int)HandLandmarkId.RingDip] = new Vec3(0.55f, 0.3f, 0f);
            points[(int)HandLandmarkId.RingTip] = new Vec3(0.55f, 0.2f, 0f);
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = new Vec3(points[i].X + xOffset, points[i].Y, points[i].Z);
            }

            return new HandFrame(timestampMs, 1f, false, new[] { new HandLandmarks(points, HandSide.Right) });
        }

        [Test]
        public void FirstFrame_MatchesTheUnsmoothedPose()
        {
            var tracker = new RingTracker(RingSmoothingSettings.Default);

            Assert.That(tracker.TryUpdate(Frame(0), out var pose, out var tube), Is.True);

            RingPoseCalculator.TryCalculate(Frame(0).Hands[0], 1f, false, out var raw);
            Assert.That((pose.Position - raw.Position).Length, Is.LessThan(1e-6f));
            Assert.That(pose.Scale, Is.EqualTo(raw.Scale));
            Assert.That(tube.Centres.Count, Is.EqualTo(FingerTubeCalculator.CentreCount));
        }

        [Test]
        public void KeepsRollFromThePoseCalculator()
        {
            var tracker = new RingTracker(RingSmoothingSettings.Default);
            for (var t = 0; t < 10; t++)
            {
                tracker.TryUpdate(Frame(t * 33), out _, out _);
            }

            tracker.TryUpdate(Frame(330), out var pose, out _);

            var top = pose.Rotation.Rotate(new Vec3(0f, 0f, 1f));
            Assert.That(top.Z, Is.EqualTo(-1f).Within(1e-3f), "head must still face the camera for a right hand's back");
        }

        [Test]
        public void RingAndTube_MoveTogether()
        {
            var tracker = new RingTracker(RingSmoothingSettings.Default);
            tracker.TryUpdate(Frame(0), out _, out _);

            for (var t = 1; t <= 6; t++)
            {
                tracker.TryUpdate(Frame(t * 33, xOffset: 0.1f), out var pose, out var tube);
                var onTube = Vec3.Lerp(tube.Centres[1], tube.Centres[2], tracker.PositionAlongSegment);
                Assert.That((onTube - pose.Position).Length, Is.LessThan(0.002f), $"frame {t}: ring left the finger");
            }
        }

        [Test]
        public void TubeIsSizedFromTheSmoothedRing()
        {
            var tracker = new RingTracker(RingSmoothingSettings.Default);

            tracker.TryUpdate(Frame(0), out var pose, out var tube);

            Assert.That(tube.Radii[1], Is.EqualTo(0.5f * pose.Scale * FingerTubeCalculator.RadiusFractions[1]).Within(1e-6f));
        }

        [Test]
        public void LosingTheHand_ReportsNothingAndStartsAfresh()
        {
            var tracker = new RingTracker(RingSmoothingSettings.Default);
            tracker.TryUpdate(Frame(0), out _, out _);
            tracker.TryUpdate(Frame(33), out _, out _);

            Assert.That(tracker.TryUpdate(Frame(66, withHand: false), out _, out _), Is.False);

            tracker.TryUpdate(Frame(1000, xOffset: 0.2f), out var pose, out _);
            RingPoseCalculator.TryCalculate(Frame(1000, xOffset: 0.2f).Hands[0], 1f, false, out var raw);
            Assert.That((pose.Position - raw.Position).Length, Is.LessThan(1e-6f), "ring should not glide from the old spot");
        }

        [Test]
        public void PositionAlongSegment_DefaultsToTheCalculatorsDefault()
        {
            Assert.That(new RingTracker(RingSmoothingSettings.Default).PositionAlongSegment,
                Is.EqualTo(RingPoseCalculator.DefaultPositionAlongSegment));
        }

        [Test]
        public void PositionAlongSegment_MovesTheRingButNotTheTube()
        {
            var tracker = new RingTracker(RingSmoothingSettings.Default) { PositionAlongSegment = 0.8f };

            tracker.TryUpdate(Frame(0), out var pose, out var tube);

            var expected = Vec3.Lerp(tube.Centres[1], tube.Centres[2], 0.8f);
            Assert.That((pose.Position - expected).Length, Is.LessThan(1e-6f));
            var defaultTube = FingerTubeCalculator.Calculate(Frame(0).Hands[0], 1f, pose.Scale);
            Assert.That((tube.Centres[1] - defaultTube.Centres[1]).Length, Is.LessThan(1e-6f));
        }

        [TestCase(-0.5f, 0f)]
        [TestCase(1.5f, 1f)]
        [TestCase(float.NaN, RingPoseCalculator.DefaultPositionAlongSegment)]
        public void PositionAlongSegment_IsKeptOnTheBone(float requested, float expected)
        {
            var tracker = new RingTracker(RingSmoothingSettings.Default) { PositionAlongSegment = requested };

            Assert.That(tracker.PositionAlongSegment, Is.EqualTo(expected));
            Assert.That(tracker.TryUpdate(Frame(0), out _, out _), Is.True);
        }

        [Test]
        public void NullFrame_ReportsNothing()
        {
            Assert.That(new RingTracker(RingSmoothingSettings.Default).TryUpdate(null, out _, out _), Is.False);
        }
    }
}
