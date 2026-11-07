using System.IO;
using System.Xml;
using UnityEditor.Android;

namespace ArRingTryOn.Editor
{
    /// <summary>Applies <see cref="StoragePermissionLimiter"/> to every Android build.</summary>
    internal sealed class LimitStoragePermissionsOnBuild : IPostGenerateGradleAndroidProject
    {
        // Run after other post-processors so nothing re-adds unlimited declarations afterwards.
        public int callbackOrder => 1000;

        /// <param name="path">The generated unityLibrary module, whose manifest the app's AARs merge under.</param>
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            var manifest = new XmlDocument { PreserveWhitespace = true };
            manifest.Load(manifestPath);
            StoragePermissionLimiter.Apply(manifest);
            manifest.Save(manifestPath);
        }
    }
}
