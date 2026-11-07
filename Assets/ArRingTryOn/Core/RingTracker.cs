namespace ArRingTryOn.Core
{
    /// <summary>
    /// Turns hand frames into a smoothed ring pose and a matching finger tube. Both are smoothed
    /// alike so that the ring and the tube that hides its back move together.
    /// </summary>
    public sealed class RingTracker
    {
        private readonly RingPoseFilter _poseFilter;
        private readonly Vec3OneEuroFilter[] _tubeFilters;
        private float _positionAlongSegment = RingPoseCalculator.DefaultPositionAlongSegment;

        public RingTracker(RingSmoothingSettings settings)
        {
            _poseFilter = new RingPoseFilter(settings);
            _tubeFilters = new Vec3OneEuroFilter[FingerTubeCalculator.CentreCount];
            for (var i = 0; i < _tubeFilters.Length; i++)
            {
                _tubeFilters[i] = new Vec3OneEuroFilter(settings.Position);
            }
        }

        public RingSmoothingSettings Settings
        {
            get => _poseFilter.Settings;
            set
            {
                _poseFilter.Settings = value;
                foreach (var filter in _tubeFilters)
                {
                    filter.Settings = value.Position;
                }
            }
        }

        /// <summary>
        /// Where along the first finger bone the ring sits, from 0 (knuckle) to 1 (next joint).
        /// Values outside that range are clamped.
        /// </summary>
        public float PositionAlongSegment
        {
            get => _positionAlongSegment;
            set => _positionAlongSegment = float.IsNaN(value) ? RingPoseCalculator.DefaultPositionAlongSegment : Clamp01(value);
        }

        /// <returns>
        /// False when there is no usable hand. Smoothing then starts afresh, so a returning hand
        /// does not drag the ring across from where it was last seen.
        /// </returns>
        public bool TryUpdate(HandFrame frame, out RingPose pose, out FingerTube tube)
        {
            pose = default;
            tube = default;
            if (frame == null || frame.Hands.Count == 0
                || !RingPoseCalculator.TryCalculate(frame.Hands[0], frame.DisplayAspect, frame.IsImageMirrored, _positionAlongSegment, out var raw))
            {
                Reset();
                return false;
            }

            var time = frame.TimestampMs / 1000.0;
            pose = _poseFilter.Filter(raw, time);

            var rawTube = FingerTubeCalculator.Calculate(frame.Hands[0], frame.DisplayAspect, pose.Scale);
            var centres = new Vec3[_tubeFilters.Length];
            for (var i = 0; i < centres.Length; i++)
            {
                centres[i] = _tubeFilters[i].Filter(rawTube.Centres[i], time);
            }

            tube = new FingerTube(centres, rawTube.Radii);
            return true;
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;

        public void Reset()
        {
            _poseFilter.Reset();
            foreach (var filter in _tubeFilters)
            {
                filter.Reset();
            }
        }
    }
}
