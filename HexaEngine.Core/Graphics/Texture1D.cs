namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a 1D texture that implements various graphics interfaces.
    /// </summary>
    public unsafe class Texture1D : ITexture1D, IShaderResourceView, IRenderTargetView, IUnorderedAccessView
    {
        private readonly string dbgName;
        private Texture1DDescription description;
        private Format format;
        private int width;
        private int mipLevels;
        private int arraySize;
        private CpuAccessFlags cpuAccessFlags;
        private GpuAccessFlags gpuAccessFlags;
        private ResourceMiscFlag miscFlag;
        private bool canWrite;
        private bool canRead;
        private ITexture1D texture;
        private IShaderResourceView? srv;
        private IRenderTargetView? rtv;
        private IUnorderedAccessView? uav;
        private bool isDirty;
        private bool disposedValue;
        private int rowPitch;
        private int slicePitch;
        private byte* local;

        private readonly IGraphicsDevice device;
        private readonly Asset? asset;

#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1D"/> class.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="asset">The asset associated with this texture.</param>
        /// <param name="generateMips">Indicates whether to generate mipmaps.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture1D(IGraphicsDevice device, Asset asset, bool generateMips = true, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            this.device = device;
            this.asset = asset;
            asset.OnExistsChanged += OnExistsChanged;
            asset.OnContentChanged += OnContentChanged;
            dbgName = $"Texture1D: {asset.FullPath}, {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            texture = device.TextureLoader.LoadTexture1D(new TextureFileDescription(asset.FullPath, mipLevels: generateMips ? 0 : 1));
            texture.DebugName = dbgName;
            description = texture.Description;
            format = description.Format;
            width = description.Width;
            mipLevels = description.MipLevels;
            arraySize = description.ArraySize;
            cpuAccessFlags = description.CPUAccessFlags;
            gpuAccessFlags = description.Usage switch
            {
                Usage.Default => GpuAccessFlags.RW,
                Usage.Dynamic => GpuAccessFlags.Read,
                Usage.Staging => GpuAccessFlags.None,
                Usage.Immutable => GpuAccessFlags.Read,
                _ => throw new NotImplementedException(),
            };
            miscFlag = description.MiscFlags;

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch);
                ZeroMemory(local, rowPitch);
            }
            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, arraySize > 1));
                uav.DebugName = dbgName + ".UAV";
            }
            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture);
                rtv.DebugName = dbgName + ".RTV";
            }
            MemoryManager.Register(texture);
        }

#nullable restore

        /// <summary>
        /// Loads a <see cref="Texture2D"/> from assets and returns it.
        /// </summary>
        /// <param name="device">The graphics device associated with the texture.</param>
        /// <param name="path">The path to the texture asset.</param>
        /// <param name="generateMips">Indicates whether to generate mipmaps.</param>
        /// <returns>The loaded <see cref="Texture2D"/> object.</returns>
        public static Texture2D LoadFromAssets(IGraphicsDevice device, string path, bool generateMips = true)
        {
            return new(device, FileSystem.GetAsset(Paths.CurrentTexturePath + path), generateMips);
        }

#nullable disable
        /// <summary>
        /// Asynchronously loads a <see cref="Texture1D"/> from assets and returns it.
        /// </summary>
        /// <param name="device">The graphics device associated with the texture.</param>
        /// <param name="path">The path to the texture asset.</param>
        /// <param name="generateMips">Indicates whether to generate mipmaps.</param>
        /// <returns>The loaded <see cref="Texture1D"/> object.</returns>
        public static Task<Texture1D> LoadFromAssetsAsync(IGraphicsDevice device, string path, bool generateMips = true)
        {
            return Task.Factory.StartNew((object state) =>
            {
                var data = ((IGraphicsDevice device, string path, bool generateMips))state;
                return new Texture1D(data.device, FileSystem.GetAsset(Paths.CurrentTexturePath + data.path), data.generateMips);
            }, (device, path, generateMips));
        }
#nullable restore

        private void OnContentChanged(Asset asset)
        {
            Reload();
        }

        private void OnExistsChanged(Asset asset, bool exists)
        {
            Reload();
        }

#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1D"/> class using the specified <see cref="TextureFileDescription"/>.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="description">The <see cref="TextureFileDescription"/> used to create the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture1D(IGraphicsDevice device, TextureFileDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            texture = device.TextureLoader.LoadTexture1D(description.Path);
            texture.DebugName = dbgName;
            this.description = texture.Description;
            format = this.description.Format;
            width = this.description.Width;
            mipLevels = this.description.MipLevels;
            arraySize = this.description.ArraySize;
            cpuAccessFlags = this.description.CPUAccessFlags;
            gpuAccessFlags = this.description.Usage switch
            {
                Usage.Default => GpuAccessFlags.RW,
                Usage.Dynamic => GpuAccessFlags.Read,
                Usage.Staging => GpuAccessFlags.None,
                Usage.Immutable => GpuAccessFlags.Read,
                _ => throw new NotImplementedException(),
            };
            miscFlag = this.description.MiscFlags;

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * 1);
                ZeroMemory(local, rowPitch * 1);
            }

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, arraySize > 1));
                uav.DebugName = dbgName + ".UAV";
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture);
                rtv.DebugName = dbgName + ".RTV";
            }
            MemoryManager.Register(texture);
        }

#nullable restore

#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1D"/> class with the specified parameters.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="format">The format of the texture.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="arraySize">The array size of the texture.</param>
        /// <param name="mipLevels">The number of mip levels for the texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the texture.</param>
        /// <param name="gpuAccessFlags">The GPU access flags for the texture (default is read-only).</param>
        /// <param name="miscFlag">The miscellaneous flags for the texture (default is none).</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture1D(IGraphicsDevice device, Format format, int width, int arraySize, int mipLevels, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            this.format = format;
            this.width = width;
            this.mipLevels = mipLevels;
            this.arraySize = arraySize;
            this.cpuAccessFlags = cpuAccessFlags;
            this.gpuAccessFlags = gpuAccessFlags;
            this.miscFlag = miscFlag;
            description = new(format, width, arraySize, mipLevels, BindFlags.ShaderResource | BindFlags.RenderTarget, Usage.Default, cpuAccessFlags, miscFlag);

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0 && (gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot read at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0 && (gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot write at the same time");
            }

            if (cpuAccessFlags != CpuAccessFlags.None && (gpuAccessFlags & GpuAccessFlags.UA) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot use rw with uva at the same time");
            }

            if ((gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.ShaderResource;
            }

            if ((gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.RenderTarget;
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

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch);
                ZeroMemory(local, rowPitch);
            }

            texture = device.CreateTexture1D(description);
            texture.DebugName = dbgName;

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, arraySize > 1));
                uav.DebugName = dbgName + ".UAV";
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture);
                rtv.DebugName = dbgName + ".RTV";
            }
            MemoryManager.Register(texture);
        }

#nullable restore

#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1D"/> class using the provided <see cref="Texture1DDescription"/>.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="description">The <see cref="Texture1DDescription"/> used to create the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture1D(IGraphicsDevice device, Texture1DDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            format = description.Format;
            width = description.Width;
            mipLevels = description.MipLevels;
            arraySize = description.ArraySize;
            cpuAccessFlags = description.CPUAccessFlags;
            gpuAccessFlags = GpuAccessFlags.None;
            miscFlag = description.MiscFlags;
            this.description = description;

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0 && (gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot read at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0 && (gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot write at the same time");
            }

            if (cpuAccessFlags != CpuAccessFlags.None && (gpuAccessFlags & GpuAccessFlags.UA) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot use rw with uva at the same time");
            }

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * 1);
                ZeroMemory(local, rowPitch * 1);
            }

            texture = device.CreateTexture1D(description);
            texture.DebugName = dbgName;

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, arraySize > 1));
                uav.DebugName = dbgName + ".UAV";
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture);
                rtv.DebugName = dbgName + ".RTV";
            }
            MemoryManager.Register(texture);
        }

#nullable restore

#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1D"/> class using the provided <see cref="Texture1DDescription"/> and initial data.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="description">The <see cref="Texture1DDescription"/> used to create the texture.</param>
        /// <param name="initialData">An array of <see cref="SubresourceData"/> containing the initial data for the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture1D(IGraphicsDevice device, Texture1DDescription description, SubresourceData[] initialData, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            format = description.Format;
            width = description.Width;
            mipLevels = description.MipLevels;
            arraySize = description.ArraySize;
            cpuAccessFlags = description.CPUAccessFlags;
            gpuAccessFlags = GpuAccessFlags.None;
            miscFlag = description.MiscFlags;
            this.description = description;

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0 && (gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot read at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0 && (gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot write at the same time");
            }

            if (cpuAccessFlags != CpuAccessFlags.None && (gpuAccessFlags & GpuAccessFlags.UA) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot use rw with uva at the same time");
            }

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * 1);
                ZeroMemory(local, rowPitch * 1);
            }

            texture = device.CreateTexture1D(description, initialData);
            texture.DebugName = dbgName;

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, arraySize > 1));
                uav.DebugName = dbgName + ".UAV";
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture);
                rtv.DebugName = dbgName + ".RTV";
            }
            MemoryManager.Register(texture);
        }

#nullable restore

#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1D"/> class using the provided <see cref="Texture1DDescription"/> and a single <see cref="SubresourceData"/> for initial data.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="description">The <see cref="Texture1DDescription"/> used to create the texture.</param>
        /// <param name="initialData">A single <see cref="SubresourceData"/> containing the initial data for the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture1D(IGraphicsDevice device, Texture1DDescription description, SubresourceData initialData, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            format = description.Format;
            width = description.Width;
            mipLevels = description.MipLevels;
            arraySize = description.ArraySize;
            cpuAccessFlags = description.CPUAccessFlags;
            gpuAccessFlags = GpuAccessFlags.None;
            miscFlag = description.MiscFlags;
            this.description = description;

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0 && (gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot read at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0 && (gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot write at the same time");
            }

            if (cpuAccessFlags != CpuAccessFlags.None && (gpuAccessFlags & GpuAccessFlags.UA) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot use rw with uva at the same time");
            }

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * 1);
                ZeroMemory(local, rowPitch * 1);
            }

            texture = device.CreateTexture1D(description, new SubresourceData[] { initialData });
            texture.DebugName = dbgName;

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, arraySize > 1));
                uav.DebugName = dbgName + ".UAV";
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            ArraySlices = new IRenderTargetView[description.ArraySize];

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture);
                rtv.DebugName = dbgName + ".RTV";

                for (int i = 0; i < description.ArraySize; i++)
                {
                    ArraySlices[i] = device.CreateRenderTargetView(texture, new RenderTargetViewDescription(texture, arraySize > 1, firstArraySlice: i, arraySize: 1));
                    ArraySlices[i].DebugName = dbgName + $".RTV.{i}";
                }
            }
            MemoryManager.Register(texture);
        }

#nullable restore

#nullable disable

        /// <summary>
        /// Asynchronously creates a new instance of the <see cref="Texture1D"/> class using the provided <see cref="TextureFileDescription"/>.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="description">The <see cref="TextureFileDescription"/> used to create the texture.</param>
        /// <returns>A task that represents the asynchronous operation and contains the created <see cref="Texture1D"/>.</returns>
        public static Task<Texture1D> CreateTextureAsync(IGraphicsDevice device, TextureFileDescription description)
        {
            return Task.Factory.StartNew((object state) =>
            {
                var data = ((IGraphicsDevice, TextureFileDescription))state;
                return new Texture1D(data.Item1, data.Item2);
            }, (device, description));
        }
