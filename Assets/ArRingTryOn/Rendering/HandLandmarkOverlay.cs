using ArRingTryOn.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ArRingTryOn.Rendering
{
    /// <summary>Debug overlay that draws the hand skeleton as a single UI mesh.</summary>
    public sealed class HandLandmarkOverlay : MaskableGraphic
    {
        private const int PointSides = 8;

        [SerializeField] private float _pointRadius = 14f;
        [SerializeField] private float _lineWidth = 6f;
        [SerializeField] private Color _pointColor = new Color(1f, 0.25f, 0.35f);
        [SerializeField] private Color _lineColor = new Color(0.2f, 1f, 0.5f);

        private HandFrame _frame;

        /// <summary>Lines the overlay up with the displayed camera image.</summary>
        public void Configure(CameraView view)
        {
            var scale = ViewportFit.Centre(rectTransform, view);
            rectTransform.sizeDelta = new Vector2(view.DisplayWidth * scale, view.DisplayHeight * scale);
        }

        public void Show(HandFrame frame)
        {
            _frame = frame;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertices)
        {
            vertices.Clear();
            if (_frame == null)
            {
                return;
            }

            var bounds = rectTransform.rect;
            foreach (var hand in _frame.Hands)
            {
                foreach (var (from, to) in HandTopology.Connections)
                {
                    AddLine(vertices, ToLocal(hand[from], bounds), ToLocal(hand[to], bounds));
                }

                for (var id = 0; id < HandLandmarks.Count; id++)
                {
                    AddPoint(vertices, ToLocal(hand[(HandLandmarkId)id], bounds));
                }
            }
        }

        private static Vector2 ToLocal(Vec3 point, Rect bounds)
        {
            return new Vector2(bounds.xMin + point.X * bounds.width, bounds.yMax - point.Y * bounds.height);
        }

        private void AddLine(VertexHelper vertices, Vector2 from, Vector2 to)
        {
            var along = to - from;
            if (along.sqrMagnitude < Mathf.Epsilon)
            {
                return;
            }

            var offset = new Vector2(-along.y, along.x).normalized * (_lineWidth * 0.5f);
            var first = vertices.currentVertCount;
            vertices.AddVert(from + offset, _lineColor, Vector4.zero);
            vertices.AddVert(from - offset, _lineColor, Vector4.zero);
            vertices.AddVert(to - offset, _lineColor, Vector4.zero);
            vertices.AddVert(to + offset, _lineColor, Vector4.zero);
            vertices.AddTriangle(first, first + 1, first + 2);
            vertices.AddTriangle(first, first + 2, first + 3);
        }

        private void AddPoint(VertexHelper vertices, Vector2 centre)
        {
            var first = vertices.currentVertCount;
            vertices.AddVert(centre, _pointColor, Vector4.zero);

            for (var i = 0; i < PointSides; i++)
            {
                var angle = i * Mathf.PI * 2f / PointSides;
                var edge = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _pointRadius;
                vertices.AddVert(centre + edge, _pointColor, Vector4.zero);
                vertices.AddTriangle(first, first + 1 + i, first + 1 + (i + 1) % PointSides);
            }
        }
    }
}
