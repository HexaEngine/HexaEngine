namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// [Obsolete] Use Texture2D instead with UA flag
    /// </summary>
    [Obsolete("Use Texture2D instead with UA flag")]
    public unsafe class UavTexture2D : ITexture2D
    {
        private readonly string dbgName;
        private Texture2DDescription description;
        private Format format;
        private int width;
        private int height;
        private int mipLevels;
        private int arraySize;
        private ResourceMiscFlag miscFlag;
        private ITexture2D texture;
        private IShaderResourceView? srv;
        private IRenderTargetView? rtv;
        private IUnorderedAccessView? uav;
        private bool disposedValue;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        /// <param name="device"></param>
        /// <param name="format"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="arraySize"></param>
        /// <param name="mipLevels"></param>
        /// <param name="isSRV"></param>
        /// <param name="isRTV"></param>
        /// <param name="miscFlag"></param>
        /// <param name="filename"></param>
        /// <param name="lineNumber"></param>
        public UavTexture2D(IGraphicsDevice device, Format format, int width, int height, int arraySize, int mipLevels, bool isSRV, bool isRTV, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"RWTexture2D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            this.format = format;
            this.width = width;
            this.height = height;
            this.mipLevels = mipLevels;
            this.arraySize = arraySize;
            this.miscFlag = miscFlag;
            description = new(format, width, height, arraySize, mipLevels, BindFlags.UnorderedAccess, Usage.Default, CpuAccessFlags.None, 1, 0, miscFlag);

            if (isSRV)
                description.BindFlags |= BindFlags.ShaderResource;
            if (isRTV)
                description.BindFlags |= BindFlags.RenderTarget;

            texture = device.CreateTexture2D(description);
            texture.DebugName = dbgName;

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                UnorderedAccessViewDescription description = new(texture, this.arraySize > 1 ? UnorderedAccessViewDimension.Texture2DArray : UnorderedAccessViewDimension.Texture2D);
                uav = device.CreateUnorderedAccessView(texture, description);
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

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public ResourceDimension Dimension => ResourceDimension.Texture2D;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public Texture2DDescription Description => description;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public Format Format => format;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public int Width => width;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public int Height => height;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public int MipLevels => mipLevels;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public int ArraySize => arraySize;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public ResourceMiscFlag MiscFlag => miscFlag;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public IShaderResourceView? SRV => srv;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public IRenderTargetView? RTV => rtv;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public IUnorderedAccessView? UAV => uav;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public Viewport Viewport => new(width, height);

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public nint NativePointer => texture.NativePointer;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public string? DebugName { get => texture.DebugName; set => texture.DebugName = value; }

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public bool IsDisposed => texture.IsDisposed;

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public event EventHandler? OnDisposed
        {
            add
            {
                texture.OnDisposed += value;
            }

            remove
            {
                texture.OnDisposed -= value;
            }
        }

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        /// <param name="device"></param>
        /// <param name="format"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="arraySize"></param>
        /// <param name="mipLevels"></param>
        /// <param name="isSRV"></param>
        /// <param name="isRTV"></param>
        /// <param name="miscFlag"></param>
        public void Resize(IGraphicsDevice device, Format format, int width, int height, int arraySize, int mipLevels, bool isSRV, bool isRTV, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            this.format = format;
            this.width = width;
            this.height = height;
            this.mipLevels = mipLevels;
            this.arraySize = arraySize;
            this.miscFlag = miscFlag;
            description = new(format, width, height, arraySize, mipLevels, BindFlags.UnorderedAccess, Usage.Default, CpuAccessFlags.None, 1, 0, miscFlag);

            if (isSRV)
                description.BindFlags |= BindFlags.ShaderResource;
            if (isRTV)
                description.BindFlags |= BindFlags.RenderTarget;
            texture.Dispose();
            uav?.Dispose();
            srv?.Dispose();
            rtv?.Dispose();
            MemoryManager.Unregister(texture);
            texture = device.CreateTexture2D(description);
            texture.DebugName = dbgName;
            MemoryManager.Register(texture);

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                UnorderedAccessViewDescription description = new(texture, this.arraySize > 1 ? UnorderedAccessViewDimension.Texture2DArray : UnorderedAccessViewDimension.Texture2D);
                uav = device.CreateUnorderedAccessView(texture, description);
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
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        /// <param name="context"></param>
        /// <param name="texture"></param>
        public void CopyTo(IGraphicsContext context, ITexture2D texture)
        {
            context.CopyResource(texture, this.texture);
        }

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                texture.Dispose();
                uav?.Dispose();
                srv?.Dispose();
                rtv?.Dispose();

                disposedValue = true;

                disposedValue = true;
            }
        }

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        ~UavTexture2D()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// [Obsolete] Use Texture2D instead with UA flag
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}