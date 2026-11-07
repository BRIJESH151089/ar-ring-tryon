using ArRingTryOn.Core;
using UnityEngine;

namespace ArRingTryOn.Rendering
{
    /// <summary>
    /// Places <see cref="ViewSpace"/> points in the world so they line up with the displayed
    /// camera image, for an orthographic camera showing that image filled to the screen.
    /// </summary>
    internal sealed class ViewSpaceMapping
    {
        // How far in front of the camera view space's origin sits. It only has to stay nearer
        // than the camera feed; the orthographic camera shows things the same size at any depth.
        private const float Distance = 25f;

        private readonly Camera _camera;

        public ViewSpaceMapping(Camera camera)
        {
            _camera = camera;
        }

        /// <summary>World units per display width; zero until configured.</summary>
        public float UnitsPerDisplayWidth { get; private set; }

        public void Configure(CameraView view)
        {
            var screenHeight = _camera.pixelHeight;
            var fill = DisplayFit.FillScale(view.DisplayWidth, view.DisplayHeight, _camera.pixelWidth, screenHeight);
            var worldUnitsPerPixel = 2f * _camera.orthographicSize / screenHeight;
            UnitsPerDisplayWidth = view.DisplayWidth * fill * worldUnitsPerPixel;
        }

        public Vector3 ToWorld(Vec3 point)
        {
            var units = UnitsPerDisplayWidth;
            return _camera.transform.TransformPoint(point.X * units, point.Y * units, Distance + point.Z * units);
        }

        public Quaternion ToWorld(Quat rotation)
        {
            return _camera.transform.rotation * new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
        }
    }
}
