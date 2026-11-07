using System.IO;

namespace ArRingTryOn.Sharing
{
    /// <summary>Opens the system share sheet for a photo. The only code that knows about NativeShare.</summary>
    public static class PhotoSharer
    {
        /// <param name="filePath">An existing image file, such as one in the app's temporary cache.</param>
        /// <param name="mimeType">The image's type, for example "image/jpeg".</param>
        public static bool Share(string filePath, string mimeType)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            new NativeShare()
                .AddFile(filePath, mimeType)
                .SetSubject("My ring")
                .Share();
            return true;
        }
    }
}
