namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a 3D texture that implements various graphics interfaces.
    /// </summary>
    public unsafe class Texture3D : ITexture3D, IShaderResourceView, IRenderTargetView, IUnorderedAccessView
    {
        private readonly string dbgName;
        private Texture3DDescription description;
        private Format format;
        private int width;
        private int height;
        private int depth;
        private int mipLevels;
        private CpuAccessFlags cpuAccessFlags;
        private GpuAccessFlags gpuAccessFlags;
        private ResourceMiscFlag miscFlag;
        private bool canWrite;
        private bool canRead;
        private ITexture3D texture;
        private IShaderResourceView? srv;
        private IRenderTargetView? rtv;
        private IUnorderedAccessView? uav;
        private bool isDirty;
        private bool disposedValue;
        private int rowPitch;
        private int slicePitch;
        private byte* local;

        private IGraphicsDevice device;
        private Asset? asset;

#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture3D"/> class.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="asset">The asset associated with this texture.</param>
        /// <param name="generateMips">Indicates whether to generate mipmaps.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture3D(IGraphicsDevice device, Asset asset, bool generateMips = true, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            this.device = device;
            this.asset = asset;
            asset.OnExistsChanged += OnExistsChanged;
            asset.OnContentChanged += OnContentChanged;
            dbgName = $"Texture3D: {asset.FullPath}, {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            texture = device.TextureLoader.LoadTexture3D(new TextureFileDescription(asset.FullPath, mipLevels: generateMips ? 0 : 1));
            texture.DebugName = dbgName;
            description = texture.Description;
            format = description.Format;
            width = description.Width;
            height = description.Height;
            mipLevels = description.MipLevels;
            depth = description.Depth;
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

            FormatHelper.ComputePitch(format, width, height, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * height);
                ZeroMemory(local, rowPitch * height);
            }
            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, UnorderedAccessViewDimension.Texture3D));
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
        public static Texture3D LoadFromAssets(IGraphicsDevice device, string path, bool generateMips = true)
        {
            return new(device, FileSystem.GetAsset(Paths.CurrentTexturePath + path), generateMips);
        }

#nullable disable

        /// <summary>
        /// Asynchronously loads a <see cref="Texture3D"/> from assets and returns it.
        /// </summary>
        /// <param name="device">The graphics device associated with the texture.</param>
        /// <param name="path">The path to the texture asset.</param>
        /// <param name="generateMips">Indicates whether to generate mipmaps.</param>
        /// <returns>The loaded <see cref="Texture3D"/> object.</returns>
        public static Task<Texture3D> LoadFromAssetsAsync(IGraphicsDevice device, string path, bool generateMips = true)
        {
            return Task.Factory.StartNew((object state) =>
            {
                var data = ((IGraphicsDevice device, string path, bool generateMips))state;
                return new Texture3D(data.device, FileSystem.GetAsset(Paths.CurrentTexturePath + data.path), data.generateMips);
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
        /// Initializes a new instance of the <see cref="Texture3D"/> class using the specified <see cref="TextureFileDescription"/>.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="description">The <see cref="TextureFileDescription"/> used to create the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture3D(IGraphicsDevice device, TextureFileDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture3D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            texture = device.TextureLoader.LoadTexture3D(description.Path);
            texture.DebugName = dbgName;
            this.description = texture.Description;
            format = this.description.Format;
            width = this.description.Width;
            height = this.description.Height;
            depth = this.description.Depth;
            mipLevels = this.description.MipLevels;
            cpuAccessFlags = this.description.CPUAccessFlags;
            gpuAccessFlags = description.Usage switch
            {
                Usage.Default => GpuAccessFlags.RW,
                Usage.Dynamic => GpuAccessFlags.Read,
                Usage.Staging => GpuAccessFlags.None,
                Usage.Immutable => GpuAccessFlags.Read,
                _ => throw new NotImplementedException(),
            };
            miscFlag = this.description.MiscFlags;

            FormatHelper.ComputePitch(format, width, height, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * height);
                ZeroMemory(local, rowPitch * height);
            }

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, UnorderedAccessViewDimension.Texture3D));
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
        /// Initializes a new instance of the <see cref="Texture3D"/> class with the specified parameters.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="format">The format of the texture.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="depth">The depth of the texture.</param>
        /// <param name="mipLevels">The number of mip levels for the texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the texture.</param>
        /// <param name="gpuAccessFlags">The GPU access flags for the texture (default is read-only).</param>
        /// <param name="miscFlag">The miscellaneous flags for the texture (default is none).</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture3D(IGraphicsDevice device, Format format, int width, int height, int depth, int mipLevels, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture3D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            this.format = format;
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.mipLevels = mipLevels;
            this.cpuAccessFlags = cpuAccessFlags;
            this.gpuAccessFlags = gpuAccessFlags;
            this.miscFlag = miscFlag;
            description = new(format, width, height, depth, mipLevels, BindFlags.ShaderResource | BindFlags.RenderTarget, Usage.Default, cpuAccessFlags, miscFlag);

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

            FormatHelper.ComputePitch(format, width, height, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * height);
                ZeroMemory(local, rowPitch * height);
            }

            texture = device.CreateTexture3D(description);
            texture.DebugName = dbgName;
            MemoryManager.Register(texture);

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, UnorderedAccessViewDimension.Texture3D));
                uav.DebugName = dbgName + ".UAV";
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture, new(ShaderResourceViewDimension.Texture3D));
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture);
                rtv.DebugName = dbgName + ".RTV";
            }
        }

