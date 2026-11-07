using ArRingTryOn.Core;
using UnityEngine;

namespace ArRingTryOn.Rendering
{
    /// <summary>
    /// An invisible stand-in for the ring finger. Drawn depth-only before the ring, it hides the
    /// part of the ring that is behind the finger.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class FingerOccluderView : MonoBehaviour
    {
        private const int Sides = 16;

        [SerializeField] private Camera _camera;

        private ViewSpaceMapping _mapping;
        private FingerTubeMesh _tube;
        private MeshRenderer _renderer;

        private void Awake()
        {
            _mapping = new ViewSpaceMapping(_camera);
            _tube = new FingerTubeMesh(FingerTubeCalculator.CentreCount, Sides);
            GetComponent<MeshFilter>().sharedMesh = _tube.Mesh;
            _renderer = GetComponent<MeshRenderer>();
            _renderer.enabled = false;

            // Vertices are written in world space.
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
        }

        public void Configure(CameraView view)
        {
            _mapping.Configure(view);
        }

        public void Show(FingerTube tube)
        {
            var units = _mapping.UnitsPerDisplayWidth;
            for (var i = 0; i < tube.Centres.Count; i++)
            {
                _tube.SetCentre(i, _mapping.ToWorld(tube.Centres[i]), tube.Radii[i] * units);
            }

            _tube.Apply();
            _renderer.enabled = true;
        }

        public void Hide()
        {
            _renderer.enabled = false;
        }

        private void OnDestroy()
        {
            _tube?.Dispose();
        }
    }
}
