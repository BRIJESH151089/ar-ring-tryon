using System;
using System.Collections.Generic;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class RingSelectionTests
    {
        private RingCatalog _catalog;
        private RingSelection _selection;
        private List<RingDesign> _changes;

        [SetUp]
        public void SetUp()
        {
            _catalog = RingCatalog.CreateDefault();
            _selection = new RingSelection(_catalog);
            _changes = new List<RingDesign>();
            _selection.Changed += _changes.Add;
        }

        [Test]
        public void StartsOnFirstDesign()
        {
            Assert.That(_selection.CurrentIndex, Is.EqualTo(0));
            Assert.That(_selection.Current, Is.SameAs(_catalog[0]));
        }

        [Test]
        public void CanStartOnAnotherDesign()
        {
            Assert.That(new RingSelection(_catalog, 2).Current, Is.SameAs(_catalog[2]));
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void RejectsOutOfRangeStart(int index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RingSelection(_catalog, index));
        }

        [Test]
        public void RejectsNullCatalog()
        {
            Assert.Throws<ArgumentNullException>(() => new RingSelection(null));
        }

        [Test]
        public void TrySelect_SelectsDesignWithThatId()
        {
            Assert.That(_selection.TrySelect("gemstone"), Is.True);

            Assert.That(_selection.Current.Id, Is.EqualTo("gemstone"));
            Assert.That(_changes, Is.EqualTo(new[] { _catalog[2] }));
        }

        [Test]
        public void TrySelect_UnknownIdLeavesSelectionAlone()
        {
            _selection.SelectIndex(1);
            _changes.Clear();

            Assert.That(_selection.TrySelect("missing"), Is.False);

            Assert.That(_selection.CurrentIndex, Is.EqualTo(1));
            Assert.That(_changes, Is.Empty);
        }

        [Test]
        public void TrySelect_CurrentDesignSucceedsWithoutRaisingChanged()
        {
            Assert.That(_selection.TrySelect("plain-band"), Is.True);

            Assert.That(_changes, Is.Empty);
        }

        [TestCase(-1)]
        [TestCase(3)]
        [TestCase(int.MaxValue)]
        public void SelectIndex_RejectsOutOfRangeAndKeepsSelection(int index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _selection.SelectIndex(index));

            Assert.That(_selection.CurrentIndex, Is.EqualTo(0));
            Assert.That(_changes, Is.Empty);
        }

        [Test]
        public void SelectIndex_AcceptsLastIndex()
        {
            _selection.SelectIndex(2);

            Assert.That(_selection.Current, Is.SameAs(_catalog[2]));
        }

        [Test]
        public void Next_StepsForwardThenWrapsToFirst()
        {
            var seen = new List<int>();
            for (var i = 0; i < 4; i++)
            {
                _selection.Next();
                seen.Add(_selection.CurrentIndex);
            }

            Assert.That(seen, Is.EqualTo(new[] { 1, 2, 0, 1 }));
            Assert.That(_changes.Count, Is.EqualTo(4));
        }

        [Test]
        public void Previous_WrapsToLastThenStepsBack()
        {
            var seen = new List<int>();
            for (var i = 0; i < 4; i++)
            {
                _selection.Previous();
                seen.Add(_selection.CurrentIndex);
            }

            Assert.That(seen, Is.EqualTo(new[] { 2, 1, 0, 2 }));
        }

        [Test]
        public void NextAndPrevious_DoNothingInASingleRingCatalog()
        {
            var single = new RingSelection(new RingCatalog(new[] { new RingDesign("band", "Band", RingStyle.PlainBand) }));
            var raised = 0;
            single.Changed += _ => raised++;

            single.Next();
            single.Previous();

            Assert.That(single.CurrentIndex, Is.EqualTo(0));
            Assert.That(raised, Is.EqualTo(0));
        }

        [Test]
        public void Changed_ReportsTheNewCurrentDesign()
        {
            RingDesign reported = null;
            var currentAtEvent = -1;
            _selection.Changed += design =>
            {
                reported = design;
                currentAtEvent = _selection.CurrentIndex;
            };

            _selection.SelectIndex(1);

            Assert.That(reported, Is.SameAs(_catalog[1]));
            Assert.That(currentAtEvent, Is.EqualTo(1));
        }
    }
}
