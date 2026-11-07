using System;
using UnityEngine;

namespace ArRingTryOn.Rendering
{
    /// <summary>
    /// A capped tube through a fixed number of centres, rebuilt in place each frame without
    /// allocating. Only positions are written: the tube is drawn depth-only, so it needs no normals.
    /// </summary>
    public sealed class FingerTubeMesh : IDisposable
    {
        private readonly int _centreCount;
        private readonly int _sides;
        private readonly Vector3[] _vertices;
        private readonly Vector3[] _centres;
        private readonly float[] _radii;

        public FingerTubeMesh(int centreCount, int sides)
        {
            _centreCount = centreCount;
            _sides = sides;
            _vertices = new Vector3[centreCount * sides + 2];
            _centres = new Vector3[centreCount];
            _radii = new float[centreCount];

            Mesh = new Mesh { name = "Finger Tube" };
            Mesh.MarkDynamic();
            Mesh.vertices = _vertices;
            Mesh.triangles = BuildTriangles();
        }

        public Mesh Mesh { get; }

        /// <summary>Copies a centre and its radius in; call <see cref="Apply"/> once all are set.</summary>
        public void SetCentre(int index, Vector3 centre, float radius)
        {
            _centres[index] = centre;
            _radii[index] = radius;
        }

        public void Apply()
        {
            // Carry one cross-section frame along the tube, turning it only as much as the tube
            // bends, so the rings stay lined up and the tube never twists.
            var tangent = Tangent(0);
            var u = Vector3.Cross(tangent, Mathf.Abs(tangent.y) < 0.9f ? Vector3.up : Vector3.right).normalized;

            for (var c = 0; c < _centreCount; c++)
            {
                tangent = Tangent(c);
                u = (u - tangent * Vector3.Dot(u, tangent)).normalized;
                var v = Vector3.Cross(tangent, u);

                for (var s = 0; s < _sides; s++)
                {
                    var angle = s * Mathf.PI * 2f / _sides;
                    _vertices[c * _sides + s] = _centres[c] + (u * Mathf.Cos(angle) + v * Mathf.Sin(angle)) * _radii[c];
                }
            }

            var last = _centreCount - 1;
            _vertices[_centreCount * _sides] = _centres[0] - Tangent(0) * _radii[0];
            _vertices[_centreCount * _sides + 1] = _centres[last] + Tangent(last) * _radii[last];

            Mesh.vertices = _vertices;
            Mesh.RecalculateBounds();
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(Mesh);
        }

        private Vector3 Tangent(int index)
        {
            var before = _centres[Mathf.Max(index - 1, 0)];
            var after = _centres[Mathf.Min(index + 1, _centreCount - 1)];
            var direction = after - before;
            return direction.sqrMagnitude > 1e-12f ? direction.normalized : Vector3.up;
        }

        private int[] BuildTriangles()
        {
            var triangles = new int[((_centreCount - 1) * 2 + 2) * _sides * 3];
            var next = 0;
            var baseApex = _centreCount * _sides;
            var tipApex = baseApex + 1;

            for (var s = 0; s < _sides; s++)
            {
                var s1 = (s + 1) % _sides;
                for (var c = 0; c < _centreCount - 1; c++)
                {
                    var a = c * _sides + s;
                    var b = c * _sides + s1;
                    var cc = (c + 1) * _sides + s;
                    var d = (c + 1) * _sides + s1;
                    Add(a, b, cc);
                    Add(b, d, cc);
                }

                Add(s1, s, baseApex);
                Add((_centreCount - 1) * _sides + s, (_centreCount - 1) * _sides + s1, tipApex);
            }

            return triangles;

            void Add(int i0, int i1, int i2)
            {
                triangles[next++] = i0;
                triangles[next++] = i1;
                triangles[next++] = i2;
            }
        }
    }
}
