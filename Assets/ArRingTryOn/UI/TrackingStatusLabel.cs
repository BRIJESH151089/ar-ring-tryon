using ArRingTryOn.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ArRingTryOn.UI
{
    /// <summary>Shows what the tracker is doing, so a missing overlay explains itself on device.</summary>
    [RequireComponent(typeof(Text))]
    public sealed class TrackingStatusLabel : MonoBehaviour
    {
        private Text _text;

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        public void Show(HandTrackerState state, string failureReason, HandFrame frame)
        {
            _text.text = Describe(state, failureReason, frame);
        }

        private static string Describe(HandTrackerState state, string failureReason, HandFrame frame)
        {
            switch (state)
            {
                case HandTrackerState.Idle:
                case HandTrackerState.Starting:
                    return "Starting camera...";
                case HandTrackerState.PermissionDenied:
                    return "Camera permission denied. Allow it in system settings and reopen the app.";
                case HandTrackerState.Failed:
                    return $"Hand tracking failed: {failureReason}";
                default:
                    return DescribeHands(frame);
            }
        }

        private static string DescribeHands(HandFrame frame)
        {
            if (frame == null || frame.Hands.Count == 0)
            {
                return "Show your hand to the camera";
            }

            return $"{frame.Hands[0].Side} hand: {HandLandmarks.Count} landmarks";
        }
    }
}
