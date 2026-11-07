using System;
using System.Collections.Generic;

namespace ArRingTryOn.Core
{
    /// <summary>
    /// A tapered tube following the ring finger, in <see cref="ViewSpace"/>. It stands in for the
    /// finger so that the part of the ring behind the finger can be hidden.
    /// </summary>
    public readonly struct FingerTube
    {
        public FingerTube(IReadOnlyList<Vec3> centres, IReadOnlyList<float> radii)
        {
            if (centres == null || radii == null || centres.Count != radii.Count || centres.Count < 2)
            {
                throw new ArgumentException("A tube needs at least two centres, each with a radius.");
            }

            Centres = centres;
            Radii = radii;
        }

        /// <summary>Points along the finger, from the base towards the tip.</summary>
        public IReadOnlyList<Vec3> Centres { get; }

        public IReadOnlyList<float> Radii { get; }
    }
}
