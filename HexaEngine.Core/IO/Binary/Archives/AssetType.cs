namespace HexaEngine.Core.IO.Binary.Archives
{
    /// <summary>
    /// Represents the type of an asset in the asset archive.
    /// </summary>
    public enum AssetType : ulong
    {
        /// <summary>
        /// Binary asset type.
        /// </summary>
        Binary = 0,

        /// <summary>
        /// Material library asset type.
        /// </summary>
        MaterialLibrary,

        /// <summary>
        /// Texture file asset type.
        /// </summary>
        TextureFile,

        /// <summary>
        /// Model file asset type.
        /// </summary>
        ModelFile,

        /// <summary>
        /// Font file asset type.
        /// </summary>
        FontFile,

        /// <summary>
        /// Audio file asset type.
        /// </summary>
        AudioFile,

        /// <summary>
        /// Shader source asset type.
        /// </summary>
        ShaderSource,

        /// <summary>
        /// Shader bytecode asset type.
        /// </summary>
        ShaderBytecode,
    }
}