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

        public static Quat Identity => new Quat(0f, 0f, 0f, 1f);

        public static float Dot(Quat a, Quat b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;

        /// <summary>The angle in radians of the rotation taking <paramref name="a"/> to <paramref name="b"/>.</summary>
        public static float Angle(Quat a, Quat b)
        {
            var dot = Math.Min(Math.Abs(Dot(a, b)), 1f);
            return 2f * (float)Math.Acos(dot);
        }

        /// <summary>Spherical interpolation along the shorter arc.</summary>
        public static Quat Slerp(Quat a, Quat b, float t)
        {
            var dot = Dot(a, b);
            if (dot < 0f)
            {
                b = new Quat(-b.X, -b.Y, -b.Z, -b.W);
                dot = -dot;
            }

            float wa, wb;
            if (dot > 0.9995f)
            {
                // Nearly equal: linear blending is accurate and avoids dividing by sin(0).
                wa = 1f - t;
                wb = t;
            }
            else
            {
                var theta = Math.Acos(dot);
                var sin = Math.Sin(theta);
                wa = (float)(Math.Sin((1f - t) * theta) / sin);
                wb = (float)(Math.Sin(t * theta) / sin);
            }

            return new Quat(wa * a.X + wb * b.X, wa * a.Y + wb * b.Y, wa * a.Z + wb * b.Z, wa * a.W + wb * b.W).Normalized;
        }

        public Quat Normalized
        {
            get
            {
                var length = (float)Math.Sqrt(Dot(this, this));
                return new Quat(X / length, Y / length, Z / length, W / length);
            }
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
