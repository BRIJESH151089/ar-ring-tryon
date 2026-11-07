using System;

namespace ArRingTryOn.Core
{
    /// <summary>
    /// How a raw camera image relates to what the user sees. The tracker reports landmarks in
    /// the camera's unrotated image; this converts them to upright display space.
    /// </summary>
    public readonly struct CameraView
    {
        /// <param name="imageWidth">Raw camera image width in pixels.</param>
        /// <param name="imageHeight">Raw camera image height in pixels.</param>
        /// <param name="rotationDegrees">Clockwise rotation that makes the image upright.</param>
        /// <param name="isFrontFacing">True for a selfie camera, whose image is shown mirrored.</param>
        /// <param name="isVerticallyMirrored">True if the camera delivers its image upside down.</param>
        public CameraView(int imageWidth, int imageHeight, int rotationDegrees, bool isFrontFacing, bool isVerticallyMirrored)
        {
            if (imageWidth <= 0 || imageHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(imageWidth), "Image size must be positive.");
            }

            var normalizedRotation = ((rotationDegrees % 360) + 360) % 360;
            if (normalizedRotation % 90 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rotationDegrees), rotationDegrees, "Rotation must be a multiple of 90.");
            }

            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            RotationDegrees = normalizedRotation;
            IsFrontFacing = isFrontFacing;
            IsVerticallyMirrored = isVerticallyMirrored;
        }

        public int ImageWidth { get; }

        public int ImageHeight { get; }

        public int RotationDegrees { get; }

        public bool IsFrontFacing { get; }

        public bool IsVerticallyMirrored { get; }

        private bool IsQuarterTurn => RotationDegrees == 90 || RotationDegrees == 270;

        public int DisplayWidth => IsQuarterTurn ? ImageHeight : ImageWidth;

        public int DisplayHeight => IsQuarterTurn ? ImageWidth : ImageHeight;

        public float DisplayAspect => (float)DisplayWidth / DisplayHeight;

        /// <summary>Whether the feed's texture U axis must be flipped to match the tracker's input.</summary>
        public bool FlipTextureU => IsFrontFacing && !IsQuarterTurn;

        /// <summary>Whether the feed's texture V axis must be flipped to match the tracker's input.</summary>
        public bool FlipTextureV => IsVerticallyMirrored ^ (IsFrontFacing && IsQuarterTurn);

        /// <summary>Rotates a normalized point (y down, z in image widths) into display space.</summary>
        public Vec3 ToDisplay(Vec3 imagePoint)
        {
            var z = imagePoint.Z * ImageWidth / DisplayWidth;

            switch (RotationDegrees)
            {
                case 90: return new Vec3(1f - imagePoint.Y, imagePoint.X, z);
                case 180: return new Vec3(1f - imagePoint.X, 1f - imagePoint.Y, z);
                case 270: return new Vec3(imagePoint.Y, 1f - imagePoint.X, z);
                default: return new Vec3(imagePoint.X, imagePoint.Y, z);
            }
        }

        /// <summary>
        /// Turns a reported hand side into the user's real hand. The tracker labels a hand by how
        /// it looks in the image it is given, so on a mirrored (selfie) image the label is swapped.
        /// </summary>
        public HandSide ResolveSide(HandSide reported)
        {
            if (!IsFrontFacing || reported == HandSide.Unknown)
            {
                return reported;
            }

            return reported == HandSide.Left ? HandSide.Right : HandSide.Left;
        }
    }
}
