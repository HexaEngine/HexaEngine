namespace HexaEngine.Core.IO.Assets
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