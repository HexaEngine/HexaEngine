namespace HexaEngine.Core.IO.Binary.Materials
{
    /// <summary>
    /// Represents different blend modes for textures.
    /// </summary>
    [Flags]
    public enum BlendMode
    {
        /// <summary>
        /// The default blending mode, where colors are blended based on alpha values.
        /// </summary>
        Default = 0x0,

        /// <summary>
        /// Additive blending mode, where colors of the source and destination are added together.
        /// Used for effects like light glows or particle systems.
        /// </summary>
        Additive = 0x1
    }
}