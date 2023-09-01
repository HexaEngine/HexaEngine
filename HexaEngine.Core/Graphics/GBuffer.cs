namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;
    using System.Runtime.CompilerServices;
    using static HexaEngine.Core.Utils;

    public unsafe class GBuffer : IDisposable
    {
        private readonly string dbgName;
        private readonly IGraphicsDevice device;
        private readonly Format[] formats;
        private int width;
        private int height;
        private readonly uint count;
        private readonly ITexture2D[] textures;
        private readonly IShaderResourceView[] srvs;
        private readonly IRenderTargetView[] rtvs;
        private unsafe void** pSRVs;
        private unsafe void** pRTVs;
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

        public string DebugName => dbgName;

        public GBuffer(IGraphicsDevice device, GBufferDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"GBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            this.device = device;
            formats = description.Formats;
            count = (uint)description.Count;
            width = description.Width;
            height = description.Height;
            textures = new ITexture2D[count];
            rtvs = new IRenderTargetView[formats.Length];
            srvs = new IShaderResourceView[formats.Length];
            pSRVs = AllocArray(Count);
            pRTVs = AllocArray(Count);
            for (int i = 0; i < Count; i++)
            {
                textures[i] = device.CreateTexture2D(formats[i], Width, Height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
                var name = textures[i].DebugName = $"GBuffer.{i}: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
                srvs[i] = device.CreateShaderResourceView(textures[i]);
                srvs[i].DebugName = name + ".SRV";
                rtvs[i] = device.CreateRenderTargetView(textures[i]);
                rtvs[i].DebugName = name + ".RTV";
                pSRVs[i] = (void*)srvs[i].NativePointer;
                pRTVs[i] = (void*)rtvs[i].NativePointer;
                MemoryManager.Register(textures[i]);
            }
        }

        public GBuffer(IGraphicsDevice device, int width, int height, Format[] formats, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"GBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            this.device = device;
            this.formats = formats;
            count = (uint)formats.Length;
            this.width = width;
            this.height = height;
            textures = new ITexture2D[count];
            rtvs = new IRenderTargetView[formats.Length];
            srvs = new IShaderResourceView[formats.Length];
            pSRVs = AllocArray(Count);
            pRTVs = AllocArray(Count);
            for (int i = 0; i < Count; i++)
            {
                textures[i] = device.CreateTexture2D(formats[i], Width, Height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
                var name = textures[i].DebugName = $"GBuffer.{i}: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
                srvs[i] = device.CreateShaderResourceView(textures[i]);
                srvs[i].DebugName = name + ".SRV";
                rtvs[i] = device.CreateRenderTargetView(textures[i]);
                rtvs[i].DebugName = name + ".RTV";
                pSRVs[i] = (void*)srvs[i].NativePointer;
                pRTVs[i] = (void*)rtvs[i].NativePointer;
                MemoryManager.Register(textures[i]);
            }
        }

        public void Resize(int width, int height)
        {
            for (int i = 0; i < Count; i++)
            {
                MemoryManager.Unregister(textures[i]);
                rtvs[i].Dispose();
                srvs[i].Dispose();
            }

            this.width = width;
            this.height = height;
            for (int i = 0; i < Count; i++)
            {
                ITexture2D tex = device.CreateTexture2D(formats[i], Width, Height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
                var name = textures[i].DebugName = dbgName.Replace("GBuffer", $"GBuffer.{i}");
                srvs[i] = device.CreateShaderResourceView(tex);
                srvs[i].DebugName = name + ".SRV";
                rtvs[i] = device.CreateRenderTargetView(tex);
                rtvs[i].DebugName = name + ".RTV";
                pSRVs[i] = (void*)srvs[i].NativePointer;
                pRTVs[i] = (void*)rtvs[i].NativePointer;
                MemoryManager.Register(tex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < Count; i++)
                {
                    MemoryManager.Unregister(textures[i]);
                    textures[i].Dispose();
                    rtvs[i].Dispose();
                    srvs[i].Dispose();
                }

                Free(pRTVs);
                Free(pSRVs);
                pRTVs = null;
                pSRVs = null;

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