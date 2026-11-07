using System;
using System.Globalization;
using System.Threading;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class PhotoNamingTests
    {
        [Test]
        public void FileName_IsStampedWithTheCaptureTime()
        {
            var name = PhotoNaming.FileName(new DateTime(2026, 11, 7, 17, 25, 30, 42));

            Assert.That(name, Is.EqualTo("ARRingTryOn_20261107_172530_042.jpg"));
        }

        [Test]
        public void FileName_DiffersForShotsInTheSameSecond()
        {
            var first = PhotoNaming.FileName(new DateTime(2026, 11, 7, 17, 25, 30, 100));
            var second = PhotoNaming.FileName(new DateTime(2026, 11, 7, 17, 25, 30, 101));

            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void FileName_IgnoresTheDeviceCalendarAndDigits()
        {
            var original = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("ar-SA");
                var name = PhotoNaming.FileName(new DateTime(2026, 11, 7, 17, 25, 30, 42));
                Assert.That(name, Is.EqualTo("ARRingTryOn_20261107_172530_042.jpg"));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        [Test]
        public void FileName_HasTheJpegExtensionTheGalleryNeeds()
        {
            Assert.That(PhotoNaming.FileName(DateTime.Now), Does.EndWith(PhotoNaming.Extension));
            Assert.That(PhotoNaming.MimeType, Is.EqualTo("image/jpeg"));
        }

        [Test]
        public void Album_IsNamedAfterTheApp()
        {
            Assert.That(PhotoNaming.Album, Is.EqualTo("AR Ring Try-On"));
        }
    }
}
