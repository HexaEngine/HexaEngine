namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a depth stencil texture for use in graphics rendering.
    /// </summary>
    public class DepthStencil : ITexture2D, IDepthStencilView
    {
        private readonly string dbgName;

        private int width;
        private int height;
        private int arraySize;
        private int mipLevels;
        private Format format;
        private CpuAccessFlags cpuAccessFlags;
        private GpuAccessFlags gpuAccessFlags;
        private DepthStencilViewFlags depthStencilViewFlags;
        private ResourceMiscFlag miscFlag;
        private Texture2DDescription description;

        private ITexture2D texture;
        private IDepthStencilView dsv;
        private IUnorderedAccessView? uav;
        private IShaderResourceView? srv;
        private Viewport viewport;
        private bool disposedValue;
        private bool canWrite;
        private bool canRead;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencil"/> class.
        /// </summary>
        /// <param name="device">The graphics device for rendering.</param>
        /// <param name="desc">The description of the depth stencil buffer.</param>
        /// <param name="filename">The name of the file that contains the calling code (automatically generated).</param>
        /// <param name="lineNumber">The line number of the calling code (automatically generated).</param>
        /// <exception cref="ArgumentException">Thrown when the format is invalid for a depth stencil.</exception>
        public DepthStencil(IGraphicsDevice device, DepthStencilBufferDescription desc, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!FormatHelper.IsDepthStencil(desc.Format))
            {
                throw new ArgumentException("Invalid format for a depth stencil");
            }

            cpuAccessFlags = description.CPUAccessFlags;

            if (desc.BindFlags.HasFlag(BindFlags.ShaderResource))
            {
                gpuAccessFlags |= GpuAccessFlags.Read;
            }
            if (desc.BindFlags.HasFlag(BindFlags.DepthStencil))
            {
                gpuAccessFlags |= GpuAccessFlags.Write;
            }
            if (desc.BindFlags.HasFlag(BindFlags.UnorderedAccess))
            {
                gpuAccessFlags |= GpuAccessFlags.UA;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0 && (gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot read at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0 && (gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot write at the same time");
            }

            if (arraySize != 6 && miscFlag == ResourceMiscFlag.TextureCube)
            {
                throw new ArgumentException("A texture cube must have 6 array slices");
            }

            dbgName = $"DepthStencil: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            width = desc.Width;
            height = desc.Height;
            arraySize = desc.ArraySize;
            mipLevels = desc.MipLevels;
            format = desc.Format;
            miscFlag = desc.MiscFlag;
            viewport = new(desc.Width, desc.Height);

            Format resourceFormat = GetDepthResourceFormat(format);
            Format srvFormat = GetDepthSRVFormat(format);

            description = new(resourceFormat, width, height, arraySize, mipLevels, BindFlags.None, Usage.Default, cpuAccessFlags, 1, 0, miscFlag);

            if ((gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.ShaderResource;
            }

            if ((gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.DepthStencil;
            }

            if ((gpuAccessFlags & GpuAccessFlags.UA) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.UnorderedAccess;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Dynamic;
                description.BindFlags = BindFlags.ShaderResource;
                canWrite = true;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Staging;
                description.BindFlags = BindFlags.None;
                canRead = true;
            }

            texture = device.CreateTexture2D(description);
            texture.DebugName = dbgName;

            dsv = device.CreateDepthStencilView(texture, new((ITexture2D)texture, arraySize > 1 ? DepthStencilViewDimension.Texture2DArray : DepthStencilViewDimension.Texture2D, format, flags: depthStencilViewFlags));
            dsv.DebugName = dbgName + ".DSV";

            if (description.BindFlags.HasFlag(BindFlags.ShaderResource))
            {
                srv = device.CreateShaderResourceView(texture, new((ITexture2D)texture, arraySize > 1 ? miscFlag == ResourceMiscFlag.TextureCube ? ShaderResourceViewDimension.TextureCube : ShaderResourceViewDimension.Texture2DArray : ShaderResourceViewDimension.Texture2D, srvFormat));
                srv.DebugName = dbgName + ".SRV";
            }

            if (description.BindFlags.HasFlag(BindFlags.UnorderedAccess))
            {
                uav = device.CreateUnorderedAccessView(texture, new((ITexture2D)texture, arraySize > 1 ? UnorderedAccessViewDimension.Texture2DArray : UnorderedAccessViewDimension.Texture2D, srvFormat));
                uav.DebugName = dbgName + ".UAV";
            }

            MemoryManager.Register(texture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencil"/> class with the specified format, width, height, array size, mip levels, CPU access flags, GPU access flags, depth stencil view flags, and miscellaneous flags.
        /// This constructor sets the array size to 1 and the mip levels to 1.
        /// </summary>
        /// <param name="device">The graphics device for rendering.</param>
        /// <param name="format">The format of the depth stencil buffer.</param>
        /// <param name="width">The width of the depth stencil buffer.</param>
        /// <param name="height">The height of the depth stencil buffer.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the depth stencil buffer.</param>
        /// <param name="gpuAccessFlags">The GPU access flags for the depth stencil buffer.</param>
        /// <param name="depthStencilViewFlags">The depth stencil view flags for the depth stencil buffer.</param>
        /// <param name="miscFlag">The miscellaneous flags for the depth stencil buffer.</param>
        /// <param name="filename">The name of the file that contains the calling code (automatically generated).</param>
        /// <param name="lineNumber">The line number of the calling code (automatically generated).</param>
        public DepthStencil(IGraphicsDevice device, Format format, int width, int height, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.RW, DepthStencilViewFlags depthStencilViewFlags = DepthStencilViewFlags.None, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        : this(device, format, width, height, 1, 1, cpuAccessFlags, gpuAccessFlags, depthStencilViewFlags, miscFlag, filename, lineNumber)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencil"/> class with the specified format, width, height, array size, and CPU and GPU access flags.
        /// This constructor sets the mip levels to 1 and uses the default depth stencil view flags and miscellaneous flags.
        /// </summary>
        /// <param name="device">The graphics device for rendering.</param>
        /// <param name="format">The format of the depth stencil buffer.</param>
        /// <param name="width">The width of the depth stencil buffer.</param>
        /// <param name="height">The height of the depth stencil buffer.</param>
        /// <param name="arraySize">The array size (number of slices) of the depth stencil buffer.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the depth stencil buffer.</param>
        /// <param name="gpuAccessFlags">The GPU access flags for the depth stencil buffer.</param>,
        /// <param name="depthStencilViewFlags">The depth stencil view flags for the depth stencil buffer.</param>
        /// <param name="miscFlag">The miscellaneous flags for the depth stencil buffer.</param>
        /// <param name="filename">The name of the file that contains the calling code (automatically generated).</param>
        /// <param name="lineNumber">The line number of the calling code (automatically generated).</param>
        public DepthStencil(IGraphicsDevice device, Format format, int width, int height, int arraySize, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.RW, DepthStencilViewFlags depthStencilViewFlags = DepthStencilViewFlags.None, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        : this(device, format, width, height, arraySize, 1, cpuAccessFlags, gpuAccessFlags, depthStencilViewFlags, miscFlag, filename, lineNumber)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencil"/> class with the specified format, width, height, array size, mip levels, CPU access flags, GPU access flags, depth stencil view flags, and miscellaneous flags.
        /// </summary>
        /// <param name="device">The graphics device for rendering.</param>
        /// <param name="format">The format of the depth stencil buffer.</param>
        /// <param name="width">The width of the depth stencil buffer.</param>
        /// <param name="height">The height of the depth stencil buffer.</param>
        /// <param name="arraySize">The array size (number of slices) of the depth stencil buffer.</param>
        /// <param name="mipLevels">The number of mip levels in the depth stencil buffer.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the depth stencil buffer.</param>
        /// <param name="gpuAccessFlags">The GPU access flags for the depth stencil buffer.</param>
        /// <param name="depthStencilViewFlags">The depth stencil view flags for the depth stencil buffer.</param>
        /// <param name="miscFlag">The miscellaneous flags for the depth stencil buffer.</param>
        /// <param name="filename">The name of the file that contains the calling code (automatically generated).</param>
        /// <param name="lineNumber">The line number of the calling code (automatically generated).</param>
        /// <exception cref="ArgumentException">Thrown when the format is invalid for a depth stencil.</exception>
        public DepthStencil(IGraphicsDevice device, Format format, int width, int height, int arraySize, int mipLevels, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.RW, DepthStencilViewFlags depthStencilViewFlags = DepthStencilViewFlags.None, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!FormatHelper.IsDepthStencil(format))
            {
                throw new ArgumentException("Invalid format for a depth stencil");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0 && (gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot read at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0 && (gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot write at the same time");
            }

            if (arraySize != 6 && miscFlag == ResourceMiscFlag.TextureCube)
            {
                throw new ArgumentException("A texture cube must have 6 array slices");
            }

            dbgName = $"DepthStencil: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            this.format = format;
            this.width = width;
            this.height = height;
            this.arraySize = arraySize;
            this.mipLevels = mipLevels;
            this.cpuAccessFlags = cpuAccessFlags;
            this.gpuAccessFlags = gpuAccessFlags;
            this.depthStencilViewFlags = depthStencilViewFlags;
            this.miscFlag = miscFlag;
            viewport = new(width, height);

            Format resourceFormat = GetDepthResourceFormat(format);
            Format srvFormat = GetDepthSRVFormat(format);
            description = new(resourceFormat, width, height, arraySize, mipLevels, BindFlags.None, Usage.Default, cpuAccessFlags, 1, 0, miscFlag);

            if ((gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.ShaderResource;
            }

            if ((gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.DepthStencil;
            }

            if ((gpuAccessFlags & GpuAccessFlags.UA) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.UnorderedAccess;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Dynamic;
                description.BindFlags = BindFlags.ShaderResource;
                canWrite = true;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Staging;
                description.BindFlags = BindFlags.None;
                canRead = true;
            }

            texture = device.CreateTexture2D(description);
            texture.DebugName = dbgName;

            dsv = device.CreateDepthStencilView(texture, new((ITexture2D)texture, arraySize > 1 ? DepthStencilViewDimension.Texture2DArray : DepthStencilViewDimension.Texture2D, format, flags: depthStencilViewFlags));
            dsv.DebugName = dbgName + ".DSV";

            if (description.BindFlags.HasFlag(BindFlags.ShaderResource))
            {
                srv = device.CreateShaderResourceView(texture, new((ITexture2D)texture, arraySize > 1 ? miscFlag == ResourceMiscFlag.TextureCube ? ShaderResourceViewDimension.TextureCube : ShaderResourceViewDimension.Texture2DArray : ShaderResourceViewDimension.Texture2D, srvFormat));
                srv.DebugName = dbgName + ".SRV";
            }

            if (description.BindFlags.HasFlag(BindFlags.UnorderedAccess))
            {
                uav = device.CreateUnorderedAccessView(texture, new((ITexture2D)texture, arraySize > 1 ? UnorderedAccessViewDimension.Texture2DArray : UnorderedAccessViewDimension.Texture2D, srvFormat));
                uav.DebugName = dbgName + ".UAV";
            }

            MemoryManager.Register(texture);
        }

        /// <summary>
        /// Gets the resource dimension of the depth stencil.
        /// </summary>
        public ResourceDimension Dimension => ResourceDimension.Texture2D;

        /// <summary>
        /// Gets the depth stencil view associated with the depth stencil.
        /// </summary>
        public IDepthStencilView DSV => dsv;

        /// <summary>
        /// Gets the shader resource view associated with the depth stencil, if available.
        /// </summary>
        public IShaderResourceView? SRV => srv;

        /// <summary>
        /// Gets the unordered access view associated with the depth stencil, if available.
        /// </summary>
        public IUnorderedAccessView? UAV => uav;

        /// <summary>
        /// Gets the viewport associated with the depth stencil.
        /// </summary>
        public Viewport Viewport => viewport;

        /// <summary>
        /// Gets the description of the depth stencil.
        /// </summary>
        public Texture2DDescription Description => description;

        /// <summary>
        /// Gets the format of the depth stencil.
        /// </summary>
        public Format Format => format;

        /// <summary>
        /// Gets the width of the depth stencil.
        /// </summary>
        public int Width => width;

        /// <summary>
        /// Gets the height of the depth stencil.
        /// </summary>
        public int Height => height;

        /// <summary>
        /// Gets the array size of the depth stencil.
        /// </summary>
        public int ArraySize => arraySize;

        /// <summary>
        /// Gets the number of mip levels of the depth stencil.
        /// </summary>
        public int MipLevels => mipLevels;

        /// <summary>
        /// Gets the CPU access flags of the depth stencil.
        /// </summary>
        public CpuAccessFlags CpuAccessFlags => cpuAccessFlags;

        /// <summary>
        /// Gets the GPU access flags of the depth stencil.
        /// </summary>
        public GpuAccessFlags GpuAccessFlags => gpuAccessFlags;

        /// <summary>
        /// Gets the miscellaneous flags of the depth stencil.
        /// </summary>
        public ResourceMiscFlag MiscFlag => miscFlag;

        /// <summary>
        /// Gets a value indicating whether the depth stencil can be read.
        /// </summary>
        public bool CanRead => canRead;

        /// <summary>
        /// Gets a value indicating whether the depth stencil can be written to.
        /// </summary>
        public bool CanWrite => canWrite;

        /// <summary>
        /// Gets a value indicating whether the depth stencil has been disposed.
        /// </summary>
        public bool IsDisposed => disposedValue;

        /// <summary>
        /// Gets the native pointer to the depth stencil texture.
        /// </summary>
        public nint NativePointer => texture.NativePointer;

        /// <summary>
        /// Gets or sets the debug name for the depth stencil.
        /// </summary>
        public string? DebugName { get => texture.DebugName; set => texture.DebugName = value; }

        /// <summary>
        /// Occurs when the depth stencil is disposed.
        /// </summary>
        public event EventHandler? OnDisposed;

        DepthStencilViewDescription IDepthStencilView.Description => dsv.Description;

        string? IDepthStencilView.DebugName { get => dsv.DebugName; set => dsv.DebugName = value; }

        bool IDepthStencilView.IsDisposed => dsv.IsDisposed;

        nint IDepthStencilView.NativePointer => dsv.NativePointer;

        private static Format GetDepthResourceFormat(Format depthFormat)
        {
            Format resFormat = Format.Unknown;
            switch (depthFormat)
            {
                case Format.D16UNorm:
                    resFormat = Format.R32Typeless;
                    break;

                case Format.D24UNormS8UInt:
                    resFormat = Format.R24G8Typeless;
                    break;

                case Format.D32Float:
                    resFormat = Format.R32Typeless;
                    break;

                case Format.D32FloatS8X24UInt:
                    resFormat = Format.R32G8X24Typeless;
                    break;
            }

            return resFormat;
        }

        private static Format GetDepthSRVFormat(Format depthFormat)
        {
            Format srvFormat = Format.Unknown;
            switch (depthFormat)
            {
                case Format.D16UNorm:
                    srvFormat = Format.R16Float;
                    break;

                case Format.D24UNormS8UInt:
                    srvFormat = Format.R24UNormX8Typeless;
                    break;

                case Format.D32Float:
                    srvFormat = Format.R32Float;
                    break;

                case Format.D32FloatS8X24UInt:
                    srvFormat = Format.R32FloatX8X24Typeless;
                    break;
            }
            return srvFormat;
        }

        /// <summary>
        /// Resizes the depth stencil to the specified dimensions using the existing format, array size, mip levels, CPU access flags, GPU access flags, depth stencil view flags, and miscellaneous flags.
        /// </summary>
        /// <param name="device">The graphics device used for resizing.</param>
        /// <param name="width">The new width of the depth stencil.</param>
        /// <param name="height">The new height of the depth stencil.</param>
        public void Resize(IGraphicsDevice device, int width, int height)
        {
            Resize(device, format, width, height, arraySize, mipLevels, cpuAccessFlags, gpuAccessFlags, depthStencilViewFlags, miscFlag);
        }

        /// <summary>
        /// Resizes the depth stencil to the specified dimensions using the given format, keeping the existing values for array size, mip levels, CPU access flags, GPU access flags, depth stencil view flags, and miscellaneous flags.
        /// </summary>
        /// <param name="device">The graphics device used for resizing.</param>
        /// <param name="format">The new format of the depth stencil.</param>
        /// <param name="width">The new width of the depth stencil.</param>
        /// <param name="height">The new height of the depth stencil.</param>
        public void Resize(IGraphicsDevice device, Format format, int width, int height)
        {
            Resize(device, format, width, height, arraySize, mipLevels, cpuAccessFlags, gpuAccessFlags, depthStencilViewFlags, miscFlag);
        }

        /// <summary>
        /// Resizes the depth stencil to the specified dimensions using the given format and array size, keeping the existing values for mip levels, CPU access flags, GPU access flags, depth stencil view flags, and miscellaneous flags.
        /// </summary>
        /// <param name="device">The graphics device used for resizing.</param>
        /// <param name="format">The new format of the depth stencil.</param>
        /// <param name="width">The new width of the depth stencil.</param>
        /// <param name="height">The new height of the depth stencil.</param>
        /// <param name="arraySize">The new array size of the depth stencil.</param>
        public void Resize(IGraphicsDevice device, Format format, int width, int height, int arraySize)
        {
            Resize(device, format, width, height, arraySize, mipLevels, cpuAccessFlags, gpuAccessFlags, depthStencilViewFlags, miscFlag);
        }

        /// <summary>
        /// Resizes the depth stencil to the specified dimensions using the given format, array size, and mip levels, keeping the existing values for CPU access flags, GPU access flags, depth stencil view flags, and miscellaneous flags.
        /// </summary>
        /// <param name="device">The graphics device used for resizing.</param>
        /// <param name="format">The new format of the depth stencil.</param>
        /// <param name="width">The new width of the depth stencil.</param>
        /// <param name="height">The new height of the depth stencil.</param>
        /// <param name="arraySize">The new array size of the depth stencil.</param>
        /// <param name="mipLevels">The new number of mip levels of the depth stencil.</param>
        public void Resize(IGraphicsDevice device, Format format, int width, int height, int arraySize, int mipLevels)
        {
            Resize(device, format, width, height, arraySize, mipLevels, cpuAccessFlags, gpuAccessFlags, depthStencilViewFlags, miscFlag);
        }

        /// <summary>
        /// Resizes the depth stencil with the specified parameters.
        /// </summary>
        /// <param name="device">The graphics device used for resizing.</param>
        /// <param name="format">The new format of the depth stencil.</param>
        /// <param name="width">The new width of the depth stencil.</param>
        /// <param name="height">The new height of the depth stencil.</param>
        /// <param name="cpuAccessFlags">The new CPU access flags of the depth stencil.</param>
        /// <param name="gpuAccessFlags">The new GPU access flags of the depth stencil.</param>
        /// <param name="depthStencilViewFlags">The new depth stencil view flags of the depth stencil.</param>
        /// <param name="miscFlag">The new miscellaneous flags of the depth stencil.</param>
        /// <exception cref="ArgumentException">Thrown when an invalid format is specified for a depth stencil.</exception>

        public void Resize(IGraphicsDevice device, Format format, int width, int height, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags, DepthStencilViewFlags depthStencilViewFlags = DepthStencilViewFlags.None, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            Resize(device, format, width, height, arraySize, mipLevels, cpuAccessFlags, gpuAccessFlags, depthStencilViewFlags, miscFlag);
        }

        /// <summary>
        /// Resizes the depth stencil with the specified parameters.
        /// </summary>
        /// <param name="device">The graphics device used for resizing.</param>
        /// <param name="format">The new format of the depth stencil.</param>
        /// <param name="width">The new width of the depth stencil.</param>
        /// <param name="height">The new height of the depth stencil.</param>
        /// <param name="arraySize">The new array size of the depth stencil.</param>
        /// <param name="cpuAccessFlags">The new CPU access flags of the depth stencil.</param>
        /// <param name="gpuAccessFlags">The new GPU access flags of the depth stencil.</param>
        /// <param name="depthStencilViewFlags">The new depth stencil view flags of the depth stencil.</param>
        /// <param name="miscFlag">The new miscellaneous flags of the depth stencil.</param>
        /// <exception cref="ArgumentException">Thrown when an invalid format is specified for a depth stencil.</exception>
        public void Resize(IGraphicsDevice device, Format format, int width, int height, int arraySize, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags, DepthStencilViewFlags depthStencilViewFlags = DepthStencilViewFlags.None, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            Resize(device, format, width, height, arraySize, mipLevels, cpuAccessFlags, gpuAccessFlags, depthStencilViewFlags, miscFlag);
        }

        /// <summary>
        /// Resizes the depth stencil with the specified parameters.
        /// </summary>
        /// <param name="device">The graphics device used for resizing.</param>
        /// <param name="format">The new format of the depth stencil.</param>
        /// <param name="width">The new width of the depth stencil.</param>
        /// <param name="height">The new height of the depth stencil.</param>
        /// <param name="arraySize">The new array size of the depth stencil.</param>
        /// <param name="mipLevels">The new number of mip levels of the depth stencil.</param>
        /// <param name="cpuAccessFlags">The new CPU access flags of the depth stencil.</param>
        /// <param name="gpuAccessFlags">The new GPU access flags of the depth stencil.</param>
        /// <param name="depthStencilViewFlags">The new depth stencil view flags of the depth stencil.</param>
        /// <param name="miscFlag">The new miscellaneous flags of the depth stencil.</param>
        /// <exception cref="ArgumentException">Thrown when an invalid format is specified for a depth stencil.</exception>
        public void Resize(IGraphicsDevice device, Format format, int width, int height, int arraySize, int mipLevels, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags, DepthStencilViewFlags depthStencilViewFlags = DepthStencilViewFlags.None, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            if (!FormatHelper.IsDepthStencil(format))
            {
                throw new ArgumentException("Invalid format for a depth stencil");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0 && (gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot read at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0 && (gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot write at the same time");
            }

            if (arraySize != 6 && miscFlag == ResourceMiscFlag.TextureCube)
            {
                throw new ArgumentException("A texture cube must have 6 array slices");
            }

            this.format = format;
            this.width = width;
            this.height = height;
            this.arraySize = arraySize;
            this.mipLevels = mipLevels;
            this.cpuAccessFlags = cpuAccessFlags;
            this.gpuAccessFlags = gpuAccessFlags;
            this.depthStencilViewFlags = depthStencilViewFlags;
            this.miscFlag = miscFlag;
            viewport = new(width, height);

            Format resourceFormat = GetDepthResourceFormat(format);
            Format srvFormat = GetDepthSRVFormat(format);
            description = new(resourceFormat, width, height, arraySize, mipLevels, BindFlags.None, Usage.Default, cpuAccessFlags, 1, 0, miscFlag);

            if ((gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.ShaderResource;
            }

            if ((gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.DepthStencil;
            }

            if ((gpuAccessFlags & GpuAccessFlags.UA) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.UnorderedAccess;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Dynamic;
                description.BindFlags = BindFlags.ShaderResource;
                canWrite = true;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Staging;
                description.BindFlags = BindFlags.None;
                canRead = true;
            }

            MemoryManager.Unregister(texture);
            texture.Dispose();
            dsv.Dispose();
            uav?.Dispose();
            srv?.Dispose();

            texture = device.CreateTexture2D(description);
            texture.DebugName = dbgName;

            dsv = device.CreateDepthStencilView(texture, new((ITexture2D)texture, arraySize > 1 ? DepthStencilViewDimension.Texture2DArray : DepthStencilViewDimension.Texture2D, format, flags: depthStencilViewFlags));
            dsv.DebugName = dbgName + ".DSV";

            if (description.BindFlags.HasFlag(BindFlags.ShaderResource))
            {
                srv = device.CreateShaderResourceView(texture, new((ITexture2D)texture, arraySize > 1 ? miscFlag == ResourceMiscFlag.TextureCube ? ShaderResourceViewDimension.TextureCube : ShaderResourceViewDimension.Texture2DArray : ShaderResourceViewDimension.Texture2D, srvFormat));
                srv.DebugName = dbgName + ".SRV";
            }

            if (description.BindFlags.HasFlag(BindFlags.UnorderedAccess))
            {
                uav = device.CreateUnorderedAccessView(texture, new((ITexture2D)texture, arraySize > 1 ? UnorderedAccessViewDimension.Texture2DArray : UnorderedAccessViewDimension.Texture2D, srvFormat));
                uav.DebugName = dbgName + ".UAV";
            }

            MemoryManager.Register(texture);
        }

        /// <summary>
        /// Copies the content of this depth stencil to another resource using the provided graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for copying the resource.</param>
        /// <param name="resource">The destination resource where the content of this depth stencil will be copied.</param>
        public void CopyTo(IGraphicsContext context, IResource resource)
        {
            context.CopyResource(resource, texture);
        }

        /// <summary>
        /// Disposes the specified disposing.
        /// </summary>
        /// <param name="disposing">if set to <c>true</c> [disposing].</param>
        /// <returns></returns>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                texture.Dispose();
                dsv.Dispose();
                SRV?.Dispose();
                MemoryManager.Unregister(texture);
                OnDisposed?.Invoke(this, EventArgs.Empty);
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes this instance of <see cref="DepthStencil"/>.
        /// </summary>
        /// <returns></returns>
        ~DepthStencil()
        {
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}