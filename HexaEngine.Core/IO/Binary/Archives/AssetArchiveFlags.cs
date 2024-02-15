namespace HexaEngine.Core.IO.Binary.Archives
{
    /// <summary>
    /// Flags indicating the type of asset archive.
    /// </summary>
    public enum AssetArchiveFlags
    {
        /// <summary>
        /// Normal asset archive.
        /// </summary>
        Normal,

        /// <summary>
        /// Partial asset archive.
        /// </summary>
        Partial,

        /// <summary>
        /// Partial part of an asset archive.
        /// </summary>
        PartialPart,
    }
}