namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;
    using System.Runtime.CompilerServices;
    using static HexaEngine.Core.Utils;

    public unsafe class GBuffer : IDisposable
    {
        private readonly IGraphicsDevice device;
        private bool disposedValue;

        public Format Format { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public uint Count { get; private set; }

        public IShaderResourceView[] SRVs { get; private set; }

        public IRenderTargetView[] RTVs { get; private set; }

        public void** PSRVs { get; private set; }

        public void** PRTVs { get; private set; }

        public Viewport Viewport => new(Width, Height);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GBuffer(IGraphicsDevice device, int width, int height, uint count = 1, Format format = Format.R32G32B32A32Float)
        {
            this.device = device;
            Count = count;
            Format = format;
            Width = width;
            Height = height;
            RTVs = new IRenderTargetView[count];
            SRVs = new IShaderResourceView[count];
            PSRVs = AllocArray(count);
            PRTVs = AllocArray(count);
            for (int i = 0; i < count; i++)
            {
                ITexture2D tex = device.CreateTexture2D(format, Width, Height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
                SRVs[i] = device.CreateShaderResourceView(tex);
                RTVs[i] = device.CreateRenderTargetView(tex, new(Width, Height));
                PSRVs[i] = (void*)SRVs[i].NativePointer;
                PRTVs[i] = (void*)RTVs[i].NativePointer;
                tex.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GBuffer(IGraphicsDevice device, int width, int height, params Format[] formats)
        {
            this.device = device;
            Count = (uint)formats.Length;
            Width = width;
            Height = height;
            RTVs = new IRenderTargetView[formats.Length];
            SRVs = new IShaderResourceView[formats.Length];
            PSRVs = AllocArray(Count);
            PRTVs = AllocArray(Count);
            for (int i = 0; i < Count; i++)
            {
                var format = formats[i];
                ITexture2D tex = device.CreateTexture2D(format, Width, Height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
                SRVs[i] = device.CreateShaderResourceView(tex);
                RTVs[i] = device.CreateRenderTargetView(tex, new(Width, Height));
                PSRVs[i] = (void*)SRVs[i].NativePointer;
                PRTVs[i] = (void*)RTVs[i].NativePointer;
                tex.Dispose();
            }
        }

        public void Resize(int width, int height, uint count = 1, Format format = Format.R32G32B32A32Float)
        {
            for (int i = 0; i < Count; i++)
            {
                RTVs[i].Dispose();
                SRVs[i].Dispose();
            }

            if (Count != count)
            {
                Free(PSRVs);
                Free(PRTVs);
                RTVs = new IRenderTargetView[count];
                SRVs = new IShaderResourceView[count];
                PSRVs = AllocArray(count);
                PRTVs = AllocArray(count);
            }

            Count = count;
            Format = format;
            Width = width;
            Height = height;
            for (int i = 0; i < count; i++)
            {
                ITexture2D tex = device.CreateTexture2D(format, Width, Height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
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