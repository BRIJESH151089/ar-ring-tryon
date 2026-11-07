using System;
using System.Globalization;

namespace ArRingTryOn.Core
{
    /// <summary>Where saved photos go and what they are called.</summary>
    public static class PhotoNaming
    {
        public const string Album = "AR Ring Try-On";
        public const string Extension = ".jpg";
        public const string MimeType = "image/jpeg";

        /// <summary>A file name from the capture time, down to the millisecond so quick shots never collide.</summary>
        public static string FileName(DateTime capturedAt)
        {
            return "ARRingTryOn_" + capturedAt.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture) + Extension;
        }
    }
}
