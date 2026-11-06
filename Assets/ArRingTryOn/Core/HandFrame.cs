using System;
using System.Collections.Generic;

namespace ArRingTryOn.Core
{
    /// <summary>Immutable tracking result for one camera frame. Safe to pass between threads.</summary>
    public sealed class HandFrame
    {
        public HandFrame(long timestampMs, float displayAspect, IReadOnlyList<HandLandmarks> hands)
        {
            TimestampMs = timestampMs;
            DisplayAspect = displayAspect;
            Hands = hands ?? throw new ArgumentNullException(nameof(hands));
        }

        public long TimestampMs { get; }

        /// <summary>Width over height of the upright camera image the landmarks are expressed in.</summary>
        public float DisplayAspect { get; }

        /// <summary>Detected hands. Empty when no hand is in view.</summary>
        public IReadOnlyList<HandLandmarks> Hands { get; }
    }
}
