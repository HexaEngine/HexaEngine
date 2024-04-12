namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;

    public abstract unsafe class Texture<TTexture, TDesc> : DisposableBase, IResource, IShaderResourceView, IRenderTargetView, IUnorderedAccessView where TTexture : IResource where TDesc : struct
    {
        protected readonly string dbgName;
        protected TDesc description;
        protected Format format;
        protected CpuAccessFlags cpuAccessFlags;
        protected GpuAccessFlags gpuAccessFlags;
        protected ResourceMiscFlag miscFlag;
        private bool canWrite;
        private bool canRead;
        protected TTexture texture;
        private IShaderResourceView? srv;
        private IRenderTargetView? rtv;
        private IUnorderedAccessView? uav;
        private IRenderTargetView[]? rtvArraySlices;
        private IShaderResourceView[]? srvArraySlices;
        private IUnorderedAccessView[]? uavArraySlices;
        private int rowPitch;
        private int slicePitch;

        protected Texture(string dbgName, TextureFileDescription description)
        {
            this.dbgName = dbgName;

            texture = CreateTexture(Application.GraphicsDevice, description, out this.description, out var bindFlags);
            texture.DebugName = dbgName;

            SetupTexture(bindFlags);
        }

        protected Texture(string dbgName, TDesc description, IScratchImage scratchImage, Format format, Usage usage, BindFlags bindFlags, CpuAccessFlags cpuAccessFlags, ResourceMiscFlag miscFlag)
        {
            AccessHelper.Convert(usage, bindFlags, out _, out gpuAccessFlags);

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

            this.dbgName = dbgName;
            this.description = description;
            this.format = format;
            this.cpuAccessFlags = cpuAccessFlags;
            this.miscFlag = miscFlag;

            texture = CreateTexture(Application.GraphicsDevice, scratchImage, description, out _);
            texture.DebugName = dbgName;

            SetupTexture(bindFlags);
        }

        protected Texture(string dbgName, TDesc description, Format format, GpuAccessFlags gpuAccessFlags, CpuAccessFlags cpuAccessFlags, ResourceMiscFlag miscFlag)
        {
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

            this.dbgName = dbgName;
            this.description = description;
            this.format = format;
            this.cpuAccessFlags = cpuAccessFlags;
            this.gpuAccessFlags = gpuAccessFlags;
            this.miscFlag = miscFlag;

            texture = CreateTexture(Application.GraphicsDevice, description, null, out var bindFlags);
            texture.DebugName = dbgName;

            SetupTexture(bindFlags);
        }

        protected Texture(string dbgName, TDesc description, Format format, Usage usage, BindFlags bindFlags, CpuAccessFlags cpuAccessFlags, ResourceMiscFlag miscFlag)
        {
            AccessHelper.Convert(usage, bindFlags, out _, out gpuAccessFlags);

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

            this.dbgName = dbgName;
            this.description = description;
            this.format = format;
            this.cpuAccessFlags = cpuAccessFlags;
            this.miscFlag = miscFlag;

            texture = CreateTexture(Application.GraphicsDevice, description, null, out _);
            texture.DebugName = dbgName;

            SetupTexture(bindFlags);
        }

        protected Texture(string dbgName, TDesc description, SubresourceData[] initialData, Format format, Usage usage, BindFlags bindFlags, CpuAccessFlags cpuAccessFlags, ResourceMiscFlag miscFlag)
        {
            AccessHelper.Convert(usage, bindFlags, out _, out gpuAccessFlags);

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

            this.dbgName = dbgName;
            this.description = description;
            this.format = format;
            this.cpuAccessFlags = cpuAccessFlags;
            this.miscFlag = miscFlag;

            texture = CreateTexture(Application.GraphicsDevice, description, initialData, out _);
            texture.DebugName = dbgName;

            SetupTexture(bindFlags);
        }

        protected abstract TTexture CreateTexture(IGraphicsDevice device, TDesc desc, SubresourceData[]? initialData, out BindFlags bindFlags);

        protected abstract TTexture CreateTexture(IGraphicsDevice device, TextureFileDescription fileDesc, out TDesc desc, out BindFlags bindFlags);

        protected abstract TTexture CreateTexture(IGraphicsDevice device, IScratchImage scratchImage, TDesc desc, out BindFlags bindFlags);

        protected abstract IShaderResourceView CreateSRV(IGraphicsDevice device, TTexture resource, TDesc desc);

        protected abstract IRenderTargetView CreateRTV(IGraphicsDevice device, TTexture resource, TDesc desc);

        protected abstract IUnorderedAccessView CreateUAV(IGraphicsDevice device, TTexture resource, TDesc desc);

        protected abstract void CreateArraySlices(IGraphicsDevice device, TTexture resource, TDesc desc, out IRenderTargetView[]? rtvArraySlices, out IShaderResourceView[]? srvArraySlices, out IUnorderedAccessView[]? uavArraySlices);

        protected abstract Point3 GetDimensions();

        protected void SetupTexture(BindFlags bindFlags)
        {
            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                canWrite = true;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                canRead = true;
            }

            Point3 dimensions = GetDimensions();

            FormatHelper.ComputePitch(format, dimensions.X, dimensions.Y, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if ((bindFlags & BindFlags.UnorderedAccess) != 0)
            {
                uav = CreateUAV(Application.GraphicsDevice, texture, description);
                uav.DebugName = dbgName + ".UAV";
            }
            if ((bindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = CreateSRV(Application.GraphicsDevice, texture, description);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((bindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = CreateRTV(Application.GraphicsDevice, texture, description);
                rtv.DebugName = dbgName + ".RTV";
            }

            MemoryManager.Register(texture);
        }

        /// <summary>
        /// Creates separate views for all array slices
        /// </summary>
        public void CreateArraySlices()
        {
            CreateArraySlices(Application.GraphicsDevice, texture, description, out rtvArraySlices, out srvArraySlices, out uavArraySlices);

            if (rtvArraySlices != null)
            {
                for (int i = 0; i < rtvArraySlices.Length; i++)
                {
                    rtvArraySlices[i].DebugName = dbgName + $".RTV.{i}";
                }
            }

            if (srvArraySlices != null)
            {
                for (int i = 0; i < srvArraySlices.Length; i++)
                {
                    srvArraySlices[i].DebugName = dbgName + $".SRV.{i}";
                }
            }

            if (uavArraySlices != null)
            {
                for (int i = 0; i < uavArraySlices.Length; i++)
                {
                    uavArraySlices[i].DebugName = dbgName + $".UAV.{i}";
                }
            }
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
        /// Gets the dimension of the resource.
        /// </summary>
        public abstract ResourceDimension Dimension { get; }

        /// <summary>
        /// Gets the description of the texture.
        /// </summary>
        public TDesc Description => description;

        /// <summary>
        /// Gets the format of the texture.
        /// </summary>
        public Format Format => format;

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
        /// Gets the native pointer to the underlying texture object.
        /// </summary>
        public nint NativePointer => texture.NativePointer;

        /// <summary>
        /// Gets or sets the debug name for the texture.
        /// </summary>
        public string? DebugName { get => texture.DebugName; set => texture.DebugName = value; }

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
        public IRenderTargetView[]? RTVArraySlices => rtvArraySlices;

        /// <summary>
        /// Gets an array of shader resource views for each array slice of the texture.
        /// </summary>
        public IShaderResourceView[]? SRVArraySlices => srvArraySlices;

        /// <summary>
        /// Gets an array of unordered access views for each array slice of the texture.
        /// </summary>
        public IUnorderedAccessView[]? UAVArraySlices => uavArraySlices;

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

        protected override void DisposeCore()
        {
            MemoryManager.Unregister(texture);
            //if (asset != null)
            //{
            //    asset.OnExistsChanged -= OnExistsChanged;
            //    asset.OnContentChanged -= OnContentChanged;
            //    asset.Dispose();
            //}
            texture.Dispose();
            srv?.Dispose();
            rtv?.Dispose();
            uav?.Dispose();

            if (rtvArraySlices != null)
            {
                for (int i = 0; i < rtvArraySlices.Length; i++)
                {
                    rtvArraySlices[i].Dispose();
                }
                rtvArraySlices = null;
            }

            if (srvArraySlices != null)
            {
                for (int i = 0; i < srvArraySlices.Length; i++)
                {
                    srvArraySlices[i].Dispose();
                }
                srvArraySlices = null;
            }

            if (uavArraySlices != null)
            {
                for (int i = 0; i < uavArraySlices.Length; i++)
                {
                    uavArraySlices[i].Dispose();
                }
                uavArraySlices = null;
            }
        }
    }
}