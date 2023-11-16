namespace HexaEngine.Volumes
{
    using HexaEngine.Mathematics;

    /// <summary>
    /// Represents a Volume in 3D space and controls post-processing and weather effects.
    /// </summary>
    public class Volume
    {
        /// <summary>
        /// Gets or sets the Volume mode.
        /// </summary>
        public VolumeMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the bounding box of the Volume.
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Gets or sets the bounding sphere of the Volume.
        /// </summary>
        public BoundingSphere BoundingSphere { get; set; }
    }
}