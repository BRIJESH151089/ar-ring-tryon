using System;
using ArRingTryOn.Core;
using UnityEngine;

namespace ArRingTryOn.Rendering
{
    /// <summary>
    /// Builds each ring style procedurally. Every ring has the same inner radius, with its hole
    /// around the Y axis and its top facing +Z, matching <see cref="RingPose"/>.
    /// </summary>
    public static class RingModelFactory
    {
        public const float InnerRadius = 0.5f;
        public const int MetalSubmesh = 0;
        public const int GemSubmesh = 1;

        /// <summary>A gemstone's band is a little finer than the shared band, so the stone stands out.</summary>
        public const float GemstoneBandScale = 0.8f;

        private const int RingSegments = 64;
        private const int TubeSegments = 16;
        private const int HeadSegments = 40;

        public static Mesh Build(RingStyle style) => Build(style, RingBandSettings.Default);

        public static Mesh Build(RingStyle style, RingBandSettings band)
        {
            band = band.Clamped;
            switch (style)
            {
                case RingStyle.PlainBand: return PlainBand(band);
                case RingStyle.Signet: return Signet(band);
                case RingStyle.Gemstone: return Gemstone(band);
                default: throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }

        /// <summary>Height of the band's outer surface on top of the ring.</summary>
        private static float BandTop(float topHalfThickness) => InnerRadius + 2f * topHalfThickness;

        private static Mesh PlainBand(RingBandSettings band)
        {
            var builder = new RingMeshBuilder(1);
            builder.AddBand(MetalSubmesh, InnerRadius, _ => (band.HalfWidth, band.HalfThickness), RingSegments, TubeSegments);
            return builder.ToMesh("Plain Band");
        }

        private static Mesh Signet(RingBandSettings band)
        {
            var builder = new RingMeshBuilder(1);

            // The band widens and thickens towards the top to carry the face.
            var topHalfWidth = Mathf.Max(0.3f, band.HalfWidth);
            var topHalfThickness = band.HalfThickness + 0.03f;
            builder.AddBand(MetalSubmesh, InnerRadius, top =>
            {
                var shoulder = Mathf.SmoothStep(0f, 1f, (top - 0.2f) / 0.8f);
                return (Mathf.Lerp(band.HalfWidth, topHalfWidth, shoulder), Mathf.Lerp(band.HalfThickness, topHalfThickness, shoulder));
            }, RingSegments, TubeSegments);

            // An oval face, longer along the finger, with a bevelled rim and a shallow recessed
            // panel. It sits partly sunk into the band's top so the two read as one piece.
            var faceBase = BandTop(topHalfThickness) - 0.09f;
            builder.AddLathe(MetalSubmesh, new Vector2(0.28f, 0.34f), new[]
            {
                (1f, faceBase),
                (1f, faceBase + 0.12f),
                (0.94f, faceBase + 0.16f),
                (0.84f, faceBase + 0.16f),
                (0.8f, faceBase + 0.145f),
                (0f, faceBase + 0.145f),
            }, HeadSegments);

            return builder.ToMesh("Signet");
        }

        private static Mesh Gemstone(RingBandSettings band)
        {
            var builder = new RingMeshBuilder(2);

            var halfWidth = band.HalfWidth * GemstoneBandScale;
            var halfThickness = band.HalfThickness * GemstoneBandScale;
            var topHalfWidth = halfWidth * 1.3f;
            var topHalfThickness = halfThickness * 1.2f;
            builder.AddBand(MetalSubmesh, InnerRadius, top =>
            {
                var shoulder = Mathf.SmoothStep(0f, 1f, (top - 0.4f) / 0.6f);
                return (Mathf.Lerp(halfWidth, topHalfWidth, shoulder), Mathf.Lerp(halfThickness, topHalfThickness, shoulder));
            }, RingSegments, TubeSegments);

            // An eight-sided brilliant: pointed pavilion just inside the band's top, girdle,
            // then a flat table.
            var seat = BandTop(topHalfThickness) - 0.02f;
            builder.AddFaceted(GemSubmesh, 8, new[]
            {
                (0f, seat),
                (0.22f, seat + 0.18f),
                (0.22f, seat + 0.2f),
                (0.13f, seat + 0.28f),
                (0f, seat + 0.28f),
            }, Mathf.PI / 8f);

            // Four claws rising from the band and leaning in over the crown.
            for (var i = 0; i < 4; i++)
            {
                var angle = Mathf.PI / 4f + i * Mathf.PI / 2f;
                var around = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                var from = around * 0.09f + Vector3.forward * (seat - 0.02f);
                var to = around * 0.18f + Vector3.forward * (seat + 0.26f);
                builder.AddProng(MetalSubmesh, from, to, 0.028f, 0.022f, 8);
            }

            return builder.ToMesh("Gemstone");
        }
    }
}
