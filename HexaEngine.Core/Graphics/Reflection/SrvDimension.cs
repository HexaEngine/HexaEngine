namespace HexaEngine.Core.Graphics.Reflection
{
    /// <summary>
    /// Represents the dimension of a shader resource view (SRV).
    /// </summary>
    public enum SrvDimension
    {
        /// <summary>
        /// Unknown dimension.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Buffer dimension.
        /// </summary>
        Buffer = 1,

        /// <summary>
        /// 1D texture dimension.
        /// </summary>
        Texture1D = 2,

        /// <summary>
        /// 1D texture array dimension.
        /// </summary>
        Texture1DArray = 3,

        /// <summary>
        /// 2D texture dimension.
        /// </summary>
        Texture2D = 4,

        /// <summary>
        /// 2D texture array dimension.
        /// </summary>
        Texture2DArray = 5,

        /// <summary>
        /// 2D multisampled texture dimension.
        /// </summary>
        Texture2DMS = 6,

        /// <summary>
        /// 2D multisampled texture array dimension.
        /// </summary>
        Texture2DMSArray = 7,

        /// <summary>
        /// 3D texture dimension.
        /// </summary>
        Texture3D = 8,

        /// <summary>
        /// Cube texture dimension.
        /// </summary>
        TextureCube = 9,

        /// <summary>
        /// Cube texture array dimension.
        /// </summary>
        TextureCubeArray = 10,

        /// <summary>
        /// Extended buffer dimension.
        /// </summary>
        BufferEx = 11,
    }
}