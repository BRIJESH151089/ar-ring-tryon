namespace ArRingTryOn.Core
{
    /// <summary>Where a ring sits, in <see cref="ViewSpace"/>.</summary>
    public readonly struct RingPose
    {
        public RingPose(Vec3 position, Quat rotation, float scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        /// <summary>Centre of the ring.</summary>
        public Vec3 Position { get; }

        /// <summary>
        /// Takes the ring's local axes into view space: local Y runs through the hole towards the
        /// fingertip, and local Z points out of the back of the hand.
        /// </summary>
        public Quat Rotation { get; }

        /// <summary>Estimated finger diameter, which is the ring's inner diameter, in display widths.</summary>
        public float Scale { get; }
    }
}
