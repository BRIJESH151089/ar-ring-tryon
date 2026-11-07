using System;

namespace ArRingTryOn.Core
{
    /// <summary>Smooths a stream of ring poses over time.</summary>
    public sealed class RingPoseFilter
    {
        private readonly Vec3OneEuroFilter _position;
        private readonly QuatOneEuroFilter _rotation;
        private readonly OneEuroFilter _scale;
        private RingSmoothingSettings _settings;
        private bool _hasValue;
        private double _lastTime;
        private float _lastScale;

        public RingPoseFilter(RingSmoothingSettings settings)
        {
            _position = new Vec3OneEuroFilter(settings.Position);
            _rotation = new QuatOneEuroFilter(settings.Rotation);
            _scale = new OneEuroFilter(settings.Scale);
            _settings = settings;
        }

        public RingSmoothingSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                _position.Settings = value.Position;
                _rotation.Settings = value.Rotation;
                _scale.Settings = value.Scale;
            }
        }

        public RingPose Filter(RingPose sample, double timeSeconds)
        {
            var position = _position.Filter(sample.Position, timeSeconds);
            var rotation = _rotation.Filter(sample.Rotation, timeSeconds);
            var scale = LimitScaleChange(_scale.Filter(sample.Scale, timeSeconds), timeSeconds);
            return new RingPose(position, rotation, scale);
        }

        public void Reset()
        {
            _position.Reset();
            _rotation.Reset();
            _scale.Reset();
            _hasValue = false;
        }

        private float LimitScaleChange(float scale, double timeSeconds)
        {
            if (_hasValue && _settings.MaxScaleChangePerSecond > 0f && timeSeconds > _lastTime && scale > 0f)
            {
                var limit = _settings.MaxScaleChangePerSecond * (timeSeconds - _lastTime);
                var change = Math.Log(scale / _lastScale);
                scale = _lastScale * (float)Math.Exp(Math.Max(-limit, Math.Min(limit, change)));
            }

            if (!_hasValue || timeSeconds > _lastTime)
            {
                _hasValue = true;
                _lastTime = timeSeconds;
                _lastScale = scale;
            }

            return _lastScale;
        }
    }
}
