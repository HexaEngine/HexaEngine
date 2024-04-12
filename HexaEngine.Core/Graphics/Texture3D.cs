namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;
    using System.Runtime.CompilerServices;

    public sealed unsafe class Texture3D : Texture<ITexture3D, Texture3DDescription>, ITexture3D, IShaderResourceView, IRenderTargetView, IUnorderedAccessView
    {
        private int width;
        private int height;
        private int depth;
        private int mipLevels;

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture3D"/> class using the specified <see cref="TextureFileDescription"/>.
        /// </summary>
        /// <param name="description">The <see cref="TextureFileDescription"/> used to create the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture3D(TextureFileDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture3D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture3D"/> class using the specified <see cref="IScratchImage"/>.
        /// </summary>
        /// <param name="description">The <see cref="Texture3DDescription"/> used to create the texture.</param>
        /// <param name="scratchImage">The source image of the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture3D(Texture3DDescription description, IScratchImage scratchImage, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture3D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description, scratchImage, description.Format, description.Usage, description.BindFlags, description.CPUAccessFlags, description.MiscFlags)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture3D"/> class using the provided <see cref="Texture3DDescription"/>.
        /// </summary>
        /// <param name="description">The <see cref="Texture3DDescription"/> used to create the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture3D(Texture3DDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture3D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description, description.Format, description.Usage, description.BindFlags, description.CPUAccessFlags, description.MiscFlags)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture3D"/> class with the specified parameters.
        /// </summary>
        /// <param name="format">The format of the texture.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height"></param>
        /// <param name="depth">The array size of the texture.</param>
        /// <param name="mipLevels">The number of mip levels for the texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the texture.</param>
        /// <param name="gpuAccessFlags">The GPU access flags for the texture (default is read-only).</param>
        /// <param name="miscFlag">The miscellaneous flags for the texture (default is none).</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture3D(Format format, int width, int height, int depth, int mipLevels, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture3D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", new Texture3DDescription(format, width, height, depth, mipLevels, gpuAccessFlags, cpuAccessFlags, miscFlag), format, gpuAccessFlags, cpuAccessFlags, miscFlag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture3D"/> class using the provided <see cref="Texture3DDescription"/> and initial data.
        /// </summary>
        /// <param name="description">The <see cref="Texture3DDescription"/> used to create the texture.</param>
        /// <param name="initialData">An array of <see cref="SubresourceData"/> containing the initial data for the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture3D(Texture3DDescription description, SubresourceData[] initialData, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture3D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description, initialData, description.Format, description.Usage, description.BindFlags, description.CPUAccessFlags, description.MiscFlags)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture3D"/> class using the provided <see cref="Texture3DDescription"/> and a single <see cref="SubresourceData"/> for initial data.
        /// </summary>
        /// <param name="description">The <see cref="Texture3DDescription"/> used to create the texture.</param>
        /// <param name="initialData">A single <see cref="SubresourceData"/> containing the initial data for the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture3D(Texture3DDescription description, SubresourceData initialData, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture3D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description, [initialData], description.Format, description.Usage, description.BindFlags, description.CPUAccessFlags, description.MiscFlags)
        {
        }

        /// <summary>
        /// Gets the resource dimension of the texture, which is always <see cref="ResourceDimension.Texture3D"/>.
        /// </summary>
        public override ResourceDimension Dimension { get; } = ResourceDimension.Texture3D;

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
        /// Gets a viewport with the dimensions of the texture.
        /// </summary>
        public Viewport Viewport => new(0, 0, width, height, 0, depth);

        protected override IRenderTargetView CreateRTV(IGraphicsDevice device, ITexture3D resource, Texture3DDescription desc)
        {
            return device.CreateRenderTargetView(texture, new(resource));
        }

        protected override IShaderResourceView CreateSRV(IGraphicsDevice device, ITexture3D resource, Texture3DDescription desc)
        {
            return device.CreateShaderResourceView(texture, new(texture));
        }

        protected override IUnorderedAccessView CreateUAV(IGraphicsDevice device, ITexture3D resource, Texture3DDescription desc)
        {
            return device.CreateUnorderedAccessView(texture, new(texture, UnorderedAccessViewDimension.Texture3D));
        }

        protected override ITexture3D CreateTexture(IGraphicsDevice device, Texture3DDescription desc, SubresourceData[]? initialData, out BindFlags bindFlags)
        {
            var texture = device.CreateTexture3D(desc, initialData);
            desc = texture.Description;
            bindFlags = desc.BindFlags;
            Update();
            return texture;
        }

        protected override ITexture3D CreateTexture(IGraphicsDevice device, TextureFileDescription fileDesc, out Texture3DDescription desc, out BindFlags bindFlags)
        {
            var texture = device.TextureLoader.LoadTexture3D(fileDesc);
            desc = texture.Description;
            bindFlags = desc.BindFlags;
            Update();
            return texture;
        }

        protected override ITexture3D CreateTexture(IGraphicsDevice device, IScratchImage scratchImage, Texture3DDescription desc, out BindFlags bindFlags)
        {
            AccessHelper.Convert(cpuAccessFlags, gpuAccessFlags, out Usage usage, out bindFlags);
            var texture = scratchImage.CreateTexture3D(device, usage, bindFlags, cpuAccessFlags, miscFlag);
            desc = texture.Description;
            bindFlags = desc.BindFlags;
            Update();
            return texture;
        }

        protected override Point3 GetDimensions()
        {
            return new Point3(width, height, depth);
        }

        protected override void CreateArraySlices(IGraphicsDevice device, ITexture3D resource, Texture3DDescription desc, out IRenderTargetView[]? rtvArraySlices, out IShaderResourceView[]? srvArraySlices, out IUnorderedAccessView[]? uavArraySlices)
        {
            rtvArraySlices = null;
            srvArraySlices = null;
            uavArraySlices = null;
        }

        private void Update()
        {
            format = description.Format;
            width = description.Width;
            height = description.Height;
            depth = description.Depth;
            mipLevels = description.MipLevels;
            AccessHelper.Convert(description.Usage, description.BindFlags, out cpuAccessFlags, out gpuAccessFlags);
            miscFlag = description.MiscFlags;
        }

        /// <summary>
        /// Resizes the texture with the specified parameters.
        /// </summary>
        /// <param name="format">The format of the resized texture.</param>
        /// <param name="width">The width of the resized texture.</param>
        /// <param name="height">The height of the resized texture.</param>
        /// <param name="depth">The array size of the resized texture.</param>
        /// <param name="mipLevels">The number of mip levels for the resized texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the resized texture.</param>
        /// <param name="gpuAccessFlags">The GPU access flags for the resized texture (default is GpuAccessFlags.Read).</param>
        /// <param name="miscFlag">The miscellaneous flags for the resized texture (default is ResourceMiscFlag.None).</param>
        /// <remarks>
        /// This method resizes the texture to the specified dimensions with the provided access flags and format. If CPU and GPU access flags
        /// conflict or GPU access flags are used for reading, appropriate bindings and usage are adjusted. After resizing, the previous texture
        /// is disposed, and new resources are created accordingly. The texture will be ready to use for rendering, shader resource, or unordered access.
        /// </remarks>
        public void Resize(Format format, int width, int height, int depth, int mipLevels, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            this.format = format;
            this.width = width;
            this.height = height;
            this.mipLevels = mipLevels;
            this.depth = depth;
            this.cpuAccessFlags = cpuAccessFlags;
            this.gpuAccessFlags = gpuAccessFlags;
            this.miscFlag = miscFlag;
            AccessHelper.Convert(cpuAccessFlags, gpuAccessFlags, out var usage, out var bindFlags);
            description = new(format, width, height, depth, mipLevels, bindFlags, usage, cpuAccessFlags, miscFlag);

            DisposeCore(); // note this will not trigger the OnDispose event or set the IsDisposed value.

            // recreate texture.
            texture = CreateTexture(Application.GraphicsDevice, description, null, out _);
            texture.DebugName = dbgName;
            SetupTexture(bindFlags);
        }

        /// <summary>
        /// Writes data to a specific subresource of the texture using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to write the data.</param>
        /// <param name="subresource">The index of the subresource to write.</param>
        /// <param name="data"></param>
        /// <param name="dataSize"></param>
        public void Write(IGraphicsContext context, int subresource, void* data, uint dataSize)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException();
            }
            if (CanRead)
            {
                var mapped = context.Map(texture, subresource, MapMode.Write, MapFlags.None);
                Memcpy(data, mapped.PData, (uint)height * mapped.RowPitch, dataSize);
                context.Unmap(texture, subresource);
            }
            else
            {
                var mapped = context.Map(texture, subresource, MapMode.WriteDiscard, MapFlags.None);
                Memcpy(data, mapped.PData, (uint)height * mapped.RowPitch, dataSize);
                context.Unmap(texture, subresource);
            }
        }

        /// <summary>
        /// Reads data from a specific subresource of the texture using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to read the data.</param>
        /// <param name="subresource">The index of the subresource to read.</param>
        /// <param name="data"></param>
        /// <param name="dataSize"></param>
        public void Read(IGraphicsContext context, int subresource, void* data, uint dataSize)
        {
            if (!CanRead)
            {
                throw new InvalidOperationException();
            }

            var mapped = context.Map(texture, subresource, MapMode.Read, MapFlags.None);
            Memcpy(mapped.PData, data, dataSize, (uint)height * mapped.RowPitch);
            context.Unmap(texture, subresource);
        }
    }
}