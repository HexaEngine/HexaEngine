namespace HexaEngine.Core.IO
{
    /// <summary>
    /// Specifies the compression algorithm used for data compression.
    /// </summary>
    public enum Compression
    {
        /// <summary>
        /// No compression.
        /// </summary>
        None = 0,

        /// <summary>
        /// Deflate compression algorithm.
        /// </summary>
        Deflate,

        /// <summary>
        /// LZ4 compression algorithm.
        /// </summary>
        LZ4,
    }
}