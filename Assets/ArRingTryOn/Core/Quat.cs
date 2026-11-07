using System;

namespace ArRingTryOn.Core
{
    /// <summary>Engine-free unit quaternion, using the same component convention as Unity's.</summary>
    public readonly struct Quat
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly float W;

        public Quat(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>The rotation taking the unit axes onto the given orthonormal basis, where z = x cross y.</summary>
        public static Quat FromBasis(Vec3 x, Vec3 y, Vec3 z)
        {
            var trace = x.X + y.Y + z.Z;

            if (trace > 0f)
            {
                var s = (float)Math.Sqrt(trace + 1f) * 2f;
                return new Quat((y.Z - z.Y) / s, (z.X - x.Z) / s, (x.Y - y.X) / s, 0.25f * s);
            }

            if (x.X > y.Y && x.X > z.Z)
            {
                var s = (float)Math.Sqrt(1f + x.X - y.Y - z.Z) * 2f;
                return new Quat(0.25f * s, (y.X + x.Y) / s, (z.X + x.Z) / s, (y.Z - z.Y) / s);
            }

            if (y.Y > z.Z)
            {
                var s = (float)Math.Sqrt(1f + y.Y - x.X - z.Z) * 2f;
                return new Quat((y.X + x.Y) / s, 0.25f * s, (z.Y + y.Z) / s, (z.X - x.Z) / s);
            }

            var t = (float)Math.Sqrt(1f + z.Z - x.X - y.Y) * 2f;
            return new Quat((z.X + x.Z) / t, (z.Y + y.Z) / t, 0.25f * t, (x.Y - y.X) / t);
        }

        public Vec3 Rotate(Vec3 v)
        {
            var axis = new Vec3(X, Y, Z);
            var t = Vec3.Cross(axis, v) * 2f;
            return v + t * W + Vec3.Cross(axis, t);
        }
    }
}
