namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents the dimension of an unordered access view.
    /// </summary>
    public enum UnorderedAccessViewDimension : int
    {
        /// <summary>
        /// Unknown dimension.
        /// </summary>
        Unknown = unchecked(0),

        /// <summary>
        /// Buffer dimension.
        /// </summary>
        Buffer = unchecked(1),

        /// <summary>
        /// 1D texture dimension.
        /// </summary>
        Texture1D = unchecked(2),

        /// <summary>
        /// 1D array texture dimension.
        /// </summary>
        Texture1DArray = unchecked(3),

        /// <summary>
        /// 2D texture dimension.
        /// </summary>
        Texture2D = unchecked(4),

        /// <summary>
        /// 2D array texture dimension.
        /// </summary>
        Texture2DArray = unchecked(5),

        /// <summary>
        /// 3D texture dimension.
        /// </summary>
        Texture3D = unchecked(8)
    }
}