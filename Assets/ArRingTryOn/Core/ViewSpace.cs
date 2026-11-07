namespace ArRingTryOn.Core
{
    /// <summary>
    /// The space ring placement works in: origin at the display centre, x right, y up, z away
    /// from the camera, with every axis measured in display widths so lengths and angles are true.
    /// </summary>
    public static class ViewSpace
    {
        /// <summary>Converts a display space point (see <see cref="HandLandmarks"/>).</summary>
        public static Vec3 FromDisplay(Vec3 point, float displayAspect)
        {
            return new Vec3(point.X - 0.5f, (0.5f - point.Y) / displayAspect, point.Z);
        }
    }
}
