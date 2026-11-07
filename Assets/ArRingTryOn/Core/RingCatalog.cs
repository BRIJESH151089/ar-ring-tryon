using System;
using System.Collections.Generic;

namespace ArRingTryOn.Core
{
    /// <summary>The rings on offer, in display order.</summary>
    public sealed class RingCatalog
    {
        private readonly RingDesign[] _designs;

        public RingCatalog(IReadOnlyList<RingDesign> designs)
        {
            if (designs == null)
            {
                throw new ArgumentNullException(nameof(designs));
            }

            if (designs.Count == 0)
            {
                throw new ArgumentException("A catalog needs at least one ring.", nameof(designs));
            }

            var ids = new HashSet<string>();
            _designs = new RingDesign[designs.Count];
            for (var i = 0; i < designs.Count; i++)
            {
                var design = designs[i] ?? throw new ArgumentException($"Ring {i} is null.", nameof(designs));
                if (!ids.Add(design.Id))
                {
                    throw new ArgumentException($"Duplicate ring id '{design.Id}'.", nameof(designs));
                }

                _designs[i] = design;
            }
        }

        public static RingCatalog CreateDefault()
        {
            return new RingCatalog(new[]
            {
                new RingDesign("plain-band", "Gold Band", RingStyle.PlainBand),
                new RingDesign("signet", "Signet", RingStyle.Signet),
                new RingDesign("gemstone", "Gemstone", RingStyle.Gemstone),
            });
        }

        public int Count => _designs.Length;

        public RingDesign this[int index] => _designs[index];

        /// <returns>The design's index, or -1 if no design has this id.</returns>
        public int IndexOf(string id)
        {
            for (var i = 0; i < _designs.Length; i++)
            {
                if (_designs[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
