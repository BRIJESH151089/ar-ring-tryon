using System;

namespace ArRingTryOn.Core
{
    /// <summary>One-Euro filter for a single value. Each output lies between the previous output
    /// and the new sample, so a step is approached without overshoot.</summary>
    public sealed class OneEuroFilter
    {
        private bool _hasValue;
        private double _lastTime;
        private float _value;
        private float _speed;

        public OneEuroFilter(OneEuroSettings settings)
        {
            Settings = settings;
        }

        public OneEuroSettings Settings { get; set; }

        /// <param name="sample">The new raw value.</param>
        /// <param name="timeSeconds">When it was measured. Samples that are not newer are ignored.</param>
        public float Filter(float sample, double timeSeconds)
        {
            if (!_hasValue)
            {
                _hasValue = true;
                _lastTime = timeSeconds;
                _value = sample;
                _speed = 0f;
                return sample;
            }

            var dt = timeSeconds - _lastTime;
            if (dt <= 0.0)
            {
                return _value;
            }

            _lastTime = timeSeconds;
            var rawSpeed = (float)((sample - _value) / dt);
            _speed += OneEuroSettings.Alpha(Settings.DerivativeCutoff, dt) * (rawSpeed - _speed);
            var alpha = OneEuroSettings.Alpha(Settings.CutoffAt(Math.Abs(_speed)), dt);
            _value += alpha * (sample - _value);
            return _value;
        }

        public void Reset() => _hasValue = false;
    }
}
