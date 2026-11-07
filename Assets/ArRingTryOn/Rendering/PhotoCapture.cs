using UnityEngine;

namespace ArRingTryOn.Rendering
{
    /// <summary>
    /// Renders the camera feed and the ring into a photo at screen resolution. UI lives on screen
    /// space overlay canvases, which cameras never draw, so the debug skeleton and every button
    /// stay out of the picture without being hidden from the user.
    /// </summary>
    public sealed class PhotoCapture : MonoBehaviour
    {
        [Tooltip("The base camera. Its camera stack (the ring) is rendered with it.")]
        [SerializeField] private Camera _camera;
        [SerializeField, Range(1, 100)] private int _jpegQuality = 95;

        /// <returns>The photo as JPEG bytes, or null if it could not be rendered.</returns>
        public byte[] CaptureJpeg()
        {
            var width = _camera.pixelWidth;
            var height = _camera.pixelHeight;
            if (width <= 0 || height <= 0)
            {
                return null;
            }

            // An sRGB target, so the linear-space render is stored as the colours seen on screen.
            var target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var previousTarget = _camera.targetTexture;
            var previousActive = RenderTexture.active;
            var photo = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                _camera.targetTexture = target;
                _camera.Render();

                RenderTexture.active = target;
                photo.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                return photo.EncodeToJPG(_jpegQuality);
            }
            finally
            {
                _camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(target);
                Destroy(photo);
            }
        }
    }
}
