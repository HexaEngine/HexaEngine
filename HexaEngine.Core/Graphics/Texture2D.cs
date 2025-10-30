namespace HexaEngine.Core.Graphics
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.IO;
    using System.Runtime.CompilerServices;

    public sealed unsafe class Texture2D : Texture<ITexture2D, Texture2DDescription>, ITexture2D, IShaderResourceView, IRenderTargetView, IUnorderedAccessView
    {
        private int width;
        private int height;
        private int mipLevels;
        private int arraySize;
        private SampleDescription sampleDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2D"/> class using the specified <see cref="TextureFileDescription"/>.
        /// </summary>
        /// <param name="description">The <see cref="TextureFileDescription"/> used to create the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture2D(TextureFileDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture2D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2D"/> class using the specified <see cref="IScratchImage"/>.
        /// </summary>
        /// <param name="description">The <see cref="Texture2DDescription"/> used to create the texture.</param>
        /// <param name="scratchImage">The source image of the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture2D(Texture2DDescription description, IScratchImage scratchImage, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture2D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description, scratchImage, description.Format, description.Usage, description.BindFlags, description.CPUAccessFlags, description.MiscFlags)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2D"/> class using the provided <see cref="Texture2DDescription"/>.
        /// </summary>
        /// <param name="description">The <see cref="Texture2DDescription"/> used to create the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture2D(Texture2DDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture2D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description, description.Format, description.Usage, description.BindFlags, description.CPUAccessFlags, description.MiscFlags)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2D"/> class with the specified parameters.
        /// </summary>
        /// <param name="format">The format of the texture.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height"></param>
        /// <param name="arraySize">The array size of the texture.</param>
        /// <param name="mipLevels">The number of mip levels for the texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the texture.</param>
        /// <param name="gpuAccessFlags">The GPU access flags for the texture (default is read-only).</param>
        /// <param name="sampleCount">Specifies multisampling parameters for the texture.</param>
        /// <param name="sampleQuality">Specifies multisampling parameters for the texture.</param>
        /// <param name="miscFlag">The miscellaneous flags for the texture (default is none).</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture2D(Format format, int width, int height, int arraySize, int mipLevels, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, int sampleCount = 1, int sampleQuality = 0, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture2D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", new Texture2DDescription(format, width, height, arraySize, mipLevels, gpuAccessFlags, cpuAccessFlags, sampleCount, sampleQuality, miscFlag), format, gpuAccessFlags, cpuAccessFlags, miscFlag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2D"/> class using the provided <see cref="Texture2DDescription"/> and initial data.
        /// </summary>
        /// <param name="description">The <see cref="Texture2DDescription"/> used to create the texture.</param>
        /// <param name="initialData">An array of <see cref="SubresourceData"/> containing the initial data for the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture2D(Texture2DDescription description, SubresourceData[] initialData, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture2D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description, initialData, description.Format, description.Usage, description.BindFlags, description.CPUAccessFlags, description.MiscFlags)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2D"/> class using the provided <see cref="Texture2DDescription"/> and a single <see cref="SubresourceData"/> for initial data.
        /// </summary>
        /// <param name="description">The <see cref="Texture2DDescription"/> used to create the texture.</param>
        /// <param name="initialData">A single <see cref="SubresourceData"/> containing the initial data for the texture.</param>
        /// <param name="filename">The file path of the source code file where this constructor is called. (auto-generated)</param>
        /// <param name="lineNumber">The line number in the source code file where this constructor is called. (auto-generated)</param>
        public Texture2D(Texture2DDescription description, SubresourceData initialData, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) :
            base($"Texture2D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}", description, [initialData], description.Format, description.Usage, description.BindFlags, description.CPUAccessFlags, description.MiscFlags)
        {
        }

        /// <summary>
        /// Gets the resource dimension of the texture, which is always <see cref="ResourceDimension.Texture2D"/>.
        /// </summary>
        public override ResourceDimension Dimension { get; } = ResourceDimension.Texture2D;

        /// <summary>
        /// Gets the width of the texture.
        /// </summary>
        public int Width => width;

        /// <summary>
        /// Gets the height of the texture.
        /// </summary>
        public int Height => height;

        /// <summary>
        /// Gets the number of mip levels in the texture.
        /// </summary>
        public int MipLevels => mipLevels;

        /// <summary>
        /// Gets the array size of the texture.
        /// </summary>
        public int ArraySize => arraySize;

        /// <summary>
        /// Gets the <see cref="Graphics.SampleDescription"/> of the texture.
        /// </summary>
        public SampleDescription SampleDescription => sampleDescription;

        /// <summary>
        /// Gets the <see cref="Hexa.NET.Mathematics.Viewport"/> of the texture.
        /// </summary>
        public Viewport Viewport => new(width, height);

        protected override IRenderTargetView CreateRTV(IGraphicsDevice device, ITexture2D resource, Texture2DDescription desc)
        {
            bool ms = desc.SampleDescription.Count > 1;
            RenderTargetViewDimension renderTargetViewDimension = ms ? RenderTargetViewDimension.Texture2DMultisampled : RenderTargetViewDimension.Texture2D;

            if (desc.ArraySize > 1)
            {
                renderTargetViewDimension = ms ? RenderTargetViewDimension.Texture2DMultisampledArray : RenderTargetViewDimension.Texture2DArray;
            }

            return device.CreateRenderTargetView(texture, new(texture, renderTargetViewDimension));
        }

        protected override IShaderResourceView CreateSRV(IGraphicsDevice device, ITexture2D resource, Texture2DDescription desc)
        {
            bool ms = desc.SampleDescription.Count > 1;
            ShaderResourceViewDimension shaderResourceViewDimension = ms ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D;

            if (desc.ArraySize > 1)
            {
                shaderResourceViewDimension = ms ? ShaderResourceViewDimension.Texture2DMultisampledArray : ShaderResourceViewDimension.Texture2DArray;
            }

            if ((desc.MiscFlags & ResourceMiscFlag.TextureCube) != 0)
            {
                shaderResourceViewDimension = ShaderResourceViewDimension.TextureCube;
                if (arraySize > 6)
                {
                    shaderResourceViewDimension = ShaderResourceViewDimension.TextureCubeArray;
                }
            }

            return device.CreateShaderResourceView(texture, new(texture, shaderResourceViewDimension));
        }

        protected override IUnorderedAccessView CreateUAV(IGraphicsDevice device, ITexture2D resource, Texture2DDescription desc)
        {
            return device.CreateUnorderedAccessView(texture, new(texture, arraySize > 1 ? UnorderedAccessViewDimension.Texture2DArray : UnorderedAccessViewDimension.Texture2D));
        }

        protected override ITexture2D CreateTexture(IGraphicsDevice device, Texture2DDescription desc, SubresourceData[]? initialData, out BindFlags bindFlags)
        {
            var texture = device.CreateTexture2D(desc, initialData);
            desc = texture.Description;
            bindFlags = desc.BindFlags;
            Update();
            return texture;
        }

        protected override ITexture2D CreateTexture(IGraphicsDevice device, TextureFileDescription fileDesc, out Texture2DDescription desc, out BindFlags bindFlags)
        {
            var texture = device.TextureLoader.LoadTexture2D(fileDesc);
            desc = texture.Description;
            bindFlags = desc.BindFlags;
            Update();
            return texture;
        }

        protected override ITexture2D CreateTexture(IGraphicsDevice device, IScratchImage scratchImage, Texture2DDescription desc, out BindFlags bindFlags)
        {
            AccessHelper.Convert(cpuAccessFlags, gpuAccessFlags, out Usage usage, out bindFlags);
            var texture = scratchImage.CreateTexture2D(device, usage, bindFlags, cpuAccessFlags, miscFlag);
            desc = texture.Description;
            bindFlags = desc.BindFlags;
            Update();
            return texture;
        }

        protected override Point3 GetDimensions()
        {
            return new Point3(width, height, arraySize);
        }

        protected override void CreateArraySlices(IGraphicsDevice device, ITexture2D resource, Texture2DDescription desc, out IRenderTargetView[]? rtvArraySlices, out IShaderResourceView[]? srvArraySlices, out IUnorderedAccessView[]? uavArraySlices)
        {
            rtvArraySlices = null;
            srvArraySlices = null;
            uavArraySlices = null;

            bool ms = desc.SampleDescription.Count > 1;

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                RenderTargetViewDimension renderTargetViewDimension = ms ? RenderTargetViewDimension.Texture2DMultisampledArray : RenderTargetViewDimension.Texture2DArray;

                rtvArraySlices = new IRenderTargetView[description.ArraySize];
                for (int i = 0; i < description.ArraySize; i++)
                {
                    rtvArraySlices[i] = device.CreateRenderTargetView(resource, new RenderTargetViewDescription(resource, renderTargetViewDimension, firstArraySlice: i, arraySize: 1));
                }
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                ShaderResourceViewDimension shaderResourceViewDimension = ms ? ShaderResourceViewDimension.Texture2DMultisampledArray : ShaderResourceViewDimension.Texture2DArray;

                srvArraySlices = new IShaderResourceView[description.ArraySize];
                for (int i = 0; i < description.ArraySize; i++)
                {
                    srvArraySlices[i] = device.CreateShaderResourceView(resource, new ShaderResourceViewDescription(resource, shaderResourceViewDimension, firstArraySlice: i, arraySize: 1));
                }
            }

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uavArraySlices = new IUnorderedAccessView[description.ArraySize];
                for (int i = 0; i < description.ArraySize; i++)
                {
                    uavArraySlices[i] = device.CreateUnorderedAccessView(resource, new UnorderedAccessViewDescription(resource, UnorderedAccessViewDimension.Texture2DArray, firstArraySlice: i, arraySize: 1));
                }
            }
        }

        private void Update()
        {
            format = description.Format;
            width = description.Width;
            height = description.Height;
            mipLevels = description.MipLevels;
            arraySize = description.ArraySize;
            AccessHelper.Convert(description.Usage, description.BindFlags, out cpuAccessFlags, out gpuAccessFlags);
            miscFlag = description.MiscFlags;
            sampleDescription = description.SampleDescription;
        }

        /// <summary>
        /// Resizes the texture with the specified parameters.
        /// </summary>
        /// <param name="format">The format of the resized texture.</param>
        /// <param name="width">The width of the resized texture.</param>
        /// <param name="height">The height of the resized texture.</param>
        /// <param name="arraySize">The array size of the resized texture.</param>
        /// <param name="mipLevels">The number of mip levels for the resized texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the resized texture.</param>
        /// <param name="gpuAccessFlags">The GPU access flags for the resized texture (default is GpuAccessFlags.Read).</param>
        /// <param name="sampleCount">Specifies multisampling parameters for the texture.</param>
        /// <param name="sampleQuality">Specifies multisampling parameters for the texture.</param>
        /// <param name="miscFlag">The miscellaneous flags for the resized texture (default is ResourceMiscFlag.None).</param>
        /// <remarks>
        /// This method resizes the texture to the specified dimensions with the provided access flags and format. If CPU and GPU access flags
        /// conflict or GPU access flags are used for reading, appropriate bindings and usage are adjusted. After resizing, the previous texture
        /// is disposed, and new resources are created accordingly. The texture will be ready to use for rendering, shader resource, or unordered access.
        /// </remarks>
        public void Resize(Format format, int width, int height, int arraySize, int mipLevels, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, int sampleCount = 1, int sampleQuality = 0, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            this.format = format;
            this.width = width;
            this.height = height;
            this.mipLevels = mipLevels;
            this.arraySize = arraySize;
            this.cpuAccessFlags = cpuAccessFlags;
            this.gpuAccessFlags = gpuAccessFlags;
            this.miscFlag = miscFlag;
            AccessHelper.Convert(cpuAccessFlags, gpuAccessFlags, out var usage, out var bindFlags);
            description = new(format, width, height, arraySize, mipLevels, bindFlags, usage, cpuAccessFlags, sampleCount, sampleQuality, miscFlag);

            DisposeCore(); // note this will not trigger the OnDispose event or set the IsDisposed value.

            // recreate texture.
            texture = CreateTexture(Application.GraphicsDevice, description, null, out _);
            texture.DebugName = dbgName;
            SetupTexture(bindFlags);
        }

        /// <summary>
        /// Writes the modified data to the texture using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to write the data.</param>
        /// <param name="data"></param>
        /// <param name="dataSize"></param>
        public void Write(IGraphicsContext context, void* data, uint dataSize)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException("Writing is not allowed.");
            }

            for (int i = 0; i < mipLevels; i++)
            {
                for (int j = 0; j < arraySize; j++)
                {
                    Write(context, TextureHelper.ComputeSubresourceIndex2D(mipLevels, arraySize, i, j), data, dataSize);
                }
            }
        }

        /// <summary>
        /// Writes data to a specific subresource of the texture using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to write the data.</param>
        /// <param name="subresourceIndex">The index of the subresource to write.</param>
        /// <param name="data"></param>
        /// <param name="dataSize"></param>
        public void Write(IGraphicsContext context, int subresourceIndex, void* data, uint dataSize)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException("Writing is not allowed.");
            }
            if (CanRead)
            {
                var mapped = context.Map(texture, subresourceIndex, MapMode.Write, MapFlags.None);
                Memcpy(data, mapped.PData, (uint)description.Height * mapped.RowPitch, dataSize);
                context.Unmap(texture, subresourceIndex);
            }
            else
            {
                var mapped = context.Map(texture, subresourceIndex, MapMode.WriteDiscard, MapFlags.None);
                Memcpy(data, mapped.PData, (uint)description.Height * mapped.RowPitch, dataSize);
                context.Unmap(texture, subresourceIndex);
            }
        }

        /// <summary>
        /// Reads data from the texture using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to read the data.</param>
        /// <param name="data"></param>
        /// <param name="dataSize"></param>
        public void Read(IGraphicsContext context, void* data, uint dataSize)
        {
            if (!CanRead)
            {
                throw new InvalidOperationException("Reading is not allowed.");
            }

            for (int i = 0; i < mipLevels; i++)
            {
                for (int j = 0; j < arraySize; j++)
                {
                    Read(context, TextureHelper.ComputeSubresourceIndex2D(mipLevels, arraySize, i, j), data, dataSize);
                }
            }
        }

        /// <summary>
        /// Reads data from a specific subresource of the texture using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to read the data.</param>
        /// <param name="subresourceIndex">The index of the subresource to read.</param>
        /// <param name="data"></param>
        /// <param name="dataSize"></param>
        public void Read(IGraphicsContext context, int subresourceIndex, void* data, uint dataSize)
        {
            if (!CanRead)
            {
                throw new InvalidOperationException("Reading is not allowed.");
            }

            var mapped = context.Map(texture, subresourceIndex, MapMode.Read, MapFlags.None);
            Memcpy(mapped.PData, data, dataSize, (uint)description.Height * mapped.RowPitch);
            context.Unmap(texture, subresourceIndex);
        }

        /// <summary>
        /// Loads a <see cref="Texture2D"/> from assets and returns it.
        /// </summary>
        /// <param name="path">The path to the texture asset.</param>
        /// <returns>The loaded <see cref="Texture2D"/> object.</returns>
        [Obsolete("Use AssetPath overload instead.")]
        public static Texture2D LoadFromAssets(string path)
        {
            return new(new TextureFileDescription(new(path)));
        }

        /// <summary>
        /// Loads a <see cref="Texture2D"/> from assets and returns it.
        /// </summary>
        /// <param name="path">The path to the texture asset.</param>
        /// <returns>The loaded <see cref="Texture2D"/> object.</returns>
        public static Texture2D LoadFromAssets(AssetPath path)
        {
            return new(new TextureFileDescription(path));
        }

        /// <summary>
        /// Loads a <see cref="Texture2D"/> from assets and returns it.
        /// </summary>
        /// <param name="assetRef">The path to the texture asset.</param>
        /// <returns>The loaded <see cref="Texture2D"/> object.</returns>
        public static Texture2D LoadFromAssets(AssetRef assetRef)
        {
            return new(new TextureFileDescription(assetRef));
        }

        /// <summary>
        /// Loads a <see cref="Texture2D"/> from assets and returns it.
        /// </summary>
        /// <param name="assetRef">The path to the texture asset.</param>
        /// <param name="forceDim"></param>
        /// <returns>The loaded <see cref="Texture2D"/> object.</returns>
        public static Texture2D LoadFromAssets(AssetRef assetRef, TextureDimension forceDim)
        {
            return new(new TextureFileDescription(assetRef, forceDim));
        }
    }
}