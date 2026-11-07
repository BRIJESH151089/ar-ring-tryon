using ArRingTryOn.Rendering;
using NUnit.Framework;
using UnityEngine;

namespace ArRingTryOn.Tests
{
    public class TorusMeshTests
    {
        private const float MajorRadius = 0.56f;
        private const float TubeRadius = 0.06f;
        private const float Tolerance = 1e-3f;

        private Mesh _mesh;

        [SetUp]
        public void SetUp()
        {
            _mesh = TorusMesh.Build(MajorRadius, TubeRadius, 32, 12);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_mesh);
        }

        [Test]
        public void Size_MatchesRadii()
        {
            var size = _mesh.bounds.size;

            Assert.That(size.x, Is.EqualTo(2f * (MajorRadius + TubeRadius)).Within(Tolerance));
            Assert.That(size.y, Is.EqualTo(2f * TubeRadius).Within(Tolerance));
            Assert.That(size.z, Is.EqualTo(2f * (MajorRadius + TubeRadius)).Within(Tolerance));
        }

        [Test]
        public void Hole_MatchesInnerDiameter()
        {
            var nearest = float.MaxValue;
            foreach (var vertex in _mesh.vertices)
            {
                nearest = Mathf.Min(nearest, new Vector2(vertex.x, vertex.z).magnitude);
            }

            Assert.That(nearest, Is.EqualTo(MajorRadius - TubeRadius).Within(Tolerance));
        }

        [Test]
        public void TriangleCount_MatchesSegments()
        {
            Assert.That(_mesh.triangles.Length / 3, Is.EqualTo(32 * 12 * 2));
        }

        [Test]
        public void Normals_AreUnitLength()
        {
            foreach (var normal in _mesh.normals)
            {
                Assert.That(normal.magnitude, Is.EqualTo(1f).Within(Tolerance));
            }
        }

        [Test]
        public void Triangles_FaceOutwards()
        {
            // A reversed winding would be culled, leaving the ring invisible from outside.
            var vertices = _mesh.vertices;
            var normals = _mesh.normals;
            var triangles = _mesh.triangles;

            for (var i = 0; i < triangles.Length; i += 3)
            {
                var a = triangles[i];
                var b = triangles[i + 1];
                var c = triangles[i + 2];
                var face = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                var average = normals[a] + normals[b] + normals[c];

                Assert.That(Vector3.Dot(face, average), Is.GreaterThan(0f), $"triangle {i / 3}");
            }
        }
    }
}
