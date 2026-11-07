using ArRingTryOn.Core;
using ArRingTryOn.HandTracking;
using ArRingTryOn.Rendering;
using ArRingTryOn.UI;
using UnityEngine;

namespace ArRingTryOn.App
{
    /// <summary>Composition root: turns tracker output and the chosen ring into what is on screen.</summary>
    public sealed class RingTryOnApp : MonoBehaviour
    {
        [SerializeField] private MediaPipeHandTracker _tracker;
        [SerializeField] private CameraFeedView _cameraFeed;
        [SerializeField] private RingView _ring;
        [SerializeField] private HandLandmarkOverlay _overlay;
        [SerializeField] private TrackingStatusLabel _status;
        [SerializeField] private RingPickerView _picker;
        [SerializeField] private DoubleTapGesture _overlayToggle;
        [SerializeField] private bool _showDebugOverlay = true;

        private readonly RingCatalog _catalog = RingCatalog.CreateDefault();
        private RingSelection _selection;
        private bool _viewsConfigured;
        private HandFrame _shownFrame;
        private HandTrackerState _shownState = (HandTrackerState)(-1);

        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            _selection = new RingSelection(_catalog);
            _ring.SetStyle(_selection.Current.Style);
            _selection.Changed += OnRingSelected;
            _picker.Bind(_catalog, _selection);

            _overlay.enabled = _showDebugOverlay;
            _overlayToggle.DoubleTapped += ToggleOverlay;
            _tracker.StartTracking();
        }

        private void Update()
        {
            var state = _tracker.State;
            if (state == HandTrackerState.Running && !_viewsConfigured)
            {
                _cameraFeed.Show(_tracker.CameraTexture, _tracker.View);
                _overlay.Configure(_tracker.View);
                _ring.Configure(_tracker.View);
                _viewsConfigured = true;
            }

            var frame = _tracker.LatestFrame;
            if (frame == _shownFrame && state == _shownState)
            {
                return;
            }

            if (frame != _shownFrame)
            {
                _overlay.Show(frame);
                PlaceRing(frame);
            }

            _shownFrame = frame;
            _shownState = state;
            _status.Show(state, _tracker.FailureReason, frame);
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

        private void OnRingSelected(RingDesign design)
        {
            _ring.SetStyle(design.Style);
        }

        private void ToggleOverlay()
        {
            _overlay.enabled = !_overlay.enabled;
        }

        private void OnDestroy()
        {
            if (_selection != null)
            {
                _selection.Changed -= OnRingSelected;
            }

            _overlayToggle.DoubleTapped -= ToggleOverlay;
        }
    }
}
