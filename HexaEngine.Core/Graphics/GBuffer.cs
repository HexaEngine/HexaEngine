namespace HexaEngine.Core.Graphics
{
    using Hexa.NET.Mathematics;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a G-Buffer used for storing intermediate rendering results.
    /// </summary>
    public unsafe class GBuffer : IDisposable
    {
        private readonly string dbgName;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="GBuffer"/> class.
        /// </summary>
        /// <param name="description">The G-Buffer description.</param>
        /// <param name="filename">The file path where the constructor is called (automatically provided by the compiler).</param>
        /// <param name="lineNumber">The line number where the constructor is called (automatically provided by the compiler).</param>
        public GBuffer(GBufferDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"GBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            var device = Application.GraphicsDevice;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="GBuffer"/> class with specified width, height, and formats.
        /// </summary>
        /// <param name="width">The width of the G-Buffer.</param>
        /// <param name="height">The height of the G-Buffer.</param>
        /// <param name="formats">An array of texture formats for the G-Buffer.</param>
        /// <param name="filename">The file path where the constructor is called (automatically provided by the compiler).</param>
        /// <param name="lineNumber">The line number where the constructor is called (automatically provided by the compiler).</param>
        public GBuffer(int width, int height, Format[] formats, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"GBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            var device = Application.GraphicsDevice;
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

        /// <summary>
        /// Gets the array of texture formats used in the G-Buffer.
        /// </summary>
        public Format[] Formats => formats;

        /// <summary>
        /// Gets the width of the G-Buffer.
        /// </summary>
        public int Width => width;

        /// <summary>
        /// Gets the height of the G-Buffer.
        /// </summary>
        public int Height => height;

        /// <summary>
        /// Gets the count of textures in the G-Buffer.
        /// </summary>
        public uint Count => count;

        /// <summary>
        /// Gets an array of shader resource views (SRVs) for the G-Buffer.
        /// </summary>
        public IShaderResourceView[] SRVs => srvs;

        /// <summary>
        /// Gets an array of render target views (RTVs) for the G-Buffer.
        /// </summary>
        public IRenderTargetView[] RTVs => rtvs;

        /// <summary>
        /// Gets a pointer to the shader resource views (SRVs) for the G-Buffer.
        /// </summary>
        public void** PSRVs => pSRVs;

        /// <summary>
        /// Gets a pointer to the render target views (RTVs) for the G-Buffer.
        /// </summary>
        public void** PRTVs => pRTVs;

        /// <summary>
        /// Gets the viewport for the G-Buffer using its width and height.
        /// </summary>
        public Viewport Viewport => new(Width, Height);

        /// <summary>
        /// Gets a descriptive name for debugging purposes.
        /// </summary>
        public string DebugName => dbgName;

        /// <summary>
        /// Resizes the G-Buffer to the specified width and height.
        /// </summary>
        /// <param name="width">The new width of the G-Buffer.</param>
        /// <param name="height">The new height of the G-Buffer.</param>
        public void Resize(int width, int height)
        {
            for (int i = 0; i < Count; i++)
            {
                MemoryManager.Unregister(textures[i]);
                rtvs[i].Dispose();
                srvs[i].Dispose();
            }

            var device = Application.GraphicsDevice;

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

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}