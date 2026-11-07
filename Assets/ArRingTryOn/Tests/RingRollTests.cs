using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    /// <summary>
    /// The ring's head must sit on the back of the finger for either hand on either camera. These
    /// run the whole chain the app uses: the tracker's label, <see cref="CameraView.ResolveSide"/>
    /// and <see cref="RingPoseCalculator"/>.
    /// </summary>
    /// <remarks>
    /// The tracker labels a hand by how it looks in the image it is given. That was confirmed on
    /// a phone: a right hand on the rear camera put the head on the palm while we assumed the
    /// opposite convention.
    /// </remarks>
    public class RingRollTests
    {
        private const float Tolerance = 1e-4f;

        private static readonly Vec3 RingTop = new Vec3(0f, 0f, 1f);
        private static readonly Vec3 TowardsCamera = new Vec3(0f, 0f, -1f);
        private static readonly Vec3 AwayFromCamera = new Vec3(0f, 0f, 1f);

        public enum Facing
        {
            BackToCamera,
            PalmToCamera,
        }

        [TestCase(HandSide.Right, false, Facing.BackToCamera)]
        [TestCase(HandSide.Left, false, Facing.BackToCamera)]
        [TestCase(HandSide.Right, true, Facing.BackToCamera)]
        [TestCase(HandSide.Left, true, Facing.BackToCamera)]
        [TestCase(HandSide.Right, false, Facing.PalmToCamera)]
        [TestCase(HandSide.Left, false, Facing.PalmToCamera)]
        [TestCase(HandSide.Right, true, Facing.PalmToCamera)]
        [TestCase(HandSide.Left, true, Facing.PalmToCamera)]
        public void HeadSitsOnBackOfFinger(HandSide realHand, bool frontCamera, Facing facing)
        {
            var view = new CameraView(1280, 720, 0, frontCamera, false);
            var points = HandAsSeen(realHand, frontCamera, facing);

            var reported = ReportedLabel(realHand, frontCamera);
            var hand = new HandLandmarks(points, view.ResolveSide(reported));
            Assert.That(hand.Side, Is.EqualTo(realHand), "resolved side");

            Assert.That(RingPoseCalculator.TryCalculate(hand, 1f, view.IsFrontFacing, out var pose), Is.True);
            var expected = facing == Facing.BackToCamera ? TowardsCamera : AwayFromCamera;
            AssertVector(pose.Rotation.Rotate(RingTop), expected);
        }

        // An upright hand as the image shows it. Seen from behind, a right hand has its thumb on
        // the left; showing the palm, a left hand instead, or a mirrored camera each flip that.
        private static Vec3[] HandAsSeen(HandSide realHand, bool mirrored, Facing facing)
        {
            var thumbOnLeft = (realHand == HandSide.Right) ^ (facing == Facing.PalmToCamera) ^ mirrored;
            return HandTestData.UprightHand(thumbOnLeft);
        }

        // The tracker names the hand it sees in its image, so a mirror swaps the name. Facing does
        // not change it: a right hand is still a right hand when it shows its palm.
        private static HandSide ReportedLabel(HandSide realHand, bool mirrored)
        {
            if (!mirrored)
            {
                return realHand;
            }

            return realHand == HandSide.Right ? HandSide.Left : HandSide.Right;
        }

        private static void AssertVector(Vec3 actual, Vec3 expected)
        {
            Assert.That(actual.X, Is.EqualTo(expected.X).Within(Tolerance), "x");
            Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(Tolerance), "y");
            Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(Tolerance), "z");
        }
    }
}
