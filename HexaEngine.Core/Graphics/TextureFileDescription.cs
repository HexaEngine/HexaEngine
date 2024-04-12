namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Describes a texture file.
    /// </summary>
    public struct TextureFileDescription : IEquatable<TextureFileDescription>
    {
        /// <summary>
        /// The path to the texture file.
        /// </summary>
        public string Path;

        /// <summary>
        /// The dimension of the texture.
        /// </summary>
        public TextureDimension Dimension;

        /// <summary>
        /// The number of mip levels.
        /// </summary>
        public int MipLevels;

        /// <summary>
        /// The usage of the texture.
        /// </summary>
        public Usage Usage;

        /// <summary>
        /// The bind flags of the texture.
        /// </summary>
        public BindFlags BindFlags;

        /// <summary>
        /// The CPU access flags of the texture.
        /// </summary>
        public CpuAccessFlags CPUAccessFlags;

        /// <summary>
        /// Indicates whether to force SRGB.
        /// </summary>
        public bool ForceSRGB;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureFileDescription"/> struct.
        /// </summary>
        /// <param name="path">The path to the texture file.</param>
        /// <param name="dimension">The dimension of the texture.</param>
        /// <param name="mipLevels">The number of mip levels.</param>
        /// <param name="usage">The usage of the texture.</param>
        /// <param name="bindFlags">The bind flags of the texture.</param>
        /// <param name="cPUAccessFlags">The CPU access flags of the texture.</param>
        /// <param name="forceSRGB">Indicates whether to force SRGB.</param>
        public TextureFileDescription(
            string path,
            TextureDimension dimension = TextureDimension.Texture2D,
            int mipLevels = 0,
            Usage usage = Usage.Default,
            BindFlags bindFlags = BindFlags.ShaderResource,
            CpuAccessFlags cPUAccessFlags = CpuAccessFlags.None,
            bool forceSRGB = false
            )
        {
            Path = path;
            Dimension = dimension;
            MipLevels = mipLevels;
            Usage = usage;
            BindFlags = bindFlags;
            CPUAccessFlags = cPUAccessFlags;
            ForceSRGB = forceSRGB;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is TextureFileDescription description && Equals(description);
        }

        public readonly bool Equals(TextureFileDescription other)
        {
            return Path == other.Path &&
                   Dimension == other.Dimension &&
                   MipLevels == other.MipLevels &&
                   Usage == other.Usage &&
                   BindFlags == other.BindFlags &&
                   CPUAccessFlags == other.CPUAccessFlags &&
                   ForceSRGB == other.ForceSRGB;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Path, Dimension, MipLevels, Usage, BindFlags, CPUAccessFlags, ForceSRGB);
        }

        public static bool operator ==(TextureFileDescription left, TextureFileDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextureFileDescription left, TextureFileDescription right)
        {
            return !(left == right);
        }
    }
}