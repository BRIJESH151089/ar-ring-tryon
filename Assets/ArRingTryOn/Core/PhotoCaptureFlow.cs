using System;

namespace ArRingTryOn.Core
{
    /// <summary>
    /// The steps of taking a photo and saving it to the gallery, and what to tell the user at
    /// the end. Only one photo can be in progress at a time.
    /// </summary>
    public sealed class PhotoCaptureFlow
    {
        public enum Step
        {
            Idle,
            Capturing,
            AwaitingPermission,
            Saving,
        }

        public enum FollowUp
        {
            None,
            Share,
            OpenSettings,
        }

        public Step Current { get; private set; } = Step.Idle;

        public bool IsBusy => Current != Step.Idle;

        /// <summary>Message to show the user once the flow ends, or null while it runs.</summary>
        public string Message { get; private set; }

        /// <summary>What the user can do next once the flow ends.</summary>
        public FollowUp Offer { get; private set; }

        /// <returns>False if a photo is already being taken.</returns>
        public bool TryBegin()
        {
            if (IsBusy)
            {
                return false;
            }

            Current = Step.Capturing;
            Message = null;
            Offer = FollowUp.None;
            return true;
        }

        /// <param name="photo">The encoded photo, or null or empty if capturing failed.</param>
        public void OnCaptured(byte[] photo)
        {
            Expect(Step.Capturing);
            if (photo == null || photo.Length == 0)
            {
                Finish("Couldn't take the photo", FollowUp.None);
                return;
            }

            Current = Step.AwaitingPermission;
        }

        public void OnPermission(GalleryPermission permission)
        {
            Expect(Step.AwaitingPermission);
            switch (permission)
            {
                case GalleryPermission.Granted:
                    Current = Step.Saving;
                    break;
                case GalleryPermission.ShouldAsk:
                    Finish("Allow photo access to save to the gallery", FollowUp.None);
                    break;
                default:
                    Finish("Photo access is off. Turn it on in Settings to save", FollowUp.OpenSettings);
                    break;
            }
        }

        public void OnSaved(bool success)
        {
            Expect(Step.Saving);
            if (success)
            {
                Finish("Saved to gallery", FollowUp.Share);
            }
            else
            {
                Finish("Couldn't save the photo", FollowUp.None);
            }
        }

        private void Finish(string message, FollowUp offer)
        {
            Current = Step.Idle;
            Message = message;
            Offer = offer;
        }

        private void Expect(Step step)
        {
            if (Current != step)
            {
                throw new InvalidOperationException($"Expected step {step} but the flow is at {Current}.");
            }
        }
    }
}
