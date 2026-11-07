using System;
using System.Collections.Generic;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class RingCatalogTests
    {
        [Test]
        public void Default_OffersOneRingOfEachStyleInOrder()
        {
            var catalog = RingCatalog.CreateDefault();

            Assert.That(catalog.Count, Is.EqualTo(3));
            Assert.That(catalog[0].Style, Is.EqualTo(RingStyle.PlainBand));
            Assert.That(catalog[1].Style, Is.EqualTo(RingStyle.Signet));
            Assert.That(catalog[2].Style, Is.EqualTo(RingStyle.Gemstone));
        }

        [Test]
        public void Default_GivesEveryRingANameAndUniqueId()
        {
            var catalog = RingCatalog.CreateDefault();
            var ids = new HashSet<string>();

            for (var i = 0; i < catalog.Count; i++)
            {
                Assert.That(catalog[i].DisplayName, Is.Not.Empty);
                Assert.That(ids.Add(catalog[i].Id), Is.True, catalog[i].Id);
            }
        }

        [Test]
        public void IndexOf_FindsDesignById()
        {
            Assert.That(RingCatalog.CreateDefault().IndexOf("signet"), Is.EqualTo(1));
        }

        [TestCase("missing")]
        [TestCase("")]
        [TestCase(null)]
        [TestCase("Signet")]
        public void IndexOf_ReturnsMinusOneForUnknownId(string id)
        {
            Assert.That(RingCatalog.CreateDefault().IndexOf(id), Is.EqualTo(-1));
        }

        [Test]
        public void Indexer_RejectsOutOfRangeIndex()
        {
            var catalog = RingCatalog.CreateDefault();

            Assert.Throws<IndexOutOfRangeException>(() => _ = catalog[3]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = catalog[-1]);
        }

        [Test]
        public void Constructor_RejectsEmptyCatalog()
        {
            Assert.Throws<ArgumentException>(() => new RingCatalog(Array.Empty<RingDesign>()));
        }

        [Test]
        public void Constructor_RejectsNullList()
        {
            Assert.Throws<ArgumentNullException>(() => new RingCatalog(null));
        }

        [Test]
        public void Constructor_RejectsNullDesign()
        {
            Assert.Throws<ArgumentException>(() => new RingCatalog(new RingDesign[] { null }));
        }

        [Test]
        public void Constructor_RejectsDuplicateIds()
        {
            var designs = new[]
            {
                new RingDesign("band", "Band", RingStyle.PlainBand),
                new RingDesign("band", "Other Band", RingStyle.Signet),
            };

            Assert.Throws<ArgumentException>(() => new RingCatalog(designs));
        }

        [Test]
        public void Constructor_CopiesTheList()
        {
            var designs = new List<RingDesign> { new RingDesign("band", "Band", RingStyle.PlainBand) };
            var catalog = new RingCatalog(designs);

            designs.Add(new RingDesign("signet", "Signet", RingStyle.Signet));

            Assert.That(catalog.Count, Is.EqualTo(1));
        }

        [TestCase(null, "Name")]
        [TestCase(" ", "Name")]
        [TestCase("id", "")]
        [TestCase("id", null)]
        public void Design_RejectsMissingIdOrName(string id, string name)
        {
            Assert.Throws<ArgumentException>(() => new RingDesign(id, name, RingStyle.PlainBand));
        }
    }
}