#nullable restore

#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture3D"/> class using the provided <see cref="Texture3DDescription"/>.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="description">The <see cref="Texture3DDescription"/> used to create the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture3D(IGraphicsDevice device, Texture3DDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture3D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            format = description.Format;
            width = description.Width;
            height = description.Height;
            depth = description.Depth;
            mipLevels = description.MipLevels;
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

            FormatHelper.ComputePitch(format, width, height, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * height);
                ZeroMemory(local, rowPitch * height);
            }

            texture = device.CreateTexture3D(description);
            texture.DebugName = dbgName;
            MemoryManager.Register(texture);

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, UnorderedAccessViewDimension.Texture3D));
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

#nullable restore

#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture3D"/> class using the provided <see cref="Texture3DDescription"/> and initial data.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="description">The <see cref="Texture3DDescription"/> used to create the texture.</param>
        /// <param name="initialData">An array of <see cref="SubresourceData"/> containing the initial data for the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture3D(IGraphicsDevice device, Texture3DDescription description, SubresourceData initialData, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture3D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            format = description.Format;
            width = description.Width;
            height = description.Height;
            depth = description.Depth;
            mipLevels = description.MipLevels;
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

            FormatHelper.ComputePitch(format, width, height, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * height);
                ZeroMemory(local, rowPitch * height);
            }

            texture = device.CreateTexture3D(description, [initialData]);
            texture.DebugName = dbgName;
            MemoryManager.Register(texture);

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, UnorderedAccessViewDimension.Texture3D));
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

#nullable restore

#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture3D"/> class using the provided <see cref="Texture3DDescription"/> and a single <see cref="SubresourceData"/> for initial data.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="description">The <see cref="Texture3DDescription"/> used to create the texture.</param>
        /// <param name="initialData">A single <see cref="SubresourceData"/> containing the initial data for the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture3D(IGraphicsDevice device, Texture3DDescription description, SubresourceData[] initialData, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture3D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            format = description.Format;
            width = description.Width;
            height = description.Height;
            depth = description.Depth;
            mipLevels = description.MipLevels;
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

            FormatHelper.ComputePitch(format, width, height, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * height);
                ZeroMemory(local, rowPitch * height);
            }

            texture = device.CreateTexture3D(description, initialData);
            texture.DebugName = dbgName;
            MemoryManager.Register(texture);

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, UnorderedAccessViewDimension.Texture3D));
                uav.DebugName = dbgName + ".UAV";
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            ArraySlices = new IRenderTargetView[description.Depth];

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture);
                rtv.DebugName = dbgName + ".RTV";

                for (int i = 0; i < description.Depth; i++)
                {
                    ArraySlices[i] = device.CreateRenderTargetView(texture, new RenderTargetViewDescription(texture, firstWSlice: i, wSize: 1));
                    ArraySlices[i].DebugName = dbgName + $".RTV.{i}";
                }
            }
        }

#nullable restore

#nullable disable

        /// <summary>
        /// Asynchronously creates a new instance of the <see cref="Texture3D"/> class using the provided <see cref="TextureFileDescription"/>.
        /// </summary>
        /// <param name="device">The graphics device associated with this texture.</param>
        /// <param name="description">The <see cref="TextureFileDescription"/> used to create the texture.</param>
        /// <returns>A task that represents the asynchronous operation and contains the created <see cref="Texture3D"/>.</returns>
        public static Task<Texture3D> CreateTextureAsync(IGraphicsDevice device, TextureFileDescription description)
        {
            return Task.Factory.StartNew((object state) =>
            {
                var data = ((IGraphicsDevice, TextureFileDescription))state;
                return new Texture3D(data.Item1, data.Item2);
            }, (device, description));
        }