#nullable restore

        /// <summary>
        /// Gets the resource dimension of the texture, which is always <see cref="ResourceDimension.Texture1D"/>.
        /// </summary>
        public ResourceDimension Dimension => ResourceDimension.Texture1D;

        /// <summary>
        /// Gets the description of the texture.
        /// </summary>
        public Texture1DDescription Description => description;

        /// <summary>
        /// Gets the format of the texture.
        /// </summary>
        public Format Format => format;

        /// <summary>
        /// Gets the width of the texture.
        /// </summary>
        public int Width => width;

        /// <summary>
        /// Gets the number of mip levels in the texture.
        /// </summary>
        public int MipLevels => mipLevels;

        /// <summary>
        /// Gets the array size of the texture.
        /// </summary>
        public int ArraySize => arraySize;

        /// <summary>
        /// Gets the CPU access flags for the texture.
        /// </summary>
        public CpuAccessFlags CpuAccessFlags => cpuAccessFlags;

        /// <summary>
        /// Gets the GPU access flags for the texture.
        /// </summary>
        public GpuAccessFlags GpuAccessFlags => gpuAccessFlags;
        /// <summary>
        /// Gets the miscellaneous flags for the texture.
        /// </summary>
        public ResourceMiscFlag MiscFlag => miscFlag;

        /// <summary>
        /// Gets a value indicating whether the texture allows writing.
        /// </summary>
        public bool CanWrite => canWrite;

        /// <summary>
        /// Gets a value indicating whether the texture allows reading.
        /// </summary>
        public bool CanRead => canRead;

        /// <summary>
        /// Gets the row pitch of the texture.
        /// </summary>
        public int RowPitch => rowPitch;

        /// <summary>
        /// Gets the slice pitch of the texture.
        /// </summary>
        public int SlicePitch => slicePitch;

        /// <summary>
        /// Gets a pointer to the local memory associated with the texture.
        /// </summary>
        public void* Local => local;

        /// <summary>
        /// Gets the shader resource view associated with the texture.
        /// </summary>
        public IShaderResourceView? SRV => srv;

        /// <summary>
        /// Gets the render target view associated with the texture.
        /// </summary>
        public IRenderTargetView? RTV => rtv;

        /// <summary>
        /// Gets the unordered access view associated with the texture.
        /// </summary>
        public IUnorderedAccessView? UAV => uav;

        /// <summary>
        /// Gets an array of render target views for each array slice of the texture.
        /// </summary>
        public IRenderTargetView[] ArraySlices;

        /// <summary>
        /// Gets a viewport with the dimensions of the texture.
        /// </summary>
        public Viewport Viewport => new(width, 1);

        /// <summary>
        /// Gets the native pointer to the underlying texture object.
        /// </summary>
        public nint NativePointer => texture.NativePointer;

        /// <summary>
        /// Gets or sets the debug name for the texture.
        /// </summary>
        public string? DebugName { get => texture.DebugName; set => texture.DebugName = value; }

        /// <summary>
        /// Gets a value indicating whether the texture has been disposed.
        /// </summary>
        public bool IsDisposed => disposedValue;

        /// <summary>
        /// Occurs when the texture is disposed.
        /// </summary>
        public event EventHandler? OnDisposed;

        /// <summary>
        /// Gets a value indicating whether the associated asset exists.
        /// </summary>
        public bool Exists => asset?.Exists ?? false;

        /// <summary>
        /// Occurs when the texture is reloaded.
        /// </summary>
        public event Action<Texture1D>? TextureReloaded;


#nullable disable

        ShaderResourceViewDescription IShaderResourceView.Description => srv.Description;

        string IShaderResourceView.DebugName { get => srv.DebugName; set => srv.DebugName = value; }

        nint IShaderResourceView.NativePointer => srv.NativePointer;

        RenderTargetViewDescription IRenderTargetView.Description => rtv.Description;

        nint IRenderTargetView.NativePointer => rtv.NativePointer;

        string IRenderTargetView.DebugName { get => rtv.DebugName; set => rtv.DebugName = value; }

        UnorderedAccessViewDescription IUnorderedAccessView.Description => uav.Description;

        nint IUnorderedAccessView.NativePointer => uav.NativePointer;

        string IUnorderedAccessView.DebugName { get => uav.DebugName; set => uav.DebugName = value; }

