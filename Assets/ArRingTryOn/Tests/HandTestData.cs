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
