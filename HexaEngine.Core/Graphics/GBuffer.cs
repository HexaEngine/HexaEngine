namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;
    using System.Runtime.CompilerServices;
    using static HexaEngine.Core.Utils;

    public unsafe class GBuffer : IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly Format[] formats;
        private int width;
        private int height;
        private readonly uint count;
        private readonly IShaderResourceView[] srvs;
        private readonly IRenderTargetView[] rtvs;
        private readonly unsafe void** pSRVs;
        private readonly unsafe void** pRTVs;
        private bool disposedValue;

        public Format[] Formats => formats;

        public int Width => width;

        public int Height => height;

        public uint Count => count;

        public IShaderResourceView[] SRVs => srvs;

        public IRenderTargetView[] RTVs => rtvs;

        public void** PSRVs => pSRVs;

        public void** PRTVs => pRTVs;

        public Viewport Viewport => new(Width, Height);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GBuffer(IGraphicsDevice device, int width, int height, params Format[] formats)
        {
            this.device = device;
            this.formats = formats;
            count = (uint)formats.Length;
            this.width = width;
            this.height = height;
            rtvs = new IRenderTargetView[formats.Length];
            srvs = new IShaderResourceView[formats.Length];
            pSRVs = AllocArray(Count);
            pRTVs = AllocArray(Count);
            for (int i = 0; i < Count; i++)
            {
                ITexture2D tex = device.CreateTexture2D(formats[i], Width, Height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
                SRVs[i] = device.CreateShaderResourceView(tex);
                RTVs[i] = device.CreateRenderTargetView(tex, new(Width, Height));
                PSRVs[i] = (void*)SRVs[i].NativePointer;
                PRTVs[i] = (void*)RTVs[i].NativePointer;
                tex.Dispose();
            }
        }

        public void Resize(int width, int height)
        {
            for (int i = 0; i < Count; i++)
            {
                RTVs[i].Dispose();
                SRVs[i].Dispose();
            }

            this.width = width;
            this.height = height;
            for (int i = 0; i < Count; i++)
            {
                ITexture2D tex = device.CreateTexture2D(formats[i], Width, Height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
                SRVs[i] = device.CreateShaderResourceView(tex);
                RTVs[i] = device.CreateRenderTargetView(tex, new(Width, Height));
                PSRVs[i] = (void*)SRVs[i].NativePointer;
                PRTVs[i] = (void*)RTVs[i].NativePointer;
                tex.Dispose();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < Count; i++)
                {
                    RTVs[i].Dispose();
                    SRVs[i].Dispose();
                }

                Free(PSRVs);
                Free(PRTVs);

                disposedValue = true;
            }
        }

        ~GBuffer()
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