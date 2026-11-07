using System;

namespace ArRingTryOn.Core
{
    /// <summary>Engine-free 3D vector, so Core stays independent of Unity and MediaPipe.</summary>
    public readonly struct Vec3
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        /// <summary>This vector scaled to unit length. Must not be called on a zero vector.</summary>
        public Vec3 Normalized => this * (1f / Length);

        public static float Dot(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public static Vec3 Cross(Vec3 a, Vec3 b)
        {
            return new Vec3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vec3 operator *(Vec3 v, float s) => new Vec3(v.X * s, v.Y * s, v.Z * s);

        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}
