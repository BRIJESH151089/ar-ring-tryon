using System;

namespace ArRingTryOn.Core
{
    /// <summary>
    /// One detected hand, in display space: x runs right and y runs down across the upright
    /// camera image, both in [0, 1]. z is relative depth in display widths, smaller is closer
    /// to the camera, with the wrist near zero.
    /// </summary>
    public sealed class HandLandmarks
    {
        public const int Count = 21;

        private readonly Vec3[] _points;

        /// <param name="points">Indexed by <see cref="HandLandmarkId"/>. Ownership passes to this instance.</param>
        /// <param name="side">Which hand this is.</param>
        public HandLandmarks(Vec3[] points, HandSide side)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            if (points.Length != Count)
            {
                throw new ArgumentException($"Expected {Count} landmarks, got {points.Length}.", nameof(points));
            }

            _points = points;
            Side = side;
        }

        public HandSide Side { get; }

        public Vec3 this[HandLandmarkId id] => _points[(int)id];
    }
}
