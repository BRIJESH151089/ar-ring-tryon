using System;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class QuatTests
    {
        private const float Tolerance = 1e-5f;

        private static readonly Vec3 UnitX = new Vec3(1f, 0f, 0f);
        private static readonly Vec3 UnitY = new Vec3(0f, 1f, 0f);
        private static readonly Vec3 UnitZ = new Vec3(0f, 0f, 1f);

        [Test]
        public void FromBasis_OfUnitAxes_IsIdentity()
        {
            var q = Quat.FromBasis(UnitX, UnitY, UnitZ);

            Assert.That(q.W, Is.EqualTo(1f).Within(Tolerance));
            AssertVector(q.Rotate(new Vec3(1f, 2f, 3f)), new Vec3(1f, 2f, 3f));
        }

        // Each case reaches a different branch of the matrix conversion.
        [TestCase(0f, 0f, 0f)]
        [TestCase(180f, 0f, 0f)]
        [TestCase(0f, 180f, 0f)]
        [TestCase(0f, 0f, 180f)]
        [TestCase(170f, 20f, 0f)]
        [TestCase(30f, 165f, 10f)]
        [TestCase(15f, 40f, 170f)]
        [TestCase(120f, 130f, 140f)]
        public void FromBasis_TakesEachUnitAxisOntoItsBasisVector(float aboutX, float aboutY, float aboutZ)
        {
            var (x, y, z) = RotatedBasis(aboutX, aboutY, aboutZ);

            var q = Quat.FromBasis(x, y, z);

            AssertVector(q.Rotate(UnitX), x);
            AssertVector(q.Rotate(UnitY), y);
            AssertVector(q.Rotate(UnitZ), z);
        }

        [Test]
        public void Rotate_KeepsLength()
        {
            var (x, y, z) = RotatedBasis(25f, 50f, 75f);

            var rotated = Quat.FromBasis(x, y, z).Rotate(new Vec3(3f, 0f, 4f));

            Assert.That(rotated.Length, Is.EqualTo(5f).Within(Tolerance));
        }

        private static (Vec3 X, Vec3 Y, Vec3 Z) RotatedBasis(float aboutX, float aboutY, float aboutZ)
        {
            Vec3 Apply(Vec3 v) => AboutZ(AboutY(AboutX(v, aboutX), aboutY), aboutZ);
            return (Apply(UnitX), Apply(UnitY), Apply(UnitZ));
        }

        private static Vec3 AboutX(Vec3 v, float degrees)
        {
            var (c, s) = CosSin(degrees);
            return new Vec3(v.X, c * v.Y - s * v.Z, s * v.Y + c * v.Z);
        }

        private static Vec3 AboutY(Vec3 v, float degrees)
        {
            var (c, s) = CosSin(degrees);
            return new Vec3(c * v.X + s * v.Z, v.Y, c * v.Z - s * v.X);
        }

        private static Vec3 AboutZ(Vec3 v, float degrees)
        {
            var (c, s) = CosSin(degrees);
            return new Vec3(c * v.X - s * v.Y, s * v.X + c * v.Y, v.Z);
        }

        private static (float Cos, float Sin) CosSin(float degrees)
        {
            var radians = degrees * Math.PI / 180.0;
            return ((float)Math.Cos(radians), (float)Math.Sin(radians));
        }

        private static void AssertVector(Vec3 actual, Vec3 expected)
        {
            Assert.That(actual.X, Is.EqualTo(expected.X).Within(Tolerance), "x");
            Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(Tolerance), "y");
            Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(Tolerance), "z");
        }
    }
}
