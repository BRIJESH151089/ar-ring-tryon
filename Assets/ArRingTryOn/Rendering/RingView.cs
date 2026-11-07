using System;
using System.Collections.Generic;
using ArRingTryOn.Core;
using UnityEngine;

namespace ArRingTryOn.Rendering
{
    /// <summary>Draws the selected ring at the pose Core produces.</summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RingView : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private StyleMaterials[] _materials = Array.Empty<StyleMaterials>();

        [Tooltip("Band metal for every ring style. Changes apply live in Play mode.")]
        [SerializeField] private RingBandSettings _band = RingBandSettings.Default;

        private readonly Dictionary<RingStyle, Mesh> _meshes = new Dictionary<RingStyle, Mesh>();
        private ViewSpaceMapping _mapping;
        private MeshFilter _filter;
        private MeshRenderer _renderer;
        private RingStyle _style = RingStyle.PlainBand;

        private void Awake()
        {
            _mapping = new ViewSpaceMapping(_camera);
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            _renderer.enabled = false;

            BuildMeshes();
            SetStyle(RingStyle.PlainBand);
        }

        public void SetStyle(RingStyle style)
        {
            _style = style;
            var mesh = _meshes[style];
            var materials = MaterialsFor(style);
            if (materials.Length != mesh.subMeshCount)
            {
                Debug.LogError($"{style} ring has {mesh.subMeshCount} parts but {materials.Length} materials.", this);
            }

            _filter.sharedMesh = mesh;
            _renderer.sharedMaterials = materials;
        }

        /// <summary>Matches the ring's placement to the displayed camera image.</summary>
        public void Configure(CameraView view)
        {
            _mapping.Configure(view);
        }

        public void Show(RingPose pose)
        {
            transform.SetPositionAndRotation(_mapping.ToWorld(pose.Position), _mapping.ToWorld(pose.Rotation));
            var units = _mapping.UnitsPerDisplayWidth;
            transform.localScale = Vector3.one * (pose.Scale * units / (2f * RingModelFactory.InnerRadius));

            _renderer.enabled = true;
        }

        public void Hide()
        {
            _renderer.enabled = false;
        }

        private Material[] MaterialsFor(RingStyle style)
        {
            foreach (var entry in _materials)
            {
                if (entry.Style == style)
                {
                    return entry.Materials;
                }
            }

            return Array.Empty<Material>();
        }

        // Every style is built once up front, so switching rings costs nothing per frame.
        private void BuildMeshes()
        {
            DestroyMeshes();
            foreach (RingStyle style in Enum.GetValues(typeof(RingStyle)))
            {
                _meshes[style] = RingModelFactory.Build(style, _band);
            }
        }

        private void DestroyMeshes()
        {
            foreach (var mesh in _meshes.Values)
            {
                Destroy(mesh);
            }

            _meshes.Clear();
        }

        private void OnValidate()
        {
            // Rebuild when the band is tuned from the Inspector while the app runs.
            if (Application.isPlaying && _filter != null)
            {
                BuildMeshes();
                SetStyle(_style);
            }
        }

        private void OnDestroy()
        {
            DestroyMeshes();
        }

        [Serializable]
        private struct StyleMaterials
        {
            public RingStyle Style;
            public Material[] Materials;
        }
    }
}
