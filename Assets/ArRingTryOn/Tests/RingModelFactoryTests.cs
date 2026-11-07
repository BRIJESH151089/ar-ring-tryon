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

        private static readonly RingBandSettings Band = RingBandSettings.Default;

        private static readonly RingBandSettings[] BandVariety =
        {
            new RingBandSettings(0.06f, 0.1f),
            RingBandSettings.Default,
            new RingBandSettings(0.3f, 0.45f),
        };

        [Test]
        public void PlainBand_HasTheConfiguredThicknessAndWidth()
        {
            WithMesh(RingStyle.PlainBand, mesh =>
            {
                var size = mesh.bounds.size;
                var outerRadius = RingModelFactory.InnerRadius + Band.Thickness;
                Assert.That(size.x, Is.EqualTo(2f * outerRadius).Within(Tolerance));
                Assert.That(size.y, Is.EqualTo(Band.Width).Within(Tolerance));
                Assert.That(size.z, Is.EqualTo(2f * outerRadius).Within(Tolerance));
                Assert.That(mesh.subMeshCount, Is.EqualTo(1));
            });
        }

        [Test]
        public void DefaultBand_IsASolidBandNotAWire()
        {
            // The first band was 0.12 thick and 0.12 wide, which read as a thin wire in photos.
            Assert.That(Band.Thickness, Is.GreaterThanOrEqualTo(0.15f));
            Assert.That(Band.Width, Is.GreaterThanOrEqualTo(0.2f));
        }

        [Test]
        public void Hole_FitsTheFingerExactlyForEveryStyleAndBand(
            [ValueSource(nameof(AllStyles))] RingStyle style, [ValueSource(nameof(BandVariety))] RingBandSettings band)
        {
            WithMesh(style, band, mesh =>
            {
                var nearest = mesh.vertices.Min(v => new Vector2(v.x, v.z).magnitude);
                Assert.That(nearest, Is.EqualTo(RingModelFactory.InnerRadius).Within(Tolerance));
            });
        }

        [Test]
        public void ThickerBand_GrowsOutwardsOnly()
        {
            var thin = RingModelFactory.Build(RingStyle.PlainBand, new RingBandSettings(0.08f, 0.12f));
            var thick = RingModelFactory.Build(RingStyle.PlainBand, new RingBandSettings(0.24f, 0.3f));
            try
            {
                Assert.That(thick.bounds.size.x - thin.bounds.size.x, Is.EqualTo(2f * 0.16f).Within(Tolerance));
                Assert.That(thick.bounds.size.y - thin.bounds.size.y, Is.EqualTo(0.18f).Within(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(thin);
                Object.DestroyImmediate(thick);
            }
        }

        [Test]
        public void OutOfRangeBand_IsClampedRatherThanBroken()
        {
            WithMesh(RingStyle.PlainBand, new RingBandSettings(-1f, 5f), mesh =>
            {
                Assert.That(mesh.bounds.size.y, Is.EqualTo(RingBandSettings.MaxWidth).Within(Tolerance));
                var nearest = mesh.vertices.Min(v => new Vector2(v.x, v.z).magnitude);
                Assert.That(nearest, Is.EqualTo(RingModelFactory.InnerRadius).Within(Tolerance));
            });
        }

        [Test]
        public void GemstoneAndSignetBands_AreNoThinnerThanTheirShare(
            [ValueSource(nameof(BandVariety))] RingBandSettings band)
        {
            // Underneath the finger each band shows its plain cross-section.
            foreach (var (style, scale) in new[] { (RingStyle.Signet, 1f), (RingStyle.Gemstone, RingModelFactory.GemstoneBandScale) })
            {
                WithMesh(style, band, mesh =>
                {
                    Assert.That(-mesh.bounds.min.z, Is.EqualTo(RingModelFactory.InnerRadius + band.Thickness * scale).Within(Tolerance), $"{style}");
                });
            }
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

                Assert.That(top, Is.GreaterThan(-mesh.bounds.min.z + 0.1f), "the face stands proud of the band");
                Assert.That(flatTop.Max(v => v.x) - flatTop.Min(v => v.x), Is.GreaterThan(0.4f));
            });
        }

        [Test]
        public void Heads_SitOnTopOfAThickBandInsteadOfSinkingIntoIt(
            [ValueSource(nameof(BandVariety))] RingBandSettings band)
        {
            WithMesh(RingStyle.Gemstone, band, mesh =>
            {
                var vertices = mesh.vertices;
                var gem = mesh.GetTriangles(RingModelFactory.GemSubmesh).Select(i => vertices[i]).ToArray();
                var bandTop = RingModelFactory.InnerRadius + band.Thickness * RingModelFactory.GemstoneBandScale * 1.2f;

                Assert.That(gem.Max(v => v.z) - bandTop, Is.GreaterThan(0.2f), "the stone stands above the band");
                Assert.That(gem.Min(v => v.z), Is.LessThan(bandTop), "its point is seated in the band");
            });

            WithMesh(RingStyle.Signet, band, mesh =>
            {
                var bandTop = RingModelFactory.InnerRadius + band.Thickness + 0.06f;
                Assert.That(mesh.bounds.max.z - bandTop, Is.EqualTo(0.07f).Within(Tolerance), "the face rises the same amount at any thickness");
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
                var gemTop = mesh.GetTriangles(RingModelFactory.GemSubmesh).Max(i => vertices[i].z);

                // The girdle is 0.08 below the table.
                Assert.That(metalTop, Is.GreaterThan(gemTop - 0.08f));
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

        private static void WithMesh(RingStyle style, Action<Mesh> check) => WithMesh(style, Band, check);

        private static void WithMesh(RingStyle style, RingBandSettings band, Action<Mesh> check)
        {
            var mesh = RingModelFactory.Build(style, band);
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
