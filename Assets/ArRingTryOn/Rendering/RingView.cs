using ArRingTryOn.Core;
using UnityEngine;

namespace ArRingTryOn.Rendering
{
    /// <summary>Draws the placeholder ring at the pose Core produces.</summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RingView : MonoBehaviour
    {
        // How far in front of the camera the ring sits. It only has to stay nearer than the
        // camera feed, because the orthographic camera shows it the same size at any depth.
        private const float RingDistance = 25f;

        [SerializeField] private Camera _camera;
        [SerializeField] private float _majorRadius = 0.56f;
        [SerializeField] private float _tubeRadius = 0.06f;
        [SerializeField] private int _ringSegments = 64;
        [SerializeField] private int _tubeSegments = 16;

        private MeshRenderer _renderer;
        private Mesh _mesh;
        private float _worldUnitsPerDisplayWidth;

        private void Awake()
        {
            _mesh = TorusMesh.Build(_majorRadius, _tubeRadius, _ringSegments, _tubeSegments);
            GetComponent<MeshFilter>().sharedMesh = _mesh;
            _renderer = GetComponent<MeshRenderer>();
            _renderer.enabled = false;
        }

        /// <summary>Matches the ring's placement to the displayed camera image.</summary>
        public void Configure(CameraView view)
        {
            var screenHeight = _camera.pixelHeight;
            var fill = DisplayFit.FillScale(view.DisplayWidth, view.DisplayHeight, _camera.pixelWidth, screenHeight);
            var worldUnitsPerPixel = 2f * _camera.orthographicSize / screenHeight;
            _worldUnitsPerDisplayWidth = view.DisplayWidth * fill * worldUnitsPerPixel;
        }

        public void Show(RingPose pose)
        {
            var units = _worldUnitsPerDisplayWidth;
            var position = pose.Position;
            var rotation = pose.Rotation;
            var innerDiameter = 2f * (_majorRadius - _tubeRadius);

            var cameraTransform = _camera.transform;
            transform.SetPositionAndRotation(
                cameraTransform.TransformPoint(position.X * units, position.Y * units, RingDistance + position.Z * units),
                cameraTransform.rotation * new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W));
            transform.localScale = Vector3.one * (pose.Scale * units / innerDiameter);

            _renderer.enabled = true;
        }

        public void Hide()
        {
            _renderer.enabled = false;
        }

        private void OnDestroy()
        {
            Destroy(_mesh);
        }
    }
}
