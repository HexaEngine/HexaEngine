namespace HexaEngine.Core.Graphics
{
    using System.Xml.Serialization;

    public static class AccessHelper
    {
        public static void Convert(Usage usage, BindFlags bindFlags, out CpuAccessFlags cpuAccessFlags, out GpuAccessFlags gpuAccessFlags)
        {
            cpuAccessFlags = CpuAccessFlags.None;
            gpuAccessFlags = GpuAccessFlags.None;

            switch (usage)
            {
                case Usage.Default:
                    if ((bindFlags & BindFlags.ShaderResource) != 0)
                        gpuAccessFlags |= GpuAccessFlags.Read;
                    if ((bindFlags & BindFlags.RenderTarget) != 0)
                        gpuAccessFlags |= GpuAccessFlags.Write;
                    if ((bindFlags & BindFlags.UnorderedAccess) != 0)
                        gpuAccessFlags |= GpuAccessFlags.UA;
                    if ((bindFlags & BindFlags.DepthStencil) != 0)
                        gpuAccessFlags |= GpuAccessFlags.DepthStencil;
                    break;

                case Usage.Dynamic:
                    cpuAccessFlags |= CpuAccessFlags.Write;
                    if ((bindFlags & BindFlags.ShaderResource) != 0)
                        gpuAccessFlags |= GpuAccessFlags.Read;
                    break;

                case Usage.Staging:
                    cpuAccessFlags |= CpuAccessFlags.Read | CpuAccessFlags.Write;
                    break;
            }
        }

        public static void Convert(CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags, out Usage usage, out BindFlags bindFlags)
        {
            usage = Usage.Default;
            bindFlags = BindFlags.None;
            if ((gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                usage = Usage.Default;
                bindFlags |= BindFlags.ShaderResource;
            }

            if ((gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                usage = Usage.Default;
                bindFlags |= BindFlags.RenderTarget;
            }

            if ((gpuAccessFlags & GpuAccessFlags.UA) != 0)
            {
                usage = Usage.Default;
                bindFlags |= BindFlags.UnorderedAccess;
            }

            if ((gpuAccessFlags & GpuAccessFlags.DepthStencil) != 0)
            {
                usage = Usage.Default;
                bindFlags |= BindFlags.DepthStencil;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                usage = Usage.Dynamic;
                bindFlags = BindFlags.ShaderResource;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                usage = Usage.Staging;
                bindFlags = BindFlags.None;
            }
        }
    }

    /// <summary>
    /// Represents the description of a 1D texture.
    /// </summary>
    public struct Texture1DDescription
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
    }
}