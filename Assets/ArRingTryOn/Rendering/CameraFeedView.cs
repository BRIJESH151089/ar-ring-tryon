using ArRingTryOn.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ArRingTryOn.Rendering
{
    /// <summary>Shows the camera texture upright, filling its parent.</summary>
    [RequireComponent(typeof(RawImage))]
    public sealed class CameraFeedView : MonoBehaviour
    {
        public void Show(Texture texture, CameraView view)
        {
            var image = GetComponent<RawImage>();
            image.texture = texture;
            image.uvRect = BuildUvRect(view);
            image.enabled = true;

            var rectTransform = image.rectTransform;
            var scale = ViewportFit.Centre(rectTransform, view);
            rectTransform.sizeDelta = new Vector2(view.ImageWidth * scale, view.ImageHeight * scale);
            rectTransform.localEulerAngles = new Vector3(0f, 0f, -view.RotationDegrees);
        }

        private static Rect BuildUvRect(CameraView view)
        {
            var x = view.FlipTextureU ? 1f : 0f;
            var y = view.FlipTextureV ? 1f : 0f;
            var width = view.FlipTextureU ? -1f : 1f;
            var height = view.FlipTextureV ? -1f : 1f;
            return new Rect(x, y, width, height);
        }
    }
}
