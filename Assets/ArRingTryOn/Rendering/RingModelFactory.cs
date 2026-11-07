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

        private const int RingSegments = 64;
        private const int TubeSegments = 16;
        private const int HeadSegments = 40;

        public static Mesh Build(RingStyle style)
        {
            switch (style)
            {
                case RingStyle.PlainBand: return PlainBand();
                case RingStyle.Signet: return Signet();
                case RingStyle.Gemstone: return Gemstone();
                default: throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }

        private static Mesh PlainBand()
        {
            var builder = new RingMeshBuilder(1);
            builder.AddBand(MetalSubmesh, InnerRadius, _ => (0.06f, 0.06f), RingSegments, TubeSegments);
            return builder.ToMesh("Plain Band");
        }

        private static Mesh Signet()
        {
            var builder = new RingMeshBuilder(1);

            // The band widens and thickens towards the top to carry the face.
            builder.AddBand(MetalSubmesh, InnerRadius, top =>
            {
                var shoulder = Mathf.SmoothStep(0f, 1f, (top - 0.2f) / 0.8f);
                return (Mathf.Lerp(0.07f, 0.3f, shoulder), Mathf.Lerp(0.055f, 0.085f, shoulder));
            }, RingSegments, TubeSegments);

            // An oval face, longer along the finger, with a bevelled rim and a shallow recessed panel.
            builder.AddLathe(MetalSubmesh, new Vector2(0.28f, 0.34f), new[]
            {
                (1f, 0.58f),
                (1f, 0.7f),
                (0.94f, 0.74f),
                (0.84f, 0.74f),
                (0.8f, 0.725f),
                (0f, 0.725f),
            }, HeadSegments);

            return builder.ToMesh("Signet");
        }

        private static Mesh Gemstone()
        {
            var builder = new RingMeshBuilder(2);

            builder.AddBand(MetalSubmesh, InnerRadius, top =>
            {
                var shoulder = Mathf.SmoothStep(0f, 1f, (top - 0.4f) / 0.6f);
                return (Mathf.Lerp(0.055f, 0.08f, shoulder), Mathf.Lerp(0.05f, 0.06f, shoulder));
            }, RingSegments, TubeSegments);

            // An eight-sided brilliant: pointed pavilion below the girdle, flat table on top.
            builder.AddFaceted(GemSubmesh, 8, new[]
            {
                (0f, 0.6f),
                (0.22f, 0.78f),
                (0.22f, 0.8f),
                (0.13f, 0.88f),
                (0f, 0.88f),
            }, Mathf.PI / 8f);

            // Four claws rising from the band and leaning in over the crown.
            for (var i = 0; i < 4; i++)
            {
                var angle = Mathf.PI / 4f + i * Mathf.PI / 2f;
                var around = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                var from = around * 0.09f + Vector3.forward * 0.58f;
                var to = around * 0.18f + Vector3.forward * 0.86f;
                builder.AddProng(MetalSubmesh, from, to, 0.028f, 0.022f, 8);
            }

            return builder.ToMesh("Gemstone");
        }
    }
}
