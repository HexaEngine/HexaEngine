namespace HexaEngine.Core.Graphics
{
    using System.Xml.Serialization;

    /// <summary>
    /// Represents the description of a 1D texture.
    /// </summary>
    public struct Texture1DDescription : IEquatable<Texture1DDescription>
    {
        /// <summary>
        /// The width of the texture.
        /// </summary>
        [XmlAttribute]
        public int Width;

        /// <summary>
        /// The number of mip levels in the texture.
        /// </summary>
        [XmlAttribute]
        public int MipLevels;

        /// <summary>
        /// The number of textures in the array.
        /// </summary>
        [XmlAttribute]
        public int ArraySize;

        /// <summary>
        /// The format of the texture.
        /// </summary>
        [XmlAttribute]
        public Format Format;

        /// <summary>
        /// Specifies how the texture will be used.
        /// </summary>
        [XmlAttribute]
        public Usage Usage;

        /// <summary>
        /// Flags that specify how the texture can be bound to the pipeline.
        /// </summary>
        [XmlAttribute]
        public BindFlags BindFlags;

        /// <summary>
        /// Flags that specify how the CPU can access the resource.
        /// </summary>
        [XmlAttribute]
        public CpuAccessFlags CPUAccessFlags;

        /// <summary>
        /// Miscellaneous flags for the resource.
        /// </summary>
        [XmlAttribute]
        public ResourceMiscFlag MiscFlags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1DDescription"/> struct.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <param name="width">Texture width (in texels).</param>
        /// <param name="arraySize">Number of textures in the array.</param>
        /// <param name="mipLevels">The maximum number of mipmap levels in the texture.</param>
        /// <param name="bindFlags">The <see cref="BindFlags"/> for binding to pipeline stages.</param>
        /// <param name="usage">Value that identifies how the texture is to be read from and written to.</param>
        /// <param name="cpuAccessFlags">The <see cref="CpuAccessFlags"/> to specify the types of CPU access allowed.</param>
        /// <param name="miscFlags">The <see cref="ResourceMiscFlag"/> that identify other, less common resource options. </param>
        public Texture1DDescription(
            Format format,
            int width,
            int arraySize = 1,
            int mipLevels = 0,
            BindFlags bindFlags = BindFlags.ShaderResource,
            Usage usage = Usage.Default,
            CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None,
            ResourceMiscFlag miscFlags = ResourceMiscFlag.None)
        {
            if (format == Format.Unknown)
            {
                throw new ArgumentException($"format need to be valid", nameof(format));
            }

            if (width < 1 || width > IResource.MaximumTexture1DSize)
            {
                throw new ArgumentException($"Width need to be in range 1-{IResource.MaximumTexture1DSize}", nameof(width));
            }

            if (arraySize < 1 || arraySize > IResource.MaximumTexture1DArraySize)
            {
                throw new ArgumentException($"Array size need to be in range 1-{IResource.MaximumTexture1DArraySize}", nameof(arraySize));
            }

            Width = width;
            MipLevels = mipLevels;
            ArraySize = arraySize;
            Format = format;
            Usage = usage;
            BindFlags = bindFlags;
            CPUAccessFlags = cpuAccessFlags;
            MiscFlags = miscFlags;
        }

        public Texture1DDescription(
            Format format,
            int width,
            int arraySize = 1,
            int mipLevels = 0,
            GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read,
            CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None,
            ResourceMiscFlag miscFlags = ResourceMiscFlag.None)
        {
            if (format == Format.Unknown)
            {
                throw new ArgumentException($"format need to be valid", nameof(format));
            }

            if (width < 1 || width > IResource.MaximumTexture1DSize)
            {
                throw new ArgumentException($"Width need to be in range 1-{IResource.MaximumTexture1DSize}", nameof(width));
            }

            if (arraySize < 1 || arraySize > IResource.MaximumTexture1DArraySize)
            {
                throw new ArgumentException($"Array size need to be in range 1-{IResource.MaximumTexture1DArraySize}", nameof(arraySize));
            }

            AccessHelper.Convert(cpuAccessFlags, gpuAccessFlags, out var usage, out var bindFlags);

            Width = width;
            MipLevels = mipLevels;
            ArraySize = arraySize;
            Format = format;
            Usage = usage;
            BindFlags = bindFlags;
            CPUAccessFlags = cpuAccessFlags;
            MiscFlags = miscFlags;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture1DDescription description && Equals(description);
        }

        public readonly bool Equals(Texture1DDescription other)
        {
            return Width == other.Width &&
                   MipLevels == other.MipLevels &&
                   ArraySize == other.ArraySize &&
                   Format == other.Format &&
                   Usage == other.Usage &&
                   BindFlags == other.BindFlags &&
                   CPUAccessFlags == other.CPUAccessFlags &&
                   MiscFlags == other.MiscFlags;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Width, MipLevels, ArraySize, Format, Usage, BindFlags, CPUAccessFlags, MiscFlags);
        }

        public static bool operator ==(Texture1DDescription left, Texture1DDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture1DDescription left, Texture1DDescription right)
        {
            return !(left == right);
        }
    }
}