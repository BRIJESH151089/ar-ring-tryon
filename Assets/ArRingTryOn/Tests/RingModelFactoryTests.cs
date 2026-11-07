using System;
using System.Linq;
using ArRingTryOn.Core;
using ArRingTryOn.Rendering;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArRingTryOn.Tests
{
    public class RingModelFactoryTests
    {
        private const float Tolerance = 1e-3f;

        private static readonly RingStyle[] AllStyles = (RingStyle[])Enum.GetValues(typeof(RingStyle));

        [Test]
        public void PlainBand_IsTheTorusSizedForTheFinger()
        {
            WithMesh(RingStyle.PlainBand, mesh =>
            {
                var size = mesh.bounds.size;
                Assert.That(size.x, Is.EqualTo(2f * 0.62f).Within(Tolerance));
                Assert.That(size.y, Is.EqualTo(0.12f).Within(Tolerance));
                Assert.That(size.z, Is.EqualTo(2f * 0.62f).Within(Tolerance));
                Assert.That(mesh.subMeshCount, Is.EqualTo(1));
            });
        }

        [TestCaseSource(nameof(AllStyles))]
        public void Hole_FitsTheFingerExactly(RingStyle style)
        {
            WithMesh(style, mesh =>
            {
                var nearest = mesh.vertices.Min(v => new Vector2(v.x, v.z).magnitude);
                Assert.That(nearest, Is.EqualTo(RingModelFactory.InnerRadius).Within(Tolerance));
            });
        }

        [TestCaseSource(nameof(AllStyles))]
        public void Normals_AreUnitLength(RingStyle style)
        {
            WithMesh(style, mesh =>
            {
                foreach (var normal in mesh.normals)
                {
                    Assert.That(normal.magnitude, Is.EqualTo(1f).Within(Tolerance));
                }
            });
        }

        [TestCaseSource(nameof(AllStyles))]
        public void Triangles_FaceTheSameWayAsTheirNormals(RingStyle style)
        {
            // A reversed winding would be culled, leaving that part invisible from outside.
            WithMesh(style, mesh =>
            {
                var vertices = mesh.vertices;
                var normals = mesh.normals;
                for (var submesh = 0; submesh < mesh.subMeshCount; submesh++)
                {
                    var triangles = mesh.GetTriangles(submesh);
                    for (var i = 0; i < triangles.Length; i += 3)
                    {
                        var a = triangles[i];
                        var b = triangles[i + 1];
                        var c = triangles[i + 2];
                        var face = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                        Assert.That(Vector3.Dot(face, normals[a] + normals[b] + normals[c]), Is.GreaterThan(0f),
                            $"submesh {submesh} triangle {i / 3}");
                    }
                }
            });
        }

        [TestCaseSource(nameof(AllStyles))]
        public void Ring_HasNoDegenerateTriangles(RingStyle style)
        {
            WithMesh(style, mesh =>
            {
                var vertices = mesh.vertices;
                var triangles = mesh.triangles;
                for (var i = 0; i < triangles.Length; i += 3)
                {
                    var area = Vector3.Cross(vertices[triangles[i + 1]] - vertices[triangles[i]],
                        vertices[triangles[i + 2]] - vertices[triangles[i]]).magnitude;
                    Assert.That(area, Is.GreaterThan(1e-8f), $"triangle {i / 3}");
                }
            });
        }

        [Test]
        public void Signet_RaisesAFlatFaceOnTop()
        {
            WithMesh(RingStyle.Signet, mesh =>
            {
                var top = mesh.bounds.max.z;
                var flatTop = mesh.vertices.Where((v, i) => v.z > top - Tolerance && mesh.normals[i].z > 0.999f).ToArray();

                Assert.That(top, Is.GreaterThan(0.7f));
                Assert.That(flatTop.Max(v => v.x) - flatTop.Min(v => v.x), Is.GreaterThan(0.4f));
                Assert.That(mesh.bounds.min.z, Is.EqualTo(-0.61f).Within(Tolerance), "band underneath stays slim");
            });
        }

        [Test]
        public void Gemstone_PutsAFacetedGemOnTop()
        {
            WithMesh(RingStyle.Gemstone, mesh =>
            {
                Assert.That(mesh.subMeshCount, Is.EqualTo(2));
                var vertices = mesh.vertices;
                var gem = mesh.GetTriangles(RingModelFactory.GemSubmesh).Select(i => vertices[i]).ToArray();

                Assert.That(gem.Length, Is.GreaterThan(0));
                Assert.That(gem.Min(v => v.z), Is.GreaterThan(RingModelFactory.InnerRadius));
                Assert.That(gem.Max(v => v.z), Is.EqualTo(0.88f).Within(Tolerance));
            });
        }

        [Test]
        public void Gemstone_FacetsAreFlatShaded()
        {
            WithMesh(RingStyle.Gemstone, mesh =>
            {
                var normals = mesh.normals;
                var gem = mesh.GetTriangles(RingModelFactory.GemSubmesh);
                for (var i = 0; i < gem.Length; i += 3)
                {
                    Assert.That(Vector3.Angle(normals[gem[i]], normals[gem[i + 1]]), Is.LessThan(0.01f));
                    Assert.That(Vector3.Angle(normals[gem[i]], normals[gem[i + 2]]), Is.LessThan(0.01f));
                }
            });
        }

        [Test]
        public void Gemstone_ProngsReachAboveTheGirdle()
        {
            WithMesh(RingStyle.Gemstone, mesh =>
            {
                var vertices = mesh.vertices;
                var metalTop = mesh.GetTriangles(RingModelFactory.MetalSubmesh).Max(i => vertices[i].z);

                Assert.That(metalTop, Is.GreaterThan(0.8f));
            });
        }

        [Test]
        public void Styles_AreDistinctShapes()
        {
            var heights = AllStyles.Select(style =>
            {
                var mesh = RingModelFactory.Build(style);
                var top = mesh.bounds.max.z;
                Object.DestroyImmediate(mesh);
                return top;
            }).ToArray();

            Assert.That(heights.Distinct().Count(), Is.EqualTo(AllStyles.Length));
        }

        [Test]
        public void UnknownStyle_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RingModelFactory.Build((RingStyle)99));
        }

        private static void WithMesh(RingStyle style, Action<Mesh> check)
        {
            var mesh = RingModelFactory.Build(style);
            try
            {
                check(mesh);
            }
            finally
            {
                Object.DestroyImmediate(mesh);
            }
        }
    }
}
