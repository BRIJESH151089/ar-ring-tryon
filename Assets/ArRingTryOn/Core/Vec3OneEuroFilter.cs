namespace ArRingTryOn.Core
{
    /// <summary>
    /// One-Euro filter for a point. It uses the speed of the point as a whole, so every axis is
    /// smoothed alike and the result does not depend on which way the point moves.
    /// </summary>
    public sealed class Vec3OneEuroFilter
    {
        private bool _hasValue;
        private double _lastTime;
        private Vec3 _value;
        private float _speed;

        public Vec3OneEuroFilter(OneEuroSettings settings)
        {
            Settings = settings;
        }

        public OneEuroSettings Settings { get; set; }

        public Vec3 Filter(Vec3 sample, double timeSeconds)
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
            var change = sample - _value;
            var rawSpeed = (float)(change.Length / dt);
            _speed += OneEuroSettings.Alpha(Settings.DerivativeCutoff, dt) * (rawSpeed - _speed);
            var alpha = OneEuroSettings.Alpha(Settings.CutoffAt(_speed), dt);
            _value += change * alpha;
            return _value;
        }

        public void Reset() => _hasValue = false;
    }
}
