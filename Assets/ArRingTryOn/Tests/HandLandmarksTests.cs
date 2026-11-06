using System;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class HandLandmarksTests
    {
        [Test]
        public void Indexer_ReturnsPointStoredAtLandmarkIndex()
        {
            var hand = HandTestData.Distinct();

            Assert.That(hand[HandLandmarkId.RingMcp].X, Is.EqualTo(13 / 100f).Within(1e-6f));
            Assert.That(hand[HandLandmarkId.PinkyTip].Z, Is.EqualTo(60 / 100f).Within(1e-6f));
        }

        [Test]
        public void Constructor_KeepsSide()
        {
            Assert.That(HandTestData.Distinct(HandSide.Left).Side, Is.EqualTo(HandSide.Left));
        }

        [TestCase(0)]
        [TestCase(20)]
        [TestCase(22)]
        public void Constructor_RejectsWrongLandmarkCount(int count)
        {
            Assert.Throws<ArgumentException>(() => new HandLandmarks(new Vec3[count], HandSide.Left));
        }

        [Test]
        public void Constructor_RejectsNullPoints()
        {
            Assert.Throws<ArgumentNullException>(() => new HandLandmarks(null, HandSide.Left));
        }
    }
}
