namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies the dimension of a graphics resource.
    /// </summary>
    public enum ResourceDimension : int
    {
        /// <summary>
        /// The dimension of the resource is unknown.
        /// </summary>
        Unknown = unchecked(0),

        /// <summary>
        /// The resource has a buffer dimension.
        /// </summary>
        Buffer = unchecked(1),

        /// <summary>
        /// The resource has a 1D texture dimension.
        /// </summary>
        Texture1D = unchecked(2),

        /// <summary>
        /// The resource has a 2D texture dimension.
        /// </summary>
        Texture2D = unchecked(3),

        /// <summary>
        /// The resource has a 3D texture dimension.
        /// </summary>
        Texture3D = unchecked(4)
    }
}