using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArRingTryOn.UI
{
    /// <summary>Reports a double tap anywhere on the screen outside the ignored areas.</summary>
    public sealed class DoubleTapGesture : MonoBehaviour
    {
        private const float MaxSecondsBetweenTaps = 0.35f;

        [Tooltip("Taps here are left to the controls, such as buttons tapped twice in a row.")]
        [SerializeField] private RectTransform[] _ignoredAreas = Array.Empty<RectTransform>();

        private float _lastTapTime = float.NegativeInfinity;

        public event Action DoubleTapped;

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
            {
                return;
            }

            if (IsOverIgnoredArea(pointer.position.ReadValue()))
            {
                _lastTapTime = float.NegativeInfinity;
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

        private bool IsOverIgnoredArea(Vector2 screenPoint)
        {
            foreach (var area in _ignoredAreas)
            {
                if (area != null && area.gameObject.activeInHierarchy
                    && RectTransformUtility.RectangleContainsScreenPoint(area, screenPoint, null))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
