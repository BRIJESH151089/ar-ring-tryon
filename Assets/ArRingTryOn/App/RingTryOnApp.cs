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
        [SerializeField] private FingerOccluderView _fingerOccluder;
        [SerializeField] private HandLandmarkOverlay _overlay;
        [SerializeField] private TrackingStatusLabel _status;
        [SerializeField] private RingPickerView _picker;
        [SerializeField] private DoubleTapGesture _overlayToggle;
        [SerializeField] private bool _showDebugOverlay = true;

        [Tooltip("Where the ring sits on the first finger bone: 0 at the knuckle, 1 at the next joint. Applies live in Play mode.")]
        [SerializeField, Range(0f, 1f)] private float _ringPositionAlongFinger = RingPoseCalculator.DefaultPositionAlongSegment;

        [Tooltip("How the ring follows the hand. Changes apply live in Play mode.")]
        [SerializeField] private RingSmoothingSettings _smoothing = RingSmoothingSettings.Default;

        private readonly RingCatalog _catalog = RingCatalog.CreateDefault();
        private RingTracker _ringTracker;
        private RingSelection _selection;
        private bool _viewsConfigured;
        private HandFrame _shownFrame;
        private HandTrackerState _shownState = (HandTrackerState)(-1);

        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            _ringTracker = new RingTracker(_smoothing) { PositionAlongSegment = _ringPositionAlongFinger };
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
                _fingerOccluder.Configure(_tracker.View);
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
            if (_ringTracker.TryUpdate(frame, out var pose, out var fingerTube))
            {
                _fingerOccluder.Show(fingerTube);
                _ring.Show(pose);
            }
            else
            {
                _fingerOccluder.Hide();
                _ring.Hide();
            }
        }

        private void OnValidate()
        {
            // Lets placement and smoothing be tuned from the Inspector while the app runs.
            if (_ringTracker != null)
            {
                _ringTracker.Settings = _smoothing;
                _ringTracker.PositionAlongSegment = _ringPositionAlongFinger;
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
