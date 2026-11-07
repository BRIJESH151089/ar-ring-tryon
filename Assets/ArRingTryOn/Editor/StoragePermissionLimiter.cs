using System.Xml;

namespace ArRingTryOn.Editor
{
    /// <summary>
    /// Limits the storage permissions NativeGallery declares to Android 9 and below.
    /// </summary>
    /// <remarks>
    /// NativeGallery needs WRITE and READ_EXTERNAL_STORAGE to save a photo on Android 6 to 9; from
    /// Android 10 it saves through MediaStore and checks neither. Declaring them with
    /// android:maxSdkVersion="28" in the app's own manifest keeps saving working on old phones
    /// while newer phones never list them. tools:node="replace" makes this declaration win over
    /// the library's, which uses replace itself.
    /// </remarks>
    public static class StoragePermissionLimiter
    {
        public const string AndroidNamespace = "http://schemas.android.com/apk/res/android";
        public const string ToolsNamespace = "http://schemas.android.com/tools";
        public const string MaxSdkVersion = "28";

        public static readonly string[] Permissions =
        {
            "android.permission.WRITE_EXTERNAL_STORAGE",
            "android.permission.READ_EXTERNAL_STORAGE",
        };

        /// <summary>Adds or updates the limited declarations in a manifest document.</summary>
        public static void Apply(XmlDocument manifest)
        {
            var root = manifest.DocumentElement;
            if (root == null || root.Name != "manifest")
            {
                throw new XmlException("Not an Android manifest: the root element must be <manifest>.");
            }

            if (!root.HasAttribute("xmlns:tools"))
            {
                root.SetAttribute("xmlns:tools", ToolsNamespace);
            }

            foreach (var permission in Permissions)
            {
                var element = Find(root, permission);
                if (element == null)
                {
                    element = manifest.CreateElement("uses-permission");
                    element.SetAttribute("name", AndroidNamespace, permission);
                    root.InsertBefore(element, FirstApplication(root));
                }

                element.SetAttribute("maxSdkVersion", AndroidNamespace, MaxSdkVersion);
                element.SetAttribute("node", ToolsNamespace, "replace");
            }
        }

        private static XmlElement Find(XmlElement root, string permission)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node is XmlElement element && element.Name == "uses-permission"
                    && element.GetAttribute("name", AndroidNamespace) == permission)
                {
                    return element;
                }
            }

            return null;
        }

        // Permissions conventionally come before <application>; null appends at the end.
        private static XmlNode FirstApplication(XmlElement root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node is XmlElement element && element.Name == "application")
                {
                    return element;
                }
            }

            return null;
        }
    }
}
