using ArRingTryOn.Core;
using ArRingTryOn.HandTracking;
using ArRingTryOn.Rendering;
using ArRingTryOn.UI;
using UnityEngine;

namespace ArRingTryOn.App
{
    /// <summary>Composition root: feeds tracker output to the camera view, overlay and status label.</summary>
    public sealed class HandTrackingDebugApp : MonoBehaviour
    {
        [SerializeField] private MediaPipeHandTracker _tracker;
        [SerializeField] private CameraFeedView _cameraFeed;
        [SerializeField] private HandLandmarkOverlay _overlay;
        [SerializeField] private TrackingStatusLabel _status;

        private bool _viewsConfigured;
        private HandFrame _shownFrame;

        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            _tracker.StartTracking();
        }

        private void Update()
        {
            if (_tracker.State == HandTrackerState.Running && !_viewsConfigured)
            {
                _cameraFeed.Show(_tracker.CameraTexture, _tracker.View);
                _overlay.Configure(_tracker.View);
                _viewsConfigured = true;
            }

            var frame = _tracker.LatestFrame;
            if (frame != _shownFrame)
            {
                _shownFrame = frame;
                _overlay.Show(frame);
            }

            _status.Show(_tracker.State, _tracker.FailureReason, frame);
        }
    }
}
