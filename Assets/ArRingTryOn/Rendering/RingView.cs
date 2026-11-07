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

        private readonly Dictionary<RingStyle, Mesh> _meshes = new Dictionary<RingStyle, Mesh>();
        private ViewSpaceMapping _mapping;
        private MeshFilter _filter;
        private MeshRenderer _renderer;

        private void Awake()
        {
            _mapping = new ViewSpaceMapping(_camera);
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            _renderer.enabled = false;

            // Every style is built once up front, so switching rings costs nothing per frame.
            foreach (RingStyle style in Enum.GetValues(typeof(RingStyle)))
            {
                _meshes[style] = RingModelFactory.Build(style);
            }

            SetStyle(RingStyle.PlainBand);
        }

        public void SetStyle(RingStyle style)
        {
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

        private void OnDestroy()
        {
            foreach (var mesh in _meshes.Values)
            {
                Destroy(mesh);
            }
        }

        [Serializable]
        private struct StyleMaterials
        {
            public RingStyle Style;
            public Material[] Materials;
        }
    }
}
