using System;

namespace ArRingTryOn.Core
{
    /// <summary>Which ring from a catalog is currently being tried on.</summary>
    public sealed class RingSelection
    {
        private readonly RingCatalog _catalog;

        public RingSelection(RingCatalog catalog, int initialIndex = 0)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            CurrentIndex = CheckIndex(initialIndex);
        }

        /// <summary>Raised with the new design whenever the selection actually changes.</summary>
        public event Action<RingDesign> Changed;

        public int CurrentIndex { get; private set; }

        public RingDesign Current => _catalog[CurrentIndex];

        public void SelectIndex(int index)
        {
            var checkedIndex = CheckIndex(index);
            if (checkedIndex == CurrentIndex)
            {
                return;
            }

            CurrentIndex = checkedIndex;
            Changed?.Invoke(Current);
        }

        /// <returns>False, leaving the selection unchanged, if the catalog has no design with this id.</returns>
        public bool TrySelect(string id)
        {
            var index = _catalog.IndexOf(id);
            if (index < 0)
            {
                return false;
            }

            SelectIndex(index);
            return true;
        }

        /// <summary>Moves to the next design, wrapping from the last to the first.</summary>
        public void Next() => SelectIndex((CurrentIndex + 1) % _catalog.Count);

        /// <summary>Moves to the previous design, wrapping from the first to the last.</summary>
        public void Previous() => SelectIndex((CurrentIndex - 1 + _catalog.Count) % _catalog.Count);

        private int CheckIndex(int index)
        {
            if (index < 0 || index >= _catalog.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Catalog has {_catalog.Count} rings.");
            }

            return index;
        }
    }
}