#nullable restore

        /// <summary>
        /// Gets or sets a byte at the specified index.
        /// </summary>
        /// <param name="index">The index to access.</param>
        /// <returns>The byte value at the specified index.</returns>
        public byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => local[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                local[index] = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a byte at a computed index based on the given float coordinates.
        /// </summary>
        /// <param name="index">The float coordinates used to compute the index.</param>
        /// <returns>The byte value at the computed index.</returns>
        public byte this[float index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => local[ComputeIndex(index)];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                local[ComputeIndex(index)] = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// Computes the memory index based on UV coordinates.
        /// </summary>
        /// <param name="uv">The float UV coordinates.</param>
        /// <returns>The computed memory index.</returns>
        public int ComputeIndex(float uv)
        {
            int x = (int)(uv * width);
            return x;
        }

        /// <summary>
        /// Writes the modified data to the texture using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to write the data.</param>
        /// <returns>True if the data was written; otherwise, false.</returns>
        public bool Write(IGraphicsContext context)
        {
            if (!canWrite)
            {
                throw new InvalidOperationException();
            }

            if (isDirty)
            {
                for (int i = 0; i < mipLevels; i++)
                {
                    for (int j = 0; j < arraySize; j++)
                    {
                        Write(context, TextureHelper.ComputeSubresourceIndex2D(mipLevels, arraySize, i, j));
                    }
                }

                isDirty = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Writes data to a specific subresource of the texture using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to write the data.</param>
        /// <param name="subresource">The index of the subresource to write.</param>
        public void Write(IGraphicsContext context, int subresource)
        {
            if (!canWrite)
            {
                throw new InvalidOperationException();
            }
            if (canRead)
            {
                var mapped = context.Map(texture, subresource, MapMode.Write, MapFlags.None);
                Memcpy(local, mapped.PData, (uint)1 * mapped.RowPitch, (uint)1 * mapped.RowPitch);
                context.Unmap(texture, subresource);
            }
            else
            {
                var mapped = context.Map(texture, subresource, MapMode.WriteDiscard, MapFlags.None);
                Memcpy(local, mapped.PData, (uint)1 * mapped.RowPitch, (uint)1 * mapped.RowPitch);
                context.Unmap(texture, subresource);
            }
        }

        /// <summary>
        /// Reads data from the texture using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to read the data.</param>
        public void Read(IGraphicsContext context)
        {
            if (!canRead)
            {
                throw new InvalidOperationException();
            }

            for (int i = 0; i < mipLevels; i++)
            {
                for (int j = 0; j < arraySize; j++)
                {
                    Read(context, TextureHelper.ComputeSubresourceIndex2D(mipLevels, arraySize, i, j));
                }
            }
        }

        /// <summary>
        /// Reads data from a specific subresource of the texture using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to read the data.</param>
        /// <param name="subresource">The index of the subresource to read.</param>
        public void Read(IGraphicsContext context, int subresource)
        {
            if (!canRead)
            {
                throw new InvalidOperationException();
            }

            var mapped = context.Map(texture, subresource, MapMode.Read, MapFlags.None);
            Memcpy(mapped.PData, local, (uint)1 * mapped.RowPitch, (uint)1 * mapped.RowPitch);
            context.Unmap(texture, subresource);
        }

        /// <summary>
        /// Resizes the texture with the specified parameters.
        /// </summary>
        /// <param name="device">The graphics device used to create the resized texture.</param>
        /// <param name="format">The format of the resized texture.</param>
        /// <param name="width">The width of the resized texture.</param>
        /// <param name="arraySize">The array size of the resized texture.</param>
        /// <param name="mipLevels">The number of mip levels for the resized texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the resized texture.</param>
        /// <param name="gpuAccessFlags">The GPU access flags for the resized texture (default is GpuAccessFlags.Read).</param>
        /// <param name="miscFlag">The miscellaneous flags for the resized texture (default is ResourceMiscFlag.None).</param>
        /// <remarks>
        /// This method resizes the texture to the specified dimensions with the provided access flags and format. If CPU and GPU access flags
        /// conflict or GPU access flags are used for reading, appropriate bindings and usage are adjusted. After resizing, the previous texture
        /// is disposed, and new resources are created accordingly. The texture will be ready to use for rendering, shader resource, or unordered access.
        /// </remarks>
        public void Resize(IGraphicsDevice device, Format format, int width, int arraySize, int mipLevels, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            this.format = format;
            this.width = width;
            this.mipLevels = mipLevels;
            this.arraySize = arraySize;
            this.cpuAccessFlags = cpuAccessFlags;
            this.gpuAccessFlags = gpuAccessFlags;
            this.miscFlag = miscFlag;
            description = new(format, width, arraySize, mipLevels, BindFlags.ShaderResource | BindFlags.RenderTarget, Usage.Default, cpuAccessFlags, miscFlag);

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0 && (gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot read at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0 && (gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot write at the same time");
            }

            if ((gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.ShaderResource;
            }

            if ((gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Default;
                description.BindFlags |= BindFlags.RenderTarget;
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

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * 1);
                ZeroMemory(local, rowPitch * 1);
            }
            texture.Dispose();
            srv?.Dispose();
            rtv?.Dispose();
            uav?.Dispose();
            MemoryManager.Unregister(texture);
            texture = device.CreateTexture1D(description);
            texture.DebugName = dbgName;
            MemoryManager.Register(texture);

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, arraySize > 1));
                uav.DebugName = dbgName + ".UAV";
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture);
                rtv.DebugName = dbgName + ".RTV";
            }
        }

        /// <summary>
        /// Reloads the texture from the associated asset.
        /// </summary>
        /// <remarks>
        /// This method reloads the texture from the associated asset, disposing the previous texture and creating new resources
        /// according to the asset's path. The texture will be ready for use in rendering, shader resource, or unordered access.
        /// The <see cref="TextureReloaded"/> event is raised upon successful reloading.
        /// </remarks>
        public void Reload()
        {
            if (asset == null)
            {
                return;
            }

            texture.Dispose();
            srv?.Dispose();
            rtv?.Dispose();
            uav?.Dispose();
            MemoryManager.Unregister(texture);
            texture = device.TextureLoader.LoadTexture1D(asset.FullPath);
            texture.DebugName = dbgName;
            description = texture.Description;
            format = description.Format;
            width = description.Width;
            mipLevels = description.MipLevels;
            arraySize = description.ArraySize;
            cpuAccessFlags = description.CPUAccessFlags;
            gpuAccessFlags = description.Usage switch
            {
                Usage.Default => GpuAccessFlags.RW,
                Usage.Dynamic => GpuAccessFlags.Read,
                Usage.Staging => GpuAccessFlags.None,
                Usage.Immutable => GpuAccessFlags.Read,
                _ => throw new NotImplementedException(),
            };
            miscFlag = description.MiscFlags;

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch);
                ZeroMemory(local, rowPitch);
            }
            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, arraySize > 1));
                uav.DebugName = dbgName + ".UAV";
            }
            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture);
                rtv.DebugName = dbgName + ".RTV";
            }
            MemoryManager.Register(texture);

            TextureReloaded?.Invoke(this);
        }

        /// <summary>
        /// Copies the contents of this texture to another resource using the provided graphics context.
        /// </summary>
        /// <param name="context">The graphics context for the copy operation.</param>
        /// <param name="resource">The target resource to which the contents will be copied.</param>
        public void CopyTo(IGraphicsContext context, IResource resource)
        {
            context.CopyResource(resource, texture);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                MemoryManager.Unregister(texture);
                if (asset != null)
                {
                    asset.OnExistsChanged -= OnExistsChanged;
                    asset.OnContentChanged -= OnContentChanged;
                    asset.Dispose();
                }
                texture.Dispose();
                srv?.Dispose();
                rtv?.Dispose();
                uav?.Dispose();
                if (cpuAccessFlags != CpuAccessFlags.None)
                {
                    Free(local);
                }

                disposedValue = true;
                OnDisposed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Texture1D"/> class.
        /// </summary>
        ~Texture1D()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}