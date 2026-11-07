using System;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class PhotoCaptureFlowTests
    {
        private static readonly byte[] Photo = { 1, 2, 3 };

        [Test]
        public void StartsIdle()
        {
            var flow = new PhotoCaptureFlow();

            Assert.That(flow.IsBusy, Is.False);
            Assert.That(flow.Message, Is.Null);
        }

        [Test]
        public void HappyPath_EndsSavedAndOffersSharing()
        {
            var flow = new PhotoCaptureFlow();

            Assert.That(flow.TryBegin(), Is.True);
            flow.OnCaptured(Photo);
            Assert.That(flow.Current, Is.EqualTo(PhotoCaptureFlow.Step.AwaitingPermission));
            flow.OnPermission(GalleryPermission.Granted);
            Assert.That(flow.Current, Is.EqualTo(PhotoCaptureFlow.Step.Saving));
            flow.OnSaved(true);

            Assert.That(flow.IsBusy, Is.False);
            Assert.That(flow.Message, Is.EqualTo("Saved to gallery"));
            Assert.That(flow.Offer, Is.EqualTo(PhotoCaptureFlow.FollowUp.Share));
        }

        [Test]
        public void SecondTapWhileBusy_IsRefused()
        {
            var flow = new PhotoCaptureFlow();
            flow.TryBegin();

            Assert.That(flow.TryBegin(), Is.False);
            flow.OnCaptured(Photo);
            Assert.That(flow.TryBegin(), Is.False);
            flow.OnPermission(GalleryPermission.Granted);
            Assert.That(flow.TryBegin(), Is.False);
        }

        [Test]
        public void CanTakeAnotherPhotoAfterFinishing()
        {
            var flow = new PhotoCaptureFlow();
            flow.TryBegin();
            flow.OnCaptured(Photo);
            flow.OnPermission(GalleryPermission.Granted);
            flow.OnSaved(true);

            Assert.That(flow.TryBegin(), Is.True);
            Assert.That(flow.Message, Is.Null, "the old message is cleared");
            Assert.That(flow.Offer, Is.EqualTo(PhotoCaptureFlow.FollowUp.None));
        }

        [Test]
        public void PermanentDenial_PointsToSettingsWithoutSaving()
        {
            var flow = new PhotoCaptureFlow();
            flow.TryBegin();
            flow.OnCaptured(Photo);

            flow.OnPermission(GalleryPermission.Denied);

            Assert.That(flow.IsBusy, Is.False);
            Assert.That(flow.Offer, Is.EqualTo(PhotoCaptureFlow.FollowUp.OpenSettings));
            Assert.That(flow.Message, Does.Contain("Settings"));
            Assert.Throws<InvalidOperationException>(() => flow.OnSaved(true), "nothing should be saved after a denial");
        }

        [Test]
        public void DenialThatCanBeAskedAgain_EndsQuietlySoTheNextTapAsksAgain()
        {
            var flow = new PhotoCaptureFlow();
            flow.TryBegin();
            flow.OnCaptured(Photo);

            flow.OnPermission(GalleryPermission.ShouldAsk);

            Assert.That(flow.IsBusy, Is.False);
            Assert.That(flow.Offer, Is.EqualTo(PhotoCaptureFlow.FollowUp.None));
            Assert.That(flow.TryBegin(), Is.True);
        }

        [Test]
        public void FailedSave_ReportsWithoutOfferingToShare()
        {
            var flow = new PhotoCaptureFlow();
            flow.TryBegin();
            flow.OnCaptured(Photo);
            flow.OnPermission(GalleryPermission.Granted);

            flow.OnSaved(false);

            Assert.That(flow.Message, Is.EqualTo("Couldn't save the photo"));
            Assert.That(flow.Offer, Is.EqualTo(PhotoCaptureFlow.FollowUp.None));
        }

        [TestCase(null)]
        [TestCase(new byte[0])]
        public void FailedCapture_EndsWithoutAskingForPermission(byte[] photo)
        {
            var flow = new PhotoCaptureFlow();
            flow.TryBegin();

            flow.OnCaptured(photo);

            Assert.That(flow.IsBusy, Is.False);
            Assert.That(flow.Message, Is.EqualTo("Couldn't take the photo"));
            Assert.Throws<InvalidOperationException>(() => flow.OnPermission(GalleryPermission.Granted));
        }

        [Test]
        public void StepsOutOfOrder_AreRejected()
        {
            var flow = new PhotoCaptureFlow();

            Assert.Throws<InvalidOperationException>(() => flow.OnCaptured(Photo));
            Assert.Throws<InvalidOperationException>(() => flow.OnPermission(GalleryPermission.Granted));
            Assert.Throws<InvalidOperationException>(() => flow.OnSaved(true));
        }
    }
}
