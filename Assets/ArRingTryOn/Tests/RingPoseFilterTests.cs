using System;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class RingPoseFilterTests
    {
        private const double FrameSeconds = 1.0 / 30.0;

        [Test]
        public void SteadyPose_StaysSteady()
        {
            var filter = new RingPoseFilter(RingSmoothingSettings.Default);
            var pose = new RingPose(new Vec3(0.1f, -0.2f, 0.05f), OneEuroFilterTests.AboutY(30f), 0.12f);

            for (var i = 0; i < 60; i++)
            {
                var output = filter.Filter(pose, i * FrameSeconds);
                Assert.That((output.Position - pose.Position).Length, Is.LessThan(1e-6f));
                Assert.That(Quat.Angle(output.Rotation, pose.Rotation), Is.LessThan(1e-3f));
                Assert.That(output.Scale, Is.EqualTo(pose.Scale).Within(1e-6f));
            }
        }

        [Test]
        public void StepInPose_ConvergesOnEveryChannel()
        {
            var filter = new RingPoseFilter(RingSmoothingSettings.Default);
            filter.Filter(new RingPose(new Vec3(0f, 0f, 0f), Quat.Identity, 0.1f), 0.0);
            var target = new RingPose(new Vec3(0.2f, 0.1f, 0f), OneEuroFilterTests.AboutY(60f), 0.12f);

            RingPose output = default;
            for (var i = 1; i <= 120; i++)
            {
                output = filter.Filter(target, i * FrameSeconds);
            }

            Assert.That((output.Position - target.Position).Length, Is.LessThan(1e-3f));
            Assert.That(Quat.Angle(output.Rotation, target.Rotation), Is.LessThan(0.01f));
            Assert.That(output.Scale, Is.EqualTo(target.Scale).Within(1e-3f));
        }

        [Test]
        public void ScaleDip_IsLimitedInRate()
        {
            // The hand turns edge-on for a third of a second and the raw size drops by 40%.
            var settings = RingSmoothingSettings.Default;
            var filter = new RingPoseFilter(settings);
            var pose = new RingPose(new Vec3(0f, 0f, 0f), Quat.Identity, 0.1f);
            for (var i = 0; i < 30; i++)
            {
                filter.Filter(pose, i * FrameSeconds);
            }

            var smallest = float.MaxValue;
            var dipped = new RingPose(pose.Position, pose.Rotation, 0.06f);
            for (var i = 30; i < 40; i++)
            {
                smallest = Math.Min(smallest, filter.Filter(dipped, i * FrameSeconds).Scale);
            }

            var allowed = 0.1f * (float)Math.Exp(-settings.MaxScaleChangePerSecond * 10 * FrameSeconds);
            Assert.That(smallest, Is.GreaterThanOrEqualTo(allowed - 1e-5f));
            Assert.That(smallest, Is.GreaterThan(0.08f), "a brief 40% dip should shrink the ring by under half as much");
        }

        [Test]
        public void ScaleRateLimit_CanBeTurnedOff()
        {
            var settings = RingSmoothingSettings.Default;
            settings.MaxScaleChangePerSecond = 0f;
            settings.Scale = new OneEuroSettings(1000f, 0f);
            var filter = new RingPoseFilter(settings);
            filter.Filter(new RingPose(new Vec3(0f, 0f, 0f), Quat.Identity, 0.1f), 0.0);

            var output = filter.Filter(new RingPose(new Vec3(0f, 0f, 0f), Quat.Identity, 0.05f), FrameSeconds);

            Assert.That(output.Scale, Is.EqualTo(0.05f).Within(1e-3f));
        }

        [Test]
        public void Reset_JumpsStraightToTheNextPose()
        {
            var filter = new RingPoseFilter(RingSmoothingSettings.Default);
            filter.Filter(new RingPose(new Vec3(0f, 0f, 0f), Quat.Identity, 0.1f), 0.0);
            filter.Reset();

            var next = new RingPose(new Vec3(0.4f, 0.3f, 0f), OneEuroFilterTests.AboutY(90f), 0.05f);
            var output = filter.Filter(next, 5.0);

            Assert.That((output.Position - next.Position).Length, Is.LessThan(1e-6f));
            Assert.That(Quat.Angle(output.Rotation, next.Rotation), Is.LessThan(1e-3f));
            Assert.That(output.Scale, Is.EqualTo(0.05f));
        }

        [Test]
        public void ChangingSettings_TakesEffectImmediately()
        {
            var filter = new RingPoseFilter(RingSmoothingSettings.Default);
            filter.Filter(new RingPose(new Vec3(0f, 0f, 0f), Quat.Identity, 0.1f), 0.0);

            var loose = RingSmoothingSettings.Default;
            loose.Position = new OneEuroSettings(1000f, 0f);
            filter.Settings = loose;
            var output = filter.Filter(new RingPose(new Vec3(1f, 0f, 0f), Quat.Identity, 0.1f), FrameSeconds);

            Assert.That(output.Position.X, Is.GreaterThan(0.99f));
        }
    }
}
