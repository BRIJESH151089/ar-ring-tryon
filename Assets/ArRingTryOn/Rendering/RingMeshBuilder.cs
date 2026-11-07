using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArRingTryOn.Rendering
{
    /// <summary>Collects ring parts into one mesh, with one submesh per material.</summary>
    internal sealed class RingMeshBuilder
    {
        private const float NormalStep = 1e-3f;

        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<int>[] _submeshes;

        public RingMeshBuilder(int submeshCount)
        {
            _submeshes = new List<int>[submeshCount];
            for (var i = 0; i < submeshCount; i++)
            {
                _submeshes[i] = new List<int>();
            }
        }

        /// <summary>
        /// A band swept around the Y axis. Its cross-section is an ellipse whose size can change
        /// around the ring, while its inner edge stays on <paramref name="innerRadius"/>.
        /// </summary>
        /// <param name="profile">
        /// Half-width along Y and half-thickness outwards, given the cosine of the angle from +Z,
        /// which is 1 on top of the ring and -1 underneath.
        /// </param>
        public void AddBand(int submesh, float innerRadius, Func<float, (float HalfWidth, float HalfThickness)> profile,
            int ringSegments, int tubeSegments)
        {
            Vector3 Point(float ringAngle, float tubeAngle)
            {
                var (halfWidth, halfThickness) = profile(Mathf.Cos(ringAngle));
                var outward = new Vector3(Mathf.Sin(ringAngle), 0f, Mathf.Cos(ringAngle));
                var radius = innerRadius + halfThickness * (1f + Mathf.Cos(tubeAngle));
                return outward * radius + Vector3.up * (halfWidth * Mathf.Sin(tubeAngle));
            }

            var columns = tubeSegments + 1;
            var first = _vertices.Count;
            for (var ring = 0; ring <= ringSegments; ring++)
            {
                var ringAngle = ring * Mathf.PI * 2f / ringSegments;
                for (var tube = 0; tube <= tubeSegments; tube++)
                {
                    var tubeAngle = tube * Mathf.PI * 2f / tubeSegments;
                    // Tangents are normalised before crossing: their raw product is too small for
                    // Vector3.normalized, which returns zero below 1e-5.
                    var alongRing = (Point(ringAngle + NormalStep, tubeAngle) - Point(ringAngle - NormalStep, tubeAngle)).normalized;
                    var aroundTube = (Point(ringAngle, tubeAngle + NormalStep) - Point(ringAngle, tubeAngle - NormalStep)).normalized;
                    AddVertex(Point(ringAngle, tubeAngle), Vector3.Cross(alongRing, aroundTube).normalized);
                }
            }

            for (var ring = 0; ring < ringSegments; ring++)
            {
                for (var tube = 0; tube < tubeSegments; tube++)
                {
                    var a = first + ring * columns + tube;
                    var b = a + columns;
                    AddQuad(submesh, a, b, a + 1, b + 1);
                }
            }
        }

        /// <summary>
        /// A smooth oval solid around the Z axis, built from a profile that runs from its outer
        /// edge inwards. Each profile step is its own hard-edged strip, so flats stay flat.
        /// </summary>
        /// <param name="radii">Oval semi-axes along X and Y.</param>
        /// <param name="profile">Scale of the oval and height at each step; a scale of 0 closes the shape.</param>
        public void AddLathe(int submesh, Vector2 radii, (float Scale, float Z)[] profile, int segments)
        {
            for (var step = 0; step < profile.Length - 1; step++)
            {
                var (s0, z0) = profile[step];
                var (s1, z1) = profile[step + 1];
                var first = _vertices.Count;

                for (var k = 0; k <= segments; k++)
                {
                    var angle = k * Mathf.PI * 2f / segments;
                    var edge = new Vector2(radii.x * Mathf.Cos(angle), radii.y * Mathf.Sin(angle));
                    var across = new Vector2(Mathf.Cos(angle) / radii.x, Mathf.Sin(angle) / radii.y).normalized;
                    var slope = new Vector2(z1 - z0, -(s1 - s0) * Vector2.Dot(edge, across)).normalized;
                    var normal = new Vector3(across.x * slope.x, across.y * slope.x, slope.y);

                    AddVertex(new Vector3(edge.x * s0, edge.y * s0, z0), normal);
                    AddVertex(new Vector3(edge.x * s1, edge.y * s1, z1), normal);
                }

                for (var k = 0; k < segments; k++)
                {
                    var a = first + 2 * k;
                    AddQuad(submesh, a, a + 2, a + 1, a + 3, s0 > 0f, s1 > 0f);
                }
            }
        }

        /// <summary>
        /// A faceted solid around the Z axis with flat-shaded faces, like a cut gem. The profile
        /// gives the polygon radius and height at each step; a radius of 0 is a point.
        /// </summary>
        public void AddFaceted(int submesh, int sides, (float Radius, float Z)[] profile, float rotation)
        {
            Vector3 Corner(float radius, float z, int side)
            {
                var angle = rotation + side * Mathf.PI * 2f / sides;
                return new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), z);
            }

            for (var step = 0; step < profile.Length - 1; step++)
            {
                var (r0, z0) = profile[step];
                var (r1, z1) = profile[step + 1];

                for (var side = 0; side < sides; side++)
                {
                    var p00 = Corner(r0, z0, side);
                    var p10 = Corner(r0, z0, side + 1);
                    var p01 = Corner(r1, z1, side);
                    var p11 = Corner(r1, z1, side + 1);

                    if (r0 <= 0f)
                    {
                        AddFlatTriangle(submesh, p00, p11, p01);
                    }
                    else if (r1 <= 0f)
                    {
                        AddFlatTriangle(submesh, p00, p10, p01);
                    }
                    else
                    {
                        var normal = Vector3.Cross(p10 - p00, p01 - p00).normalized;
                        var a = AddVertex(p00, normal);
                        var b = AddVertex(p10, normal);
                        var c = AddVertex(p01, normal);
                        var d = AddVertex(p11, normal);
                        AddQuad(submesh, a, b, c, d);
                    }
                }
            }
        }

        /// <summary>A tapered rod with a pointed end, such as a prong holding a gem.</summary>
        public void AddProng(int submesh, Vector3 from, Vector3 to, float baseRadius, float tipRadius, int sides)
        {
            var direction = (to - from).normalized;
            var reference = Mathf.Abs(direction.y) < 0.9f ? Vector3.up : Vector3.right;
            var u = Vector3.Cross(direction, reference).normalized;
            var v = Vector3.Cross(direction, u);
            var first = _vertices.Count;

            for (var k = 0; k <= sides; k++)
            {
                var angle = k * Mathf.PI * 2f / sides;
                var radial = u * Mathf.Cos(angle) + v * Mathf.Sin(angle);
                AddVertex(from + radial * baseRadius, radial);
                AddVertex(to + radial * tipRadius, radial);
            }

            var apex = AddVertex(to + direction * tipRadius, direction);
            for (var k = 0; k < sides; k++)
            {
                var a = first + 2 * k;
                AddQuad(submesh, a, a + 2, a + 1, a + 3);
                AddTriangle(submesh, a + 1, a + 3, apex);
            }
        }

        public Mesh ToMesh(string name)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(_vertices);
            mesh.SetNormals(_normals);
            mesh.subMeshCount = _submeshes.Length;
            for (var i = 0; i < _submeshes.Length; i++)
            {
                mesh.SetTriangles(_submeshes[i], i);
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        private int AddVertex(Vector3 position, Vector3 normal)
        {
            _vertices.Add(position);
            _normals.Add(normal);
            return _vertices.Count - 1;
        }

        private void AddFlatTriangle(int submesh, Vector3 a, Vector3 b, Vector3 c)
        {
            var normal = Vector3.Cross(b - a, c - a).normalized;
            AddTriangle(submesh, AddVertex(a, normal), AddVertex(b, normal), AddVertex(c, normal));
        }

        // Corners a and b run along the first edge, c and d along the second, so that the
        // front face points along (b - a) x (c - a).
        private void AddQuad(int submesh, int a, int b, int c, int d, bool firstEdgeOpen = true, bool secondEdgeOpen = true)
        {
            if (firstEdgeOpen)
            {
                AddTriangle(submesh, a, b, c);
            }

            if (secondEdgeOpen)
            {
                AddTriangle(submesh, b, d, c);
            }
        }

        private void AddTriangle(int submesh, int a, int b, int c)
        {
            var triangles = _submeshes[submesh];
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }
    }
}
