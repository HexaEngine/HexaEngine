namespace HexaEngine.Core.Graphics
{
    using System.Xml.Serialization;

    /// <summary>
    /// Represents the description of a 3D texture.
    /// </summary>
    public struct Texture3DDescription : IEquatable<Texture3DDescription>
    {
        /// <summary>
        /// The width of the texture.
        /// </summary>
        [XmlAttribute]
        public int Width;

        /// <summary>
        /// The height of the texture.
        /// </summary>
        [XmlAttribute]
        public int Height;

        /// <summary>
        /// The depth of the texture.
        /// </summary>
        [XmlAttribute]
        public int Depth;

        /// <summary>
        /// The number of mip levels in the texture.
        /// </summary>
        [XmlAttribute]
        public int MipLevels;

        /// <summary>
        /// The format of the texture.
        /// </summary>
        [XmlAttribute]
        public Format Format;

        /// <summary>
        /// The intended usage of the texture.
        /// </summary>
        [XmlAttribute]
        public Usage Usage;

        /// <summary>
        /// The bind flags specifying how the texture will be bound to the pipeline.
        /// </summary>
        [XmlAttribute]
        public BindFlags BindFlags;

        /// <summary>
        /// The CPU access flags specifying how the CPU will access the texture.
        /// </summary>
        [XmlAttribute]
        public CpuAccessFlags CPUAccessFlags;

        /// <summary>
        /// Miscellaneous flags for the resource.
        /// </summary>
        [XmlAttribute]
        public ResourceMiscFlag MiscFlags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture3DDescription"/> struct.
        /// </summary>
        /// <param name="width">Texture width (in texels).</param>
        /// <param name="height">Texture height (in texels).</param>
        /// <param name="depth">Texture depth (in texels).</param>
        /// <param name="format">Texture format.</param>
        /// <param name="mipLevels">The maximum number of mipmap levels in the texture.</param>
        /// <param name="bindFlags">The <see cref="BindFlags"/> for binding to pipeline stages.</param>
        /// <param name="usage">Value that identifies how the texture is to be read from and written to.</param>
        /// <param name="cpuAccessFlags">The <see cref="CpuAccessFlags"/> to specify the types of CPU access allowed.</param>
        /// <param name="miscFlags">The <see cref="ResourceMiscFlag"/> that identify other, less common resource options. </param>
        public Texture3DDescription(
            Format format,
            int width,
            int height,
            int depth,
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

            if (width < 1 || width > IResource.MaximumTexture3DSize)
            {
                throw new ArgumentException($"Width need to be in range 1-{IResource.MaximumTexture3DSize}", nameof(width));
            }

            if (height < 1 || height > IResource.MaximumTexture3DSize)
            {
                throw new ArgumentException($"Height need to be in range 1-{IResource.MaximumTexture3DSize}", nameof(height));
            }

            if (depth < 1 || depth > IResource.MaximumTexture3DSize)
            {
                throw new ArgumentException($"Depth need to be in range 1-{IResource.MaximumTexture3DSize}", nameof(depth));
            }

            Width = width;
            Height = height;
            Depth = depth;
            MipLevels = mipLevels;
            Format = format;
            Usage = usage;
            BindFlags = bindFlags;
            CPUAccessFlags = cpuAccessFlags;
            MiscFlags = miscFlags;
        }

        public Texture3DDescription(
         Format format,
         int width,
         int height,
         int depth,
         int mipLevels = 0,
         GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read,
         CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None,
         ResourceMiscFlag miscFlags = ResourceMiscFlag.None)
        {
            if (format == Format.Unknown)
            {
                throw new ArgumentException($"format need to be valid", nameof(format));
            }

            if (width < 1 || width > IResource.MaximumTexture3DSize)
            {
                throw new ArgumentException($"Width need to be in range 1-{IResource.MaximumTexture3DSize}", nameof(width));
            }

            if (height < 1 || height > IResource.MaximumTexture3DSize)
            {
                throw new ArgumentException($"Height need to be in range 1-{IResource.MaximumTexture3DSize}", nameof(height));
            }

            if (depth < 1 || depth > IResource.MaximumTexture3DSize)
            {
                throw new ArgumentException($"Depth need to be in range 1-{IResource.MaximumTexture3DSize}", nameof(depth));
            }

            Width = width;
            Height = height;
            Depth = depth;
            MipLevels = mipLevels;
            Format = format;
            AccessHelper.Convert(cpuAccessFlags, gpuAccessFlags, out Usage, out BindFlags);
            CPUAccessFlags = cpuAccessFlags;
            MiscFlags = miscFlags;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture3DDescription description && Equals(description);
        }

        public readonly bool Equals(Texture3DDescription other)
        {
            return Width == other.Width &&
                   Height == other.Height &&
                   Depth == other.Depth &&
                   MipLevels == other.MipLevels &&
                   Format == other.Format &&
                   Usage == other.Usage &&
                   BindFlags == other.BindFlags &&
                   CPUAccessFlags == other.CPUAccessFlags &&
                   MiscFlags == other.MiscFlags;
        }

        public override readonly int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Width);
            hash.Add(Height);
            hash.Add(Depth);
            hash.Add(MipLevels);
            hash.Add(Format);
            hash.Add(Usage);
            hash.Add(BindFlags);
            hash.Add(CPUAccessFlags);
            hash.Add(MiscFlags);
            return hash.ToHashCode();
        }

        public static bool operator ==(Texture3DDescription left, Texture3DDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture3DDescription left, Texture3DDescription right)
        {
            return !(left == right);
        }
    }
}