using System;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class RingPoseCalculatorTests
    {
        private const float Tolerance = 1e-4f;
        private const float WidthPerSpacing = RingPoseCalculator.FingerWidthPerKnuckleSpacing;
        private const float Along = RingPoseCalculator.DefaultPositionAlongSegment;

        private static readonly Vec3 LocalX = new Vec3(1f, 0f, 0f);
        private static readonly Vec3 RingAxis = new Vec3(0f, 1f, 0f);
        private static readonly Vec3 RingTop = new Vec3(0f, 0f, 1f);
        private static readonly Vec3 TowardsCamera = new Vec3(0f, 0f, -1f);

        // A right hand held upright with its back to the camera, on a square display.
        private static Vec3[] UprightRightHand()
        {
            var points = new Vec3[HandLandmarks.Count];
            points[(int)HandLandmarkId.Wrist] = new Vec3(0.5f, 0.8f, 0f);
            points[(int)HandLandmarkId.IndexMcp] = new Vec3(0.35f, 0.5f, 0f);
            points[(int)HandLandmarkId.MiddleMcp] = new Vec3(0.45f, 0.5f, 0f);
            points[(int)HandLandmarkId.RingMcp] = new Vec3(0.55f, 0.5f, 0f);
            points[(int)HandLandmarkId.PinkyMcp] = new Vec3(0.65f, 0.5f, 0f);
            points[(int)HandLandmarkId.RingPip] = new Vec3(0.55f, 0.4f, 0f);
            return points;
        }

        [Test]
        public void TargetsTheRingFingerJoints()
        {
            Assert.That((int)HandLandmarkId.RingMcp, Is.EqualTo(13));
            Assert.That((int)HandLandmarkId.RingPip, Is.EqualTo(14));
            Assert.That((int)HandLandmarkId.MiddleMcp, Is.EqualTo(9));
            Assert.That((int)HandLandmarkId.PinkyMcp, Is.EqualTo(17));
        }

        [Test]
        public void DefaultPosition_IsTheValueTunedOnDevice()
        {
            Assert.That(Along, Is.EqualTo(0.7f));
        }

        [Test]
        public void StraightFinger_SitsPartWayUpTheProximalPhalanx()
        {
            var pose = Calculate(UprightRightHand());

            // Part way from y=0.5 to y=0.4, then measured from the centre with y up.
            AssertVector(pose.Position, new Vec3(0.05f, 0.1f * Along, 0f));
        }

        [TestCase(0f)]
        [TestCase(0.35f)]
        [TestCase(0.475f)]
        [TestCase(0.7f)]
        [TestCase(1f)]
        public void Position_CanBeTunedAlongTheBone(float along)
        {
            var points = UprightRightHand();
            var hand = new HandLandmarks(points, HandSide.Right);

            Assert.That(RingPoseCalculator.TryCalculate(hand, 1f, false, along, out var pose), Is.True);

            AssertVector(pose.Position, new Vec3(0.05f, 0.1f * along, 0f));
        }

        [Test]
        public void TuningPosition_LeavesRollAndScaleAlone()
        {
            var hand = new HandLandmarks(UprightRightHand(), HandSide.Right);
            RingPoseCalculator.TryCalculate(hand, 1f, false, 0.2f, out var low);
            RingPoseCalculator.TryCalculate(hand, 1f, false, 0.8f, out var high);

            Assert.That(high.Scale, Is.EqualTo(low.Scale));
            AssertVector(high.Rotation.Rotate(RingAxis), low.Rotation.Rotate(RingAxis));
            AssertVector(high.Rotation.Rotate(RingTop), low.Rotation.Rotate(RingTop));
        }

        [TestCase(-0.01f)]
        [TestCase(1.01f)]
        [TestCase(float.NaN)]
        public void Position_OutsideTheBoneIsRejected(float along)
        {
            var hand = new HandLandmarks(UprightRightHand(), HandSide.Right);

            Assert.Throws<ArgumentOutOfRangeException>(() => RingPoseCalculator.TryCalculate(hand, 1f, false, along, out _));
        }

        [Test]
        public void StraightFinger_AlignsRingAxisWithFinger()
        {
            var pose = Calculate(UprightRightHand());

            AssertVector(pose.Rotation.Rotate(RingAxis), RingAxis);
        }

        [Test]
        public void StraightFinger_ScalesToFingerThickness()
        {
            Assert.That(Calculate(UprightRightHand()).Scale, Is.EqualTo(0.1f * WidthPerSpacing).Within(Tolerance));
        }

        [TestCase(30f)]
        [TestCase(90f)]
        [TestCase(180f)]
        [TestCase(-120f)]
        public void RotatedHand_RotatesAxisAndPositionButNotScale(float degrees)
        {
            var upright = Calculate(UprightRightHand());

            var rotated = Calculate(Transform(UprightRightHand(), p => RotateInPlane(p, degrees)));

            // Display y runs down, so a positive angle here turns the hand clockwise on screen.
            AssertVector(rotated.Rotation.Rotate(RingAxis), RotateView(RingAxis, -degrees));
            AssertVector(rotated.Position, RotateView(upright.Position, -degrees));
            Assert.That(rotated.Scale, Is.EqualTo(upright.Scale).Within(Tolerance));
        }

        [Test]
        public void FingerPointingAtCamera_TiltsRingAxisInDepth()
        {
            var points = UprightRightHand();
            points[(int)HandLandmarkId.RingPip] = new Vec3(0.55f, 0.5f, -0.1f);

            var pose = Calculate(points);

            AssertVector(pose.Rotation.Rotate(RingAxis), TowardsCamera);
            Assert.That(pose.Position.Z, Is.EqualTo(-0.1f * Along).Within(Tolerance));
        }

        [TestCase(0.5f)]
        [TestCase(2f)]
        public void HandAtDifferentDistance_ScalesWithIt(float apparentSize)
        {
            var near = Calculate(UprightRightHand());

            var scaled = Calculate(Transform(UprightRightHand(), p => ScaleAboutCentre(p, apparentSize)));

            Assert.That(scaled.Scale, Is.EqualTo(near.Scale * apparentSize).Within(Tolerance));
            AssertVector(scaled.Position, near.Position * apparentSize);
            AssertVector(scaled.Rotation.Rotate(RingAxis), RingAxis);
        }

        [Test]
        public void Scale_AveragesBothNeighbouringKnuckles()
        {
            var points = UprightRightHand();
            points[(int)HandLandmarkId.PinkyMcp] = new Vec3(0.75f, 0.5f, 0f);

            Assert.That(Calculate(points).Scale, Is.EqualTo(0.15f * WidthPerSpacing).Within(Tolerance));
        }

        [Test]
        public void Scale_CountsKnuckleDepthSoATiltedHandKeepsItsSize()
        {
            var points = UprightRightHand();
            points[(int)HandLandmarkId.MiddleMcp] = new Vec3(0.49f, 0.5f, 0.08f);
            points[(int)HandLandmarkId.PinkyMcp] = new Vec3(0.61f, 0.5f, -0.08f);

            Assert.That(Calculate(points).Scale, Is.EqualTo(0.1f * WidthPerSpacing).Within(Tolerance));
        }

        [Test]
        public void NonSquareDisplay_KeepsFingerDirectionAndSizeTrue()
        {
            // The upright hand squeezed onto a 9:16 display: the same hand, in different units.
            const float aspect = 9f / 16f;
            var points = Transform(UprightRightHand(), p => new Vec3(p.X, 0.5f + (p.Y - 0.5f) * aspect, 0f));

            var pose = Calculate(points, aspect);

            AssertVector(pose.Rotation.Rotate(RingAxis), RingAxis);
            AssertVector(pose.Position, new Vec3(0.05f, 0.1f * Along, 0f));
            Assert.That(pose.Scale, Is.EqualTo(0.1f * WidthPerSpacing).Within(Tolerance));
        }

        [Test]
        public void RightHand_BackToCamera_PutsRingTopTowardsCamera()
        {
            var pose = Calculate(UprightRightHand());

            AssertVector(pose.Rotation.Rotate(RingTop), TowardsCamera);
        }

        [Test]
        public void LeftHand_BackToCamera_PutsRingTopTowardsCamera()
        {
            var pose = Calculate(MirroredAcrossDisplay(UprightRightHand()), side: HandSide.Left);

            AssertVector(pose.Rotation.Rotate(RingTop), TowardsCamera);
            AssertVector(pose.Rotation.Rotate(RingAxis), RingAxis);
        }

        [Test]
        public void LeftHand_MirrorsPositionAndKeepsScale()
        {
            var right = Calculate(UprightRightHand());

            var left = Calculate(MirroredAcrossDisplay(UprightRightHand()), side: HandSide.Left);

            AssertVector(left.Position, new Vec3(-right.Position.X, right.Position.Y, right.Position.Z));
            Assert.That(left.Scale, Is.EqualTo(right.Scale).Within(Tolerance));
        }

        [Test]
        public void SameShape_WithPalmToCamera_PutsRingTopAwayFromCamera()
        {
            // This shape labelled Left means a real left hand showing its palm.
            var pose = Calculate(UprightRightHand(), side: HandSide.Left);

            AssertVector(pose.Rotation.Rotate(RingTop), TowardsCamera * -1f);
        }

        [Test]
        public void MirroredImage_OfRightHand_StillPutsRingTopTowardsCamera()
        {
            var pose = Calculate(MirroredAcrossDisplay(UprightRightHand()), isMirrored: true);

            AssertVector(pose.Rotation.Rotate(RingTop), TowardsCamera);
        }

        [Test]
        public void MirroredImage_OfLeftHand_StillPutsRingTopTowardsCamera()
        {
            var pose = Calculate(UprightRightHand(), side: HandSide.Left, isMirrored: true);

            AssertVector(pose.Rotation.Rotate(RingTop), TowardsCamera);
        }

        [Test]
        public void UnknownSide_IsTreatedAsRight()
        {
            var pose = Calculate(UprightRightHand(), side: HandSide.Unknown);

            AssertVector(pose.Rotation.Rotate(RingTop), TowardsCamera);
        }

        [Test]
        public void Rotation_IsAlwaysAProperOrthonormalFrame()
        {
            var points = Transform(UprightRightHand(), p => RotateInPlane(p, 37f));
            points[(int)HandLandmarkId.RingPip] = new Vec3(0.6f, 0.42f, -0.05f);

            var rotation = Calculate(points).Rotation;

            var x = rotation.Rotate(LocalX);
            var y = rotation.Rotate(RingAxis);
            var z = rotation.Rotate(RingTop);
            Assert.That(x.Length, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(y.Length, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(z.Length, Is.EqualTo(1f).Within(Tolerance));
            AssertVector(Vec3.Cross(x, y), z);
        }

        [Test]
        public void KnucklesInLineWithFinger_StillGivesAValidRotation()
        {
            // A knuckle line parallel to the finger gives no roll, so it has to fall back.
            var points = UprightRightHand();
            points[(int)HandLandmarkId.MiddleMcp] = new Vec3(0.55f, 0.6f, 0f);
            points[(int)HandLandmarkId.PinkyMcp] = new Vec3(0.55f, 0.3f, 0f);

            var pose = Calculate(points);

            AssertVector(pose.Rotation.Rotate(RingAxis), RingAxis);
            Assert.That(pose.Rotation.Rotate(RingTop).Length, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void FingerBentTowardsCamera_KeepsHeadOnBackOfFinger()
        {
            // Bending the finger about the knuckle line turns its back from the camera to below it.
            var points = UprightRightHand();
            points[(int)HandLandmarkId.RingPip] = new Vec3(0.55f, 0.5f, -0.1f);

            var pose = Calculate(points);

            AssertVector(pose.Rotation.Rotate(RingAxis), TowardsCamera);
            AssertVector(pose.Rotation.Rotate(RingTop), new Vec3(0f, -1f, 0f));
        }

        [Test]
        public void FingerCurledForward_HeadStaysOnBackOfFinger()
        {
            // A finger curled 60 degrees towards the palm, back of hand to camera.
            var points = UprightRightHand();
            var c = (float)Math.Cos(Math.PI / 3.0);
            var s = (float)Math.Sin(Math.PI / 3.0);
            points[(int)HandLandmarkId.RingPip] = new Vec3(0.55f, 0.5f - 0.1f * c, 0.1f * s);

            var top = Calculate(points).Rotation.Rotate(RingTop);

            AssertVector(top, new Vec3(0f, s, -c));
        }

        [Test]
        public void CoincidentFingerJoints_AreRejected()
        {
            var points = UprightRightHand();
            points[(int)HandLandmarkId.RingPip] = points[(int)HandLandmarkId.RingMcp];

            Assert.That(RingPoseCalculator.TryCalculate(new HandLandmarks(points, HandSide.Right), 1f, false, out _), Is.False);
        }

        [Test]
        public void CoincidentKnuckles_AreRejected()
        {
            var points = UprightRightHand();
            points[(int)HandLandmarkId.MiddleMcp] = points[(int)HandLandmarkId.RingMcp];
            points[(int)HandLandmarkId.PinkyMcp] = points[(int)HandLandmarkId.RingMcp];

            Assert.That(RingPoseCalculator.TryCalculate(new HandLandmarks(points, HandSide.Right), 1f, false, out _), Is.False);
        }

        private static RingPose Calculate(Vec3[] points, float aspect = 1f, HandSide side = HandSide.Right, bool isMirrored = false)
        {
            var found = RingPoseCalculator.TryCalculate(new HandLandmarks(points, side), aspect, isMirrored, out var pose);
            Assert.That(found, Is.True);
            return pose;
        }

        private static Vec3[] MirroredAcrossDisplay(Vec3[] points)
        {
            return Transform(points, p => new Vec3(1f - p.X, p.Y, p.Z));
        }

        private static Vec3[] Transform(Vec3[] points, Func<Vec3, Vec3> transform)
        {
            var result = new Vec3[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                result[i] = transform(points[i]);
            }

            return result;
        }

        private static Vec3 ScaleAboutCentre(Vec3 p, float scale)
        {
            return new Vec3(0.5f + (p.X - 0.5f) * scale, 0.5f + (p.Y - 0.5f) * scale, p.Z * scale);
        }

        private static Vec3 RotateInPlane(Vec3 p, float degrees)
        {
            var rotated = RotateView(new Vec3(p.X - 0.5f, p.Y - 0.5f, p.Z), degrees);
            return new Vec3(rotated.X + 0.5f, rotated.Y + 0.5f, rotated.Z);
        }

        private static Vec3 RotateView(Vec3 v, float degrees)
        {
            var radians = degrees * Math.PI / 180.0;
            var c = (float)Math.Cos(radians);
            var s = (float)Math.Sin(radians);
            return new Vec3(c * v.X - s * v.Y, s * v.X + c * v.Y, v.Z);
        }

        private static void AssertVector(Vec3 actual, Vec3 expected)
        {
            Assert.That(actual.X, Is.EqualTo(expected.X).Within(Tolerance), "x");
            Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(Tolerance), "y");
            Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(Tolerance), "z");
        }
    }
}
