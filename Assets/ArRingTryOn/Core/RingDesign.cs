using System;

namespace ArRingTryOn.Core
{
    /// <summary>One ring the user can try on.</summary>
    public sealed class RingDesign
    {
        public RingDesign(string id, string displayName, RingStyle style)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A ring needs an id.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("A ring needs a display name.", nameof(displayName));
            }

            Id = id;
            DisplayName = displayName;
            Style = style;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public RingStyle Style { get; }
    }
}
