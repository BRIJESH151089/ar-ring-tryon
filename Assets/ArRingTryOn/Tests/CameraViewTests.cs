using System;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class CameraViewTests
    {
        private const float Tolerance = 1e-5f;

        [TestCase(0, 1280, 720)]
        [TestCase(90, 720, 1280)]
        [TestCase(180, 1280, 720)]
        [TestCase(270, 720, 1280)]
        public void DisplaySize_SwapsAxesOnQuarterTurns(int rotation, int expectedWidth, int expectedHeight)
        {
            var view = new CameraView(1280, 720, rotation, false, false);

            Assert.That(view.DisplayWidth, Is.EqualTo(expectedWidth));
            Assert.That(view.DisplayHeight, Is.EqualTo(expectedHeight));
            Assert.That(view.DisplayAspect, Is.EqualTo((float)expectedWidth / expectedHeight).Within(Tolerance));
        }

        [TestCase(-90, 270)]
        [TestCase(360, 0)]
        [TestCase(450, 90)]
        public void Rotation_IsNormalized(int rotation, int expected)
        {
            Assert.That(new CameraView(640, 480, rotation, false, false).RotationDegrees, Is.EqualTo(expected));
        }

        // The image's top-left corner, turned clockwise about the centre.
        [TestCase(0, 0f, 0f)]
        [TestCase(90, 1f, 0f)]
        [TestCase(180, 1f, 1f)]
        [TestCase(270, 0f, 1f)]
        public void ToDisplay_RotatesTopLeftCornerClockwise(int rotation, float expectedX, float expectedY)
        {
            var point = new CameraView(640, 480, rotation, false, false).ToDisplay(new Vec3(0f, 0f, 0f));

            Assert.That(point.X, Is.EqualTo(expectedX).Within(Tolerance));
            Assert.That(point.Y, Is.EqualTo(expectedY).Within(Tolerance));
        }

        [TestCase(0, 0.2f, 0.1f)]
        [TestCase(90, 0.9f, 0.2f)]
        [TestCase(180, 0.8f, 0.9f)]
        [TestCase(270, 0.1f, 0.8f)]
        public void ToDisplay_MapsAnOffCentrePoint(int rotation, float expectedX, float expectedY)
        {
            var point = new CameraView(640, 480, rotation, false, false).ToDisplay(new Vec3(0.2f, 0.1f, 0f));

            Assert.That(point.X, Is.EqualTo(expectedX).Within(Tolerance));
            Assert.That(point.Y, Is.EqualTo(expectedY).Within(Tolerance));
        }

        [TestCase(0)]
        [TestCase(90)]
        [TestCase(180)]
        [TestCase(270)]
        public void ToDisplay_KeepsCentreFixed(int rotation)
        {
            var point = new CameraView(640, 480, rotation, false, false).ToDisplay(new Vec3(0.5f, 0.5f, 0f));

            Assert.That(point.X, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(point.Y, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [TestCase(0, 0.1f)]
        [TestCase(180, 0.1f)]
        [TestCase(90, 0.2f)]
        [TestCase(270, 0.2f)]
        public void ToDisplay_RescalesDepthToDisplayWidths(int rotation, float expectedZ)
        {
            var point = new CameraView(1000, 500, rotation, false, false).ToDisplay(new Vec3(0f, 0f, 0.1f));

            Assert.That(point.Z, Is.EqualTo(expectedZ).Within(Tolerance));
        }

        // Mirrors how the MediaPipe sample screen derives its UV rect from the image source.
        [TestCase(0, false, false, false, false)]
        [TestCase(90, false, false, false, false)]
        [TestCase(90, false, true, false, true)]
        [TestCase(0, true, false, true, false)]
        [TestCase(180, true, false, true, false)]
        [TestCase(90, true, false, false, true)]
        [TestCase(270, true, false, false, true)]
        [TestCase(270, true, true, false, false)]
        [TestCase(0, true, true, true, true)]
        public void TextureFlips_MatchTrackerInput(int rotation, bool front, bool verticallyMirrored, bool expectedU, bool expectedV)
        {
            var view = new CameraView(640, 480, rotation, front, verticallyMirrored);

            Assert.That(view.FlipTextureU, Is.EqualTo(expectedU));
            Assert.That(view.FlipTextureV, Is.EqualTo(expectedV));
        }

        [TestCase(false, HandSide.Left, HandSide.Right)]
        [TestCase(false, HandSide.Right, HandSide.Left)]
        [TestCase(false, HandSide.Unknown, HandSide.Unknown)]
        [TestCase(true, HandSide.Left, HandSide.Left)]
        [TestCase(true, HandSide.Right, HandSide.Right)]
        public void ResolveSide_SwapsLabelsOnlyForUnmirroredImages(bool front, HandSide reported, HandSide expected)
        {
            Assert.That(new CameraView(640, 480, 0, front, false).ResolveSide(reported), Is.EqualTo(expected));
        }

        [TestCase(0, 480)]
        [TestCase(640, 0)]
        [TestCase(-1, 480)]
        public void Constructor_RejectsInvalidSize(int width, int height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CameraView(width, height, 0, false, false));
        }

        [Test]
        public void Constructor_RejectsNonRightAngleRotation()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CameraView(640, 480, 45, false, false));
        }
    }
}
