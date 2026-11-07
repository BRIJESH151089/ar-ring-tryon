namespace ArRingTryOn.Core
{
    /// <summary>One-Euro filter for a rotation: angular speed sets the cutoff, and each step slerps
    /// part of the way from the previous output to the new sample.</summary>
    public sealed class QuatOneEuroFilter
    {
        private bool _hasValue;
        private double _lastTime;
        private Quat _value;
        private float _speed;

        public QuatOneEuroFilter(OneEuroSettings settings)
        {
            Settings = settings;
        }

        /// <summary>Speeds are in radians per second.</summary>
        public OneEuroSettings Settings { get; set; }

        public Quat Filter(Quat sample, double timeSeconds)
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
            var rawSpeed = (float)(Quat.Angle(_value, sample) / dt);
            _speed += OneEuroSettings.Alpha(Settings.DerivativeCutoff, dt) * (rawSpeed - _speed);
            var alpha = OneEuroSettings.Alpha(Settings.CutoffAt(_speed), dt);
            _value = Quat.Slerp(_value, sample, alpha);
            return _value;
        }

        public void Reset() => _hasValue = false;
    }
}
