namespace ArRingTryOn.Core
{
    /// <summary>Where a finger's ring segment is and which way it points.</summary>
    public readonly struct FingerPose
    {
        public FingerPose(Finger finger, Vec3 basePosition, Vec3 direction, float segmentLength)
        {
            Finger = finger;
            BasePosition = basePosition;
            Direction = direction;
            SegmentLength = segmentLength;
        }

        public Finger Finger { get; }

        /// <summary>The base joint, in display space.</summary>
        public Vec3 BasePosition { get; }

        /// <summary>Unit vector towards the fingertip, with all axes in display widths.</summary>
        public Vec3 Direction { get; }

        /// <summary>Length of the segment in display widths.</summary>
        public float SegmentLength { get; }
    }
}
