using ArRingTryOn.Core;

namespace ArRingTryOn.Tests
{
    internal static class HandTestData
    {
        /// <summary>A hand whose landmark N sits at (N, 2N, 3N) / 100, so every joint is distinct.</summary>
        public static HandLandmarks Distinct(HandSide side = HandSide.Right)
        {
            var points = new Vec3[HandLandmarks.Count];
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = new Vec3(i / 100f, 2f * i / 100f, 3f * i / 100f);
            }

            return new HandLandmarks(points, side);
        }

        /// <summary>
        /// The joints ring placement reads, for an upright flat hand on a square display, with the
        /// thumb on the left (a right hand seen from behind) or on the right.
        /// </summary>
        public static Vec3[] UprightHand(bool thumbOnLeft)
        {
            var points = new Vec3[HandLandmarks.Count];
            points[(int)HandLandmarkId.Wrist] = new Vec3(0.5f, 0.8f, 0f);
            points[(int)HandLandmarkId.IndexMcp] = new Vec3(0.35f, 0.5f, 0f);
            points[(int)HandLandmarkId.MiddleMcp] = new Vec3(0.45f, 0.5f, 0f);
            points[(int)HandLandmarkId.RingMcp] = new Vec3(0.55f, 0.5f, 0f);
            points[(int)HandLandmarkId.PinkyMcp] = new Vec3(0.65f, 0.5f, 0f);
            points[(int)HandLandmarkId.RingPip] = new Vec3(0.55f, 0.4f, 0f);

            if (!thumbOnLeft)
            {
                for (var i = 0; i < points.Length; i++)
                {
                    points[i] = new Vec3(1f - points[i].X, points[i].Y, points[i].Z);
                }
            }

            return points;
        }

        /// <summary>A hand with every landmark at the origin except those given.</summary>
        public static HandLandmarks With(params (HandLandmarkId Id, Vec3 Position)[] overrides)
        {
            var points = new Vec3[HandLandmarks.Count];
            foreach (var (id, position) in overrides)
            {
                points[(int)id] = position;
            }

            return new HandLandmarks(points, HandSide.Unknown);
        }
    }
}