#nullable restore

        /// <summary>
        /// Gets the resource dimension of the texture, which is always <see cref="ResourceDimension.Texture3D"/>.
        /// </summary>
        public ResourceDimension Dimension => ResourceDimension.Texture3D;

        /// <summary>
        /// Gets the description of the texture.
        /// </summary>
        public Texture3DDescription Description => description;

        /// <summary>
        /// Gets the format of the texture.
        /// </summary>

        /// <summary>
        /// Gets the width of the texture.
        /// </summary>
        public int Width => width;

        /// <summary>
        /// Gets the height of the texture.
        /// </summary>
        public int Height => height;

        /// <summary>
        /// Gets the depth of the texture.
        /// </summary>
        public int Depth => depth;

        /// <summary>
        /// Gets the number of mip levels in the texture.
        /// </summary>
        public int MipLevels => mipLevels;

        /// <summary>
        /// Gets the array size of the texture.
        /// </summary>
        public int ArraySize => depth;

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
        public Viewport Viewport => new(0, 0, width, height, 0, depth);

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
        public event Action<Texture3D>? TextureReloaded;

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
        /// Gets or sets a byte at a computed index based on the given Vector3 coordinates.
        /// </summary>
        /// <param name="index">The Vector3 coordinates used to compute the index.</param>
        /// <returns>The byte value at the computed index.</returns>
        public byte this[Vector3 index]
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
        /// <param name="uv">The Vector3 UV coordinates.</param>
        /// <returns>The computed memory index.</returns>
        public int ComputeIndex(Vector3 uv)
        {
            int x = (int)(uv.X * width);
            int y = (int)(uv.Y * height);
            int z = (int)(uv.Z * depth);
            return x + y * rowPitch + z * slicePitch;
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
                    for (int j = 0; j < depth; j++)
                    {
                        Write(context, TextureHelper.ComputeSubresourceIndex2D(mipLevels, depth, i, j));
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
                Memcpy(local, mapped.PData, (uint)description.Height * mapped.RowPitch, (uint)description.Height * mapped.RowPitch);
                context.Unmap(texture, subresource);
            }
            else
            {
                var mapped = context.Map(texture, subresource, MapMode.WriteDiscard, MapFlags.None);
                Memcpy(local, mapped.PData, (uint)description.Height * mapped.RowPitch, (uint)description.Height * mapped.RowPitch);
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
                for (int j = 0; j < depth; j++)
                {
                    Read(context, TextureHelper.ComputeSubresourceIndex2D(mipLevels, depth, i, j));
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
            Memcpy(mapped.PData, local, (uint)description.Height * mapped.RowPitch, (uint)description.Height * mapped.RowPitch);
            context.Unmap(texture, subresource);
        }

        /// <summary>
        /// Resizes the texture with the specified parameters.
        /// </summary>
        /// <param name="device">The graphics device used to create the resized texture.</param>
        /// <param name="format">The format of the resized texture.</param>
        /// <param name="width">The width of the resized texture.</param>
        /// <param name="height">The height of the resized texture.</param>
        /// <param name="depth">The depth of the resized texture.</param>
        /// <param name="mipLevels">The number of mip levels for the resized texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the resized texture.</param>
        /// <param name="gpuAccessFlags">The GPU access flags for the resized texture (default is GpuAccessFlags.Read).</param>
        /// <param name="miscFlag">The miscellaneous flags for the resized texture (default is ResourceMiscFlag.None).</param>
        /// <remarks>
        /// This method resizes the texture to the specified dimensions with the provided access flags and format. If CPU and GPU access flags
        /// conflict or GPU access flags are used for reading, appropriate bindings and usage are adjusted. After resizing, the previous texture
        /// is disposed, and new resources are created accordingly. The texture will be ready to use for rendering, shader resource, or unordered access.
        /// </remarks>
        public void Resize(IGraphicsDevice device, Format format, int width, int height, int depth, int mipLevels, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            this.format = format;
            this.width = width;
            this.height = height;
            this.mipLevels = mipLevels;
            this.depth = depth;
            this.cpuAccessFlags = cpuAccessFlags;
            this.gpuAccessFlags = gpuAccessFlags;
            this.miscFlag = miscFlag;
            description = new(format, width, height, depth, mipLevels, BindFlags.ShaderResource | BindFlags.RenderTarget, Usage.Default, cpuAccessFlags, miscFlag);

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

            FormatHelper.ComputePitch(format, width, height, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * height);
                ZeroMemory(local, rowPitch * height);
            }

            texture.Dispose();
            srv?.Dispose();
            rtv?.Dispose();
            uav?.Dispose();
            MemoryManager.Unregister(texture);
            texture = device.CreateTexture3D(description);
            texture.DebugName = dbgName;
            MemoryManager.Register(texture);

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, UnorderedAccessViewDimension.Texture3D));
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
            texture = device.TextureLoader.LoadTexture3D(asset.FullPath);
            texture.DebugName = dbgName;
            description = texture.Description;
            format = description.Format;
            width = description.Width;
            height = description.Height;
            mipLevels = description.MipLevels;
            depth = description.Depth;
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

            FormatHelper.ComputePitch(format, width, height, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * height);
                ZeroMemory(local, rowPitch * height);
            }
            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = device.CreateUnorderedAccessView(texture, new(texture, UnorderedAccessViewDimension.Texture3D));
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
                    Free(local);

                disposedValue = true;
                OnDisposed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Texture3D"/> class.
        /// </summary>
        ~Texture3D()
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