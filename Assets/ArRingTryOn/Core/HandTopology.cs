using System;
using System.Collections.Generic;

namespace ArRingTryOn.Core
{
    /// <summary>Which landmarks belong to which finger, and how the hand skeleton connects.</summary>
    public static class HandTopology
    {
        public static readonly IReadOnlyList<(HandLandmarkId From, HandLandmarkId To)> Connections = new[]
        {
            (HandLandmarkId.Wrist, HandLandmarkId.ThumbCmc),
            (HandLandmarkId.ThumbCmc, HandLandmarkId.ThumbMcp),
            (HandLandmarkId.ThumbMcp, HandLandmarkId.ThumbIp),
            (HandLandmarkId.ThumbIp, HandLandmarkId.ThumbTip),
            (HandLandmarkId.Wrist, HandLandmarkId.IndexMcp),
            (HandLandmarkId.IndexMcp, HandLandmarkId.MiddleMcp),
            (HandLandmarkId.MiddleMcp, HandLandmarkId.RingMcp),
            (HandLandmarkId.RingMcp, HandLandmarkId.PinkyMcp),
            (HandLandmarkId.Wrist, HandLandmarkId.PinkyMcp),
            (HandLandmarkId.IndexMcp, HandLandmarkId.IndexPip),
            (HandLandmarkId.IndexPip, HandLandmarkId.IndexDip),
            (HandLandmarkId.IndexDip, HandLandmarkId.IndexTip),
            (HandLandmarkId.MiddleMcp, HandLandmarkId.MiddlePip),
            (HandLandmarkId.MiddlePip, HandLandmarkId.MiddleDip),
            (HandLandmarkId.MiddleDip, HandLandmarkId.MiddleTip),
            (HandLandmarkId.RingMcp, HandLandmarkId.RingPip),
            (HandLandmarkId.RingPip, HandLandmarkId.RingDip),
            (HandLandmarkId.RingDip, HandLandmarkId.RingTip),
            (HandLandmarkId.PinkyMcp, HandLandmarkId.PinkyPip),
            (HandLandmarkId.PinkyPip, HandLandmarkId.PinkyDip),
            (HandLandmarkId.PinkyDip, HandLandmarkId.PinkyTip),
        };

        /// <summary>
        /// The two joints bounding the segment a ring sits on: the finger's base joint and the
        /// next joint towards the tip.
        /// </summary>
        public static (HandLandmarkId Base, HandLandmarkId Next) RingSegment(Finger finger)
        {
            switch (finger)
            {
                case Finger.Thumb: return (HandLandmarkId.ThumbMcp, HandLandmarkId.ThumbIp);
                case Finger.Index: return (HandLandmarkId.IndexMcp, HandLandmarkId.IndexPip);
                case Finger.Middle: return (HandLandmarkId.MiddleMcp, HandLandmarkId.MiddlePip);
                case Finger.Ring: return (HandLandmarkId.RingMcp, HandLandmarkId.RingPip);
                case Finger.Pinky: return (HandLandmarkId.PinkyMcp, HandLandmarkId.PinkyPip);
                default: throw new ArgumentOutOfRangeException(nameof(finger), finger, null);
            }
        }
    }
}
