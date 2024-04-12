namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System.Runtime.CompilerServices;

    public sealed unsafe class Texture1D : Texture<ITexture1D, Texture1DDescription>, ITexture1D, IShaderResourceView, IRenderTargetView, IUnorderedAccessView
    {
        private int width;
        private int mipLevels;
        private int arraySize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1D"/> class using the specified <see cref="TextureFileDescription"/>.
        /// </summary>
        /// <param name="description">The <see cref="TextureFileDescription"/> used to create the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture1D(TextureFileDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1D"/> class using the specified <see cref="IScratchImage"/>.
        /// </summary>
        /// <param name="description">The <see cref="Texture1DDescription"/> used to create the texture.</param>
        /// <param name="scratchImage">The source image of the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture1D(Texture1DDescription description, IScratchImage scratchImage, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description, scratchImage, description.Format, description.Usage, description.BindFlags, description.CPUAccessFlags, description.MiscFlags)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1D"/> class using the provided <see cref="Texture1DDescription"/>.
        /// </summary>
        /// <param name="description">The <see cref="Texture1DDescription"/> used to create the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture1D(Texture1DDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description, description.Format, description.Usage, description.BindFlags, description.CPUAccessFlags, description.MiscFlags)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1D"/> class with the specified parameters.
        /// </summary>
        /// <param name="format">The format of the texture.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="arraySize">The array size of the texture.</param>
        /// <param name="mipLevels">The number of mip levels for the texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the texture.</param>
        /// <param name="gpuAccessFlags">The GPU access flags for the texture (default is read-only).</param>
        /// <param name="miscFlag">The miscellaneous flags for the texture (default is none).</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture1D(Format format, int width, int arraySize, int mipLevels, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", new Texture1DDescription(format, width, arraySize, mipLevels, gpuAccessFlags, cpuAccessFlags), format, gpuAccessFlags, cpuAccessFlags, miscFlag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1D"/> class using the provided <see cref="Texture1DDescription"/> and initial data.
        /// </summary>
        /// <param name="description">The <see cref="Texture1DDescription"/> used to create the texture.</param>
        /// <param name="initialData">An array of <see cref="SubresourceData"/> containing the initial data for the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture1D(Texture1DDescription description, SubresourceData[] initialData, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description, initialData, description.Format, description.Usage, description.BindFlags, description.CPUAccessFlags, description.MiscFlags)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1D"/> class using the provided <see cref="Texture1DDescription"/> and a single <see cref="SubresourceData"/> for initial data.
        /// </summary>
        /// <param name="description">The <see cref="Texture1DDescription"/> used to create the texture.</param>
        /// <param name="initialData">A single <see cref="SubresourceData"/> containing the initial data for the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture1D(Texture1DDescription description, SubresourceData initialData, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description, [initialData], description.Format, description.Usage, description.BindFlags, description.CPUAccessFlags, description.MiscFlags)
        {
        }

        public override ResourceDimension Dimension { get; } = ResourceDimension.Texture1D;

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

        protected override IRenderTargetView CreateRTV(IGraphicsDevice device, ITexture1D resource, Texture1DDescription desc)
        {
            return device.CreateRenderTargetView(texture, new(texture, arraySize > 1));
        }

        protected override IShaderResourceView CreateSRV(IGraphicsDevice device, ITexture1D resource, Texture1DDescription desc)
        {
            return device.CreateShaderResourceView(texture, new(texture, arraySize > 1));
        }

        protected override IUnorderedAccessView CreateUAV(IGraphicsDevice device, ITexture1D resource, Texture1DDescription desc)
        {
            return device.CreateUnorderedAccessView(texture, new(texture, arraySize > 1));
        }

        protected override ITexture1D CreateTexture(IGraphicsDevice device, Texture1DDescription desc, SubresourceData[]? initialData, out BindFlags bindFlags)
        {
            var texture = device.CreateTexture1D(description, initialData);
            desc = texture.Description;
            bindFlags = desc.BindFlags;
            Update();
            return texture;
        }

        protected override ITexture1D CreateTexture(IGraphicsDevice device, TextureFileDescription fileDesc, out Texture1DDescription desc, out BindFlags bindFlags)
        {
            var texture = device.TextureLoader.LoadTexture1D(fileDesc);
            desc = texture.Description;
            bindFlags = desc.BindFlags;
            Update();
            return texture;
        }

        protected override ITexture1D CreateTexture(IGraphicsDevice device, IScratchImage scratchImage, Texture1DDescription desc, out BindFlags bindFlags)
        {
            AccessHelper.Convert(cpuAccessFlags, gpuAccessFlags, out Usage usage, out bindFlags);
            var texture = scratchImage.CreateTexture1D(device, usage, bindFlags, cpuAccessFlags, miscFlag);
            desc = texture.Description;
            bindFlags = desc.BindFlags;
            Update();
            return texture;
        }

        protected override Point3 GetDimensions()
        {
            return new(width, 1, arraySize);
        }

        protected override void CreateArraySlices(IGraphicsDevice device, ITexture1D resource, Texture1DDescription desc, out IRenderTargetView[]? rtvArraySlices, out IShaderResourceView[]? srvArraySlices, out IUnorderedAccessView[]? uavArraySlices)
        {
            rtvArraySlices = null;
            srvArraySlices = null;
            uavArraySlices = null;

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtvArraySlices = new IRenderTargetView[description.ArraySize];
                for (int i = 0; i < description.ArraySize; i++)
                {
                    rtvArraySlices[i] = device.CreateRenderTargetView(resource, new RenderTargetViewDescription(resource, true, firstArraySlice: i, arraySize: 1));
                }
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srvArraySlices = new IShaderResourceView[description.ArraySize];
                for (int i = 0; i < description.ArraySize; i++)
                {
                    srvArraySlices[i] = device.CreateShaderResourceView(resource, new ShaderResourceViewDescription(resource, true, firstArraySlice: i, arraySize: 1));
                }
            }

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uavArraySlices = new IUnorderedAccessView[description.ArraySize];
                for (int i = 0; i < description.ArraySize; i++)
                {
                    uavArraySlices[i] = device.CreateUnorderedAccessView(resource, new UnorderedAccessViewDescription(resource, true, firstArraySlice: i, arraySize: 1));
                }
            }
        }

        private void Update()
        {
            format = description.Format;
            width = description.Width;
            mipLevels = description.MipLevels;
            arraySize = description.ArraySize;
            AccessHelper.Convert(description.Usage, description.BindFlags, out cpuAccessFlags, out gpuAccessFlags);
            miscFlag = description.MiscFlags;
        }

        /// <summary>
        /// Resizes the texture with the specified parameters.
        /// </summary>
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
        public void Resize(Format format, int width, int arraySize, int mipLevels, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            this.format = format;
            this.width = width;
            this.mipLevels = mipLevels;
            this.arraySize = arraySize;
            this.cpuAccessFlags = cpuAccessFlags;
            this.gpuAccessFlags = gpuAccessFlags;
            this.miscFlag = miscFlag;
            AccessHelper.Convert(cpuAccessFlags, gpuAccessFlags, out var usage, out var bindFlags);
            description = new(format, width, arraySize, mipLevels, bindFlags, usage, cpuAccessFlags, miscFlag);

            DisposeCore(); // note this will not trigger the OnDispose event or set the IsDisposed value.

            // recreate texture.
            texture = CreateTexture(Application.GraphicsDevice, description, null, out _);
            texture.DebugName = dbgName;
            SetupTexture(bindFlags);
        }
    }
}