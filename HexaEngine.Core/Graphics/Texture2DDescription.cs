namespace HexaEngine.Core.Graphics
{
    using System.Xml.Serialization;

    /// <summary>
    /// Represents the description of a 2D texture.
    /// </summary>
    public struct Texture2DDescription
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
        /// The number of mip levels in the texture.
        /// </summary>
        [XmlAttribute]
        public int MipLevels;

        /// <summary>
        /// The array size (number of textures in the array).
        /// </summary>
        [XmlAttribute]
        public int ArraySize;

        /// <summary>
        /// The format of the texture.
        /// </summary>
        [XmlAttribute]
        public Format Format;

        /// <summary>
        /// The multisampling parameters for the texture.
        /// </summary>
        public SampleDescription SampleDescription;

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
        /// The CPU access flags indicating how the CPU can access the texture.
        /// </summary>
        [XmlAttribute]
        public CpuAccessFlags CPUAccessFlags;

        /// <summary>
        /// Miscellaneous flags for the texture.
        /// </summary>
        [XmlAttribute]
        public ResourceMiscFlag MiscFlags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2DDescription"/> struct with default values.
        /// </summary>
        public Texture2DDescription()
        {
            SampleDescription = SampleDescription.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2DDescription"/> struct.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <param name="width">Texture width (in texels).</param>
        /// <param name="height">Texture height (in texels).</param>
        /// <param name="arraySize">Number of textures in the array.</param>
        /// <param name="mipLevels">The maximum number of mipmap levels in the texture.</param>
        /// <param name="bindFlags">The <see cref="BindFlags"/> for binding to pipeline stages.</param>
        /// <param name="usage">Value that identifies how the texture is to be read from and written to.</param>
        /// <param name="cpuAccessFlags">The <see cref="CpuAccessFlags"/> to specify the types of CPU access allowed.</param>
        /// <param name="sampleCount">Specifies multisampling parameters for the texture.</param>
        /// <param name="sampleQuality">Specifies multisampling parameters for the texture.</param>
        /// <param name="miscFlags">The <see cref="ResourceMiscFlag"/> that identify other, less common resource options. </param>
        public Texture2DDescription(
            Format format,
            int width,
            int height,
            int arraySize = 1,
            int mipLevels = 0,
            BindFlags bindFlags = BindFlags.ShaderResource,
            Usage usage = Usage.Default,
            CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None,
            int sampleCount = 1,
            int sampleQuality = 0,
            ResourceMiscFlag miscFlags = ResourceMiscFlag.None)
        {
            if (format == Format.Unknown)
            {
                throw new ArgumentException($"format need to be valid", nameof(format));
            }

            if (width < 1 || width > IResource.MaximumTexture2DSize)
            {
                throw new ArgumentException($"Width need to be in range 1-{IResource.MaximumTexture2DSize}", nameof(width));
            }

            if (height < 1 || height > IResource.MaximumTexture2DSize)
            {
                throw new ArgumentException($"Height need to be in range 1-{IResource.MaximumTexture2DSize}", nameof(height));
            }

            if (arraySize < 1 || arraySize > IResource.MaximumTexture2DArraySize)
            {
                throw new ArgumentException($"Array size need to be in range 1-{IResource.MaximumTexture2DArraySize}", nameof(arraySize));
            }

            Width = width;
            Height = height;
            MipLevels = mipLevels;
            ArraySize = arraySize;
            Format = format;
            SampleDescription = new(sampleCount, sampleQuality);
            Usage = usage;
            BindFlags = bindFlags;
            CPUAccessFlags = cpuAccessFlags;
            MiscFlags = miscFlags;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2DDescription"/> struct.
        /// </summary>
        /// <param name="format">Texture format.</param>
        /// <param name="width">Texture width (in texels).</param>
        /// <param name="height">Texture height (in texels).</param>
        /// <param name="arraySize">Number of textures in the array.</param>
        /// <param name="mipLevels">The maximum number of mipmap levels in the texture.</param>
        /// <param name="gpuAccessFlags">The <see cref="GpuAccessFlags"/> for binding to pipeline stages.</param>
        /// <param name="cpuAccessFlags">The <see cref="CpuAccessFlags"/> to specify the types of CPU access allowed.</param>
        /// <param name="sampleCount">Specifies multisampling parameters for the texture.</param>
        /// <param name="sampleQuality">Specifies multisampling parameters for the texture.</param>
        /// <param name="miscFlags">The <see cref="ResourceMiscFlag"/> that identify other, less common resource options. </param>
        public Texture2DDescription(
          Format format,
          int width,
          int height,
          int arraySize = 1,
          int mipLevels = 0,
          GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read,
          CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None,
          int sampleCount = 1,
          int sampleQuality = 0,
          ResourceMiscFlag miscFlags = ResourceMiscFlag.None)
        {
            if (format == Format.Unknown)
            {
                throw new ArgumentException($"format need to be valid", nameof(format));
            }

            if (width < 1 || width > IResource.MaximumTexture2DSize)
            {
                throw new ArgumentException($"Width need to be in range 1-{IResource.MaximumTexture2DSize}", nameof(width));
            }

            if (height < 1 || height > IResource.MaximumTexture2DSize)
            {
                throw new ArgumentException($"Height need to be in range 1-{IResource.MaximumTexture2DSize}", nameof(height));
            }

            if (arraySize < 1 || arraySize > IResource.MaximumTexture2DArraySize)
            {
                throw new ArgumentException($"Array size need to be in range 1-{IResource.MaximumTexture2DArraySize}", nameof(arraySize));
            }

            Width = width;
            Height = height;
            MipLevels = mipLevels;
            ArraySize = arraySize;
            Format = format;
            SampleDescription = new(sampleCount, sampleQuality);
            TextureHelper.Convert(cpuAccessFlags, gpuAccessFlags, out Usage, out BindFlags);
            CPUAccessFlags = cpuAccessFlags;
            MiscFlags = miscFlags;
        }

        /// <summary>
        /// Get a mip-level width.
        /// </summary>
        /// <param name="mipLevel"></param>
        /// <returns></returns>
        public int GetWidth(int mipLevel = 0)
        {
            return (mipLevel == 0) || (mipLevel < MipLevels) ? Math.Max(1, Width >> mipLevel) : 0;
        }

        /// <summary>
        /// Get a mip-level height.
        /// </summary>
        /// <param name="mipLevel"></param>
        /// <returns></returns>
        public int GetHeight(int mipLevel = 0)
        {
            return (mipLevel == 0) || (mipLevel < MipLevels) ? Math.Max(1, Height >> mipLevel) : 0;
        }
    }
}