namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Runtime.CompilerServices;
    using static HexaEngine.Core.Utilities;

    public unsafe class RenderTextureArray : IDisposable
    {
        private readonly IRenderTargetView[] rtvs;
        private bool disposedValue;

        public int Width { get; }

        public int Height { get; }

        public Viewport Viewport => new(Width, Height);

        public readonly uint Count;

        public readonly IShaderResourceView[] SRVs;
        public readonly void** RTVs;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RenderTextureArray(IGraphicsDevice device, int width, int height, uint count = 1, Format format = Format.RGBA32Float)
        {
            Count = count;
            Width = width;
            Height = height;
            rtvs = new IRenderTargetView[count];
            SRVs = new IShaderResourceView[count];
            RTVs = Alloc((uint)sizeof(nint), count);
            for (int i = 0; i < count; i++)
            {
                ITexture2D tex = device.CreateTexture2D(format, Width, Height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
                SRVs[i] = device.CreateShaderResourceView(tex);
                rtvs[i] = device.CreateRenderTargetView(tex, new(Width, Height));
                RTVs[i] = (void*)rtvs[i].NativePointer;
                tex.Dispose();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                foreach (IRenderTargetView rtv in rtvs)
                    rtv.Dispose();
                foreach (IShaderResourceView srv in SRVs)
                    srv.Dispose();

                Free(RTVs);

                disposedValue = true;
            }
        }

        ~RenderTextureArray()
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