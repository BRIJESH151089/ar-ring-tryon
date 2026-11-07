using System.Linq;
using System.Xml;
using ArRingTryOn.Editor;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class StoragePermissionLimiterTests
    {
        private const string Android = StoragePermissionLimiter.AndroidNamespace;
        private const string Tools = StoragePermissionLimiter.ToolsNamespace;

        private static XmlDocument Manifest(string body)
        {
            var document = new XmlDocument();
            document.LoadXml(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                $"<manifest xmlns:android=\"{Android}\">{body}<application android:label=\"App\" /></manifest>");
            return document;
        }

        private static XmlElement[] Permissions(XmlDocument document, string name)
        {
            return document.DocumentElement.ChildNodes.OfType<XmlElement>()
                .Where(e => e.Name == "uses-permission" && e.GetAttribute("name", Android) == name)
                .ToArray();
        }

        [Test]
        public void AddsBothStoragePermissionsLimitedToAndroidNine()
        {
            var document = Manifest("<uses-permission android:name=\"android.permission.CAMERA\" />");

            StoragePermissionLimiter.Apply(document);

            foreach (var permission in StoragePermissionLimiter.Permissions)
            {
                var found = Permissions(document, permission);
                Assert.That(found.Length, Is.EqualTo(1), permission);
                Assert.That(found[0].GetAttribute("maxSdkVersion", Android), Is.EqualTo("28"));
                Assert.That(found[0].GetAttribute("node", Tools), Is.EqualTo("replace"));
            }
        }

        [Test]
        public void LimitsAnExistingDeclarationInsteadOfDuplicatingIt()
        {
            var document = Manifest("<uses-permission android:name=\"android.permission.WRITE_EXTERNAL_STORAGE\" />");

            StoragePermissionLimiter.Apply(document);

            var write = Permissions(document, "android.permission.WRITE_EXTERNAL_STORAGE");
            Assert.That(write.Length, Is.EqualTo(1));
            Assert.That(write[0].GetAttribute("maxSdkVersion", Android), Is.EqualTo("28"));
        }

        [Test]
        public void RunningTwice_ChangesNothingMore()
        {
            var document = Manifest(string.Empty);
            StoragePermissionLimiter.Apply(document);
            var once = document.OuterXml;

            StoragePermissionLimiter.Apply(document);

            Assert.That(document.OuterXml, Is.EqualTo(once));
        }

        [Test]
        public void DeclaresTheToolsNamespace()
        {
            var document = Manifest(string.Empty);

            StoragePermissionLimiter.Apply(document);

            Assert.That(document.DocumentElement.GetAttribute("xmlns:tools"), Is.EqualTo(Tools));
        }

        [Test]
        public void LeavesOtherPermissionsAndTheApplicationAlone()
        {
            var document = Manifest("<uses-permission android:name=\"android.permission.CAMERA\" />");

            StoragePermissionLimiter.Apply(document);

            var camera = Permissions(document, "android.permission.CAMERA");
            Assert.That(camera.Length, Is.EqualTo(1));
            Assert.That(camera[0].HasAttribute("maxSdkVersion", Android), Is.False);
            Assert.That(document.DocumentElement.LastChild.Name, Is.EqualTo("application"), "permissions go before <application>");
        }

        [Test]
        public void RejectsSomethingThatIsNotAManifest()
        {
            var document = new XmlDocument();
            document.LoadXml("<resources />");

            Assert.Throws<XmlException>(() => StoragePermissionLimiter.Apply(document));
        }
    }
}
