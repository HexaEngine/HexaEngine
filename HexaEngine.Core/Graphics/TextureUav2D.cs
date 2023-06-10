namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Runtime.CompilerServices;

    public unsafe class TextureUav2D : ITexture2D
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

        public TextureUav2D(IGraphicsDevice device, Format format, int width, int height, int arraySize, int mipLevels, bool isSRV, bool isRTV, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"RWTexture2D: {filename}, Line:{lineNumber}";
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
                rtv = device.CreateRenderTargetView(texture, new(width, height));
                rtv.DebugName = dbgName + ".RTV";
            }
        }

        public ResourceDimension Dimension => ResourceDimension.Texture2D;

        public Texture2DDescription Description => description;

        public Format Format => format;

        public int Width => width;

        public int Height => height;

        public int MipLevels => mipLevels;

        public int ArraySize => arraySize;

        public ResourceMiscFlag MiscFlag => miscFlag;

        public IShaderResourceView? SRV => srv;

        public IUnorderedAccessView? UAV => uav;

        public nint NativePointer => texture.NativePointer;

        public string? DebugName { get => texture.DebugName; set => texture.DebugName = value; }

        public bool IsDisposed => texture.IsDisposed;

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
                rtv = device.CreateRenderTargetView(texture, new(width, height));
                rtv.DebugName = dbgName + ".RTV";
            }
        }

        public void CopyTo(IGraphicsContext context, ITexture2D texture)
        {
            context.CopyResource(texture, this.texture);
        }

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

        ~TextureUav2D()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}