using System;
using System.Collections;
using System.IO;
using ArRingTryOn.Core;
using ArRingTryOn.Rendering;
using ArRingTryOn.Sharing;
using ArRingTryOn.UI;
using UnityEngine;

namespace ArRingTryOn.App
{
    /// <summary>Wires the shutter to capturing, saving to the gallery and sharing.</summary>
    public sealed class PhotoController : MonoBehaviour
    {
        [SerializeField] private PhotoCapture _capture;
        [SerializeField] private CaptureControls _controls;

        private readonly PhotoCaptureFlow _flow = new PhotoCaptureFlow();
        private string _sharePath;

        private void Start()
        {
            _controls.Bind(Share, PhotoGallery.OpenSettings);
            _controls.ShutterPressed += OnShutterPressed;
        }

        private void OnShutterPressed()
        {
            if (!_flow.TryBegin())
            {
                return;
            }

            _controls.SetBusy(true);
            StartCoroutine(TakePhoto());
        }

        private IEnumerator TakePhoto()
        {
            // Capture once this frame has finished rendering, so the ring is where it is shown.
            yield return new WaitForEndOfFrame();

            var photo = Capture();
            _flow.OnCaptured(photo);
            if (!_flow.IsBusy)
            {
                Finish();
                yield break;
            }

            var fileName = PhotoNaming.FileName(DateTime.Now);
            KeepCopyForSharing(photo, fileName);
            PhotoGallery.RequestPermission(permission => OnPermission(permission, photo, fileName));
        }

        private void OnPermission(GalleryPermission permission, byte[] photo, string fileName)
        {
            _flow.OnPermission(permission);
            if (_flow.Current != PhotoCaptureFlow.Step.Saving)
            {
                Finish();
                return;
            }

            try
            {
                PhotoGallery.Save(photo, PhotoNaming.Album, fileName, OnSaved);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                OnSaved(false);
            }
        }

        private void OnSaved(bool success)
        {
            _flow.OnSaved(success);
            Finish();
        }

        private void Finish()
        {
            _controls.SetBusy(false);
            _controls.ShowResult(_flow.Message, _flow.Offer);
        }

        private byte[] Capture()
        {
            try
            {
                return _capture.CaptureJpeg();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return null;
            }
        }

        // Sharing needs a file on disk. Only the latest photo is kept, so the cache does not grow.
        private void KeepCopyForSharing(byte[] photo, string fileName)
        {
            try
            {
                if (!string.IsNullOrEmpty(_sharePath) && File.Exists(_sharePath))
                {
                    File.Delete(_sharePath);
                }

                _sharePath = Path.Combine(Application.temporaryCachePath, fileName);
                File.WriteAllBytes(_sharePath, photo);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                _sharePath = null;
            }
        }

        private void Share()
        {
            if (!PhotoSharer.Share(_sharePath, PhotoNaming.MimeType))
            {
                _controls.ShowResult("Couldn't share the photo", PhotoCaptureFlow.FollowUp.None);
            }
        }

        private void OnDestroy()
        {
            _controls.ShutterPressed -= OnShutterPressed;
        }
    }
}
