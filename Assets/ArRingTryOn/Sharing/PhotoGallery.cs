using System;
using ArRingTryOn.Core;

namespace ArRingTryOn.Sharing
{
    /// <summary>
    /// Saves photos to the device gallery. The only code that knows about NativeGallery, so it
    /// can be replaced without touching the rest of the app. Callbacks arrive on the main thread.
    /// </summary>
    public static class PhotoGallery
    {
        public static void RequestPermission(Action<GalleryPermission> onResult)
        {
            NativeGallery.RequestPermissionAsync(
                permission => onResult(ToCore(permission)),
                NativeGallery.PermissionType.Write,
                NativeGallery.MediaType.Image);
        }

        public static void Save(byte[] photo, string album, string fileName, Action<bool> onSaved)
        {
            NativeGallery.SaveImageToGallery(photo, album, fileName, (success, _) => onSaved(success));
        }

        public static void OpenSettings()
        {
            NativeGallery.OpenSettings();
        }

        private static GalleryPermission ToCore(NativeGallery.Permission permission)
        {
            switch (permission)
            {
                case NativeGallery.Permission.Granted: return GalleryPermission.Granted;
                case NativeGallery.Permission.ShouldAsk: return GalleryPermission.ShouldAsk;
                default: return GalleryPermission.Denied;
            }
        }
    }
}
