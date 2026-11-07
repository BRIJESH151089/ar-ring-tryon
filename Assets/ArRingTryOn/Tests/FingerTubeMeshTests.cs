using ArRingTryOn.Rendering;
using NUnit.Framework;
using UnityEngine;

namespace ArRingTryOn.Tests
{
    public class FingerTubeMeshTests
    {
        private FingerTubeMesh _tube;

        [SetUp]
        public void SetUp()
        {
            // A finger that bends: up, then leaning right.
            _tube = new FingerTubeMesh(4, 16);
            _tube.SetCentre(0, new Vector3(0f, 0f, 0f), 0.5f);
            _tube.SetCentre(1, new Vector3(0f, 1f, 0f), 0.45f);
            _tube.SetCentre(2, new Vector3(0.5f, 1.8f, 0.2f), 0.4f);
            _tube.SetCentre(3, new Vector3(1f, 2.4f, 0.3f), 0.35f);
            _tube.Apply();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_tube.Mesh);
        }

        [Test]
        public void EveryVertexSitsAtItsCentresRadius()
        {
            var vertices = _tube.Mesh.vertices;
            var centres = new[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0.5f, 1.8f, 0.2f), new Vector3(1f, 2.4f, 0.3f) };
            var radii = new[] { 0.5f, 0.45f, 0.4f, 0.35f };

            for (var c = 0; c < 4; c++)
            {
                for (var s = 0; s < 16; s++)
                {
                    Assert.That((vertices[c * 16 + s] - centres[c]).magnitude, Is.EqualTo(radii[c]).Within(1e-4f));
                }
            }
        }

        [Test]
        public void TrianglesFaceOutwards()
        {
            // Back faces are culled, so an inward-facing tube would hide nothing.
            var vertices = _tube.Mesh.vertices;
            var triangles = _tube.Mesh.triangles;
            var centre = _tube.Mesh.bounds.center;

            for (var i = 0; i < triangles.Length; i += 3)
            {
                var a = vertices[triangles[i]];
                var b = vertices[triangles[i + 1]];
                var c = vertices[triangles[i + 2]];
                var face = Vector3.Cross(b - a, c - a);
                var middle = (a + b + c) / 3f;
                var nearestAxisPoint = NearestOnPolyline(middle);
                Assert.That(Vector3.Dot(face, middle - nearestAxisPoint), Is.GreaterThan(0f), $"triangle {i / 3}");
            }
        }

        [Test]
        public void IsClosedAtBothEnds()
        {
            var bounds = _tube.Mesh.bounds;

            Assert.That(bounds.min.y, Is.LessThan(-0.45f), "base cap reaches below the first centre");
            Assert.That(bounds.max.y, Is.GreaterThan(2.6f), "tip cap reaches past the last centre");
        }

        [Test]
        public void ReusesTheSameMeshWhenUpdated()
        {
            var mesh = _tube.Mesh;
            _tube.SetCentre(3, new Vector3(0f, 3f, 0f), 0.3f);
            _tube.Apply();

            Assert.That(_tube.Mesh, Is.SameAs(mesh));
            Assert.That(_tube.Mesh.bounds.max.y, Is.GreaterThan(3f));
        }

        private static Vector3 NearestOnPolyline(Vector3 point)
        {
            var path = new[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0.5f, 1.8f, 0.2f), new Vector3(1f, 2.4f, 0.3f) };
            var best = path[0];
            for (var i = 0; i < path.Length - 1; i++)
            {
                var along = path[i + 1] - path[i];
                var t = Mathf.Clamp01(Vector3.Dot(point - path[i], along) / along.sqrMagnitude);
                var candidate = path[i] + along * t;
                if ((candidate - point).sqrMagnitude < (best - point).sqrMagnitude)
                {
                    best = candidate;
                }
            }

            return best;
        }
    }
}
