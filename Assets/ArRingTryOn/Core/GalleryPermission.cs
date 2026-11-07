namespace ArRingTryOn.Core
{
    public enum GalleryPermission
    {
        Granted,

        /// <summary>Refused for good; only the system settings can change it.</summary>
        Denied,

        /// <summary>Refused this time; the app may ask again.</summary>
        ShouldAsk,
    }
}
