using System;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class DisplayFitTests
    {
        [TestCase(720f, 1280f, 1080f, 1920f, 1.5f)]
        [TestCase(100f, 100f, 200f, 400f, 4f)]
        [TestCase(100f, 100f, 400f, 200f, 4f)]
        [TestCase(1000f, 1000f, 500f, 250f, 0.5f)]
        public void FillScale_CoversTheContainer(float contentWidth, float contentHeight, float containerWidth, float containerHeight, float expected)
        {
            var scale = DisplayFit.FillScale(contentWidth, contentHeight, containerWidth, containerHeight);

            Assert.That(scale, Is.EqualTo(expected).Within(1e-5f));
            Assert.That(contentWidth * scale, Is.GreaterThanOrEqualTo(containerWidth - 1e-3f));
            Assert.That(contentHeight * scale, Is.GreaterThanOrEqualTo(containerHeight - 1e-3f));
        }

        [TestCase(0f, 100f)]
        [TestCase(100f, 0f)]
        public void FillScale_RejectsEmptyContent(float width, float height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DisplayFit.FillScale(width, height, 100f, 100f));
        }
    }
}
