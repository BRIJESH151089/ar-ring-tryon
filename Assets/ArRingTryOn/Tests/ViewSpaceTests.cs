using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class ViewSpaceTests
    {
        private const float Tolerance = 1e-5f;

        [Test]
        public void FromDisplay_PutsDisplayCentreAtOrigin()
        {
            var point = ViewSpace.FromDisplay(new Vec3(0.5f, 0.5f, 0f), 0.5f);

            Assert.That(point.X, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(point.Y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void FromDisplay_TurnsDownwardYIntoUpwardY()
        {
            var top = ViewSpace.FromDisplay(new Vec3(0.5f, 0f, 0f), 1f);

            Assert.That(top.Y, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void FromDisplay_MeasuresHeightInDisplayWidths()
        {
            // A 9:16 display is 16/9 widths tall, so its top edge is 8/9 of a width above centre.
            var top = ViewSpace.FromDisplay(new Vec3(0.5f, 0f, 0f), 9f / 16f);

            Assert.That(top.Y, Is.EqualTo(8f / 9f).Within(Tolerance));
        }

        [Test]
        public void FromDisplay_KeepsDepth()
        {
            Assert.That(ViewSpace.FromDisplay(new Vec3(0f, 0f, 0.25f), 2f).Z, Is.EqualTo(0.25f));
        }
    }
}
