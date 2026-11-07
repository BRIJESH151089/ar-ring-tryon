using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArRingTryOn.UI
{
    /// <summary>Reports a double tap anywhere on the screen.</summary>
    public sealed class DoubleTapGesture : MonoBehaviour
    {
        private const float MaxSecondsBetweenTaps = 0.35f;

        private float _lastTapTime = float.NegativeInfinity;

        public event Action DoubleTapped;

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
            {
                return;
            }

            var now = Time.unscaledTime;
            if (now - _lastTapTime <= MaxSecondsBetweenTaps)
            {
                _lastTapTime = float.NegativeInfinity;
                DoubleTapped?.Invoke();
            }
            else
            {
                _lastTapTime = now;
            }
        }
    }
}
