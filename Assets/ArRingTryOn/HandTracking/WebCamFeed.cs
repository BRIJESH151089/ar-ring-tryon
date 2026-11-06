using System;
using System.Collections;
using ArRingTryOn.Core;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace ArRingTryOn.HandTracking
{
    /// <summary>Opens the device camera and describes how its image is oriented.</summary>
    internal sealed class WebCamFeed
    {
        // WebCamTexture reports a 16x16 placeholder until the first real frame arrives.
        private const int PlaceholderSize = 16;
        private const float StartTimeoutSeconds = 10f;

        public WebCamTexture Texture { get; private set; }

        public CameraView View { get; private set; }

        public bool PermissionDenied { get; private set; }

        public IEnumerator Open(int width, int height, int frameRate, bool preferFrontCamera)
        {
            yield return RequestPermission();
            if (PermissionDenied)
            {
                yield break;
            }

            var device = PickDevice(preferFrontCamera);
            Texture = new WebCamTexture(device.name, width, height, frameRate);
            Texture.Play();

            var deadline = Time.realtimeSinceStartup + StartTimeoutSeconds;
            while (Texture.width <= PlaceholderSize)
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    throw new TimeoutException($"Camera '{device.name}' did not start.");
                }

                yield return null;
            }

            View = new CameraView(
                Texture.width,
                Texture.height,
                Texture.videoRotationAngle,
                device.isFrontFacing,
                Texture.videoVerticallyMirrored);
        }

        public void Close()
        {
            if (Texture == null)
            {
                return;
            }

            Texture.Stop();
            UnityEngine.Object.Destroy(Texture);
            Texture = null;
        }

        private static WebCamDevice PickDevice(bool preferFrontCamera)
        {
            var devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                throw new InvalidOperationException("No camera found.");
            }

            foreach (var device in devices)
            {
                if (device.isFrontFacing == preferFrontCamera)
                {
                    return device;
                }
            }

            return devices[0];
        }

        private IEnumerator RequestPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                yield break;
            }

            var answered = false;
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => answered = true;
            callbacks.PermissionDenied += _ =>
            {
                PermissionDenied = true;
                answered = true;
            };

            Permission.RequestUserPermission(Permission.Camera, callbacks);
            while (!answered)
            {
                yield return null;
            }
#else
            yield break;
#endif
        }
    }
}
