using ArRingTryOn.Core;
using ArRingTryOn.HandTracking;
using ArRingTryOn.Rendering;
using ArRingTryOn.UI;
using UnityEngine;

namespace ArRingTryOn.App
{
    /// <summary>Composition root: turns tracker output into the ring, camera view, overlay and status.</summary>
    public sealed class RingTryOnApp : MonoBehaviour
    {
        [SerializeField] private MediaPipeHandTracker _tracker;
        [SerializeField] private CameraFeedView _cameraFeed;
        [SerializeField] private RingView _ring;
        [SerializeField] private HandLandmarkOverlay _overlay;
        [SerializeField] private TrackingStatusLabel _status;
        [SerializeField] private DoubleTapGesture _overlayToggle;
        [SerializeField] private bool _showDebugOverlay = true;

        private bool _viewsConfigured;
        private HandFrame _shownFrame;

        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            _overlay.enabled = _showDebugOverlay;
            _overlayToggle.DoubleTapped += ToggleOverlay;
            _tracker.StartTracking();
        }

        private void Update()
        {
            if (_tracker.State == HandTrackerState.Running && !_viewsConfigured)
            {
                _cameraFeed.Show(_tracker.CameraTexture, _tracker.View);
                _overlay.Configure(_tracker.View);
                _ring.Configure(_tracker.View);
                _viewsConfigured = true;
            }

            var frame = _tracker.LatestFrame;
            if (frame != _shownFrame)
            {
                _shownFrame = frame;
                _overlay.Show(frame);
                PlaceRing(frame);
            }

            _status.Show(_tracker.State, _tracker.FailureReason, frame);
        }

        private void PlaceRing(HandFrame frame)
        {
            if (frame.Hands.Count > 0
                && RingPoseCalculator.TryCalculate(frame.Hands[0], frame.DisplayAspect, frame.IsImageMirrored, out var pose))
            {
                _ring.Show(pose);
            }
            else
            {
                _ring.Hide();
            }
        }

        private void ToggleOverlay()
        {
            _overlay.enabled = !_overlay.enabled;
        }

        private void OnDestroy()
        {
            _overlayToggle.DoubleTapped -= ToggleOverlay;
        }
    }
}
