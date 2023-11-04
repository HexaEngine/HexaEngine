namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a chain of depth textures with multiple mip levels for use in graphics rendering.
    /// </summary>
    public unsafe class DepthMipChain : IDisposable
    {
        private readonly string dbgName;
        private int height;
        private int width;
        private int mipLevels;

        private readonly Texture2D texture;

        private IRenderTargetView rtv;
        private IShaderResourceView srv;

        private IShaderResourceView[] srvs;
        private IUnorderedAccessView[] uavs;

        private Viewport[] viewports;

        private void** pUavs;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthMipChain"/> class.
        /// </summary>
        /// <param name="device">The graphics device for rendering.</param>
        /// <param name="desc">A description of the depth-stencil buffer.</param>
        /// <param name="filename">The name of the source file (automatically generated).</param>
        /// <param name="lineNumber">The line number in the source file (automatically generated).</param>
        public DepthMipChain(IGraphicsDevice device, DepthStencilBufferDescription desc, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"DepthMipChain: {Path.GetFileName(filename)}, Line: {lineNumber}";
            width = desc.Width;
            height = desc.Height;

            mipLevels = TextureHelper.ComputeMipLevels(desc.Width, desc.Height);

            texture = new(device, Format.R32Float, desc.Width, desc.Height, 1, mipLevels, CpuAccessFlags.None, GpuAccessFlags.All);
            texture.DebugName = dbgName;
            srv = device.CreateShaderResourceView(texture);
            srv.DebugName = dbgName + ".SRV";
            rtv = device.CreateRenderTargetView(texture);
            rtv.DebugName = dbgName + ".RTV";

            srvs = new IShaderResourceView[mipLevels];
            uavs = new IUnorderedAccessView[mipLevels];
            pUavs = AllocArray((uint)mipLevels);
            viewports = new Viewport[mipLevels];
            int mipWidth = desc.Width;
            int mipHeight = desc.Height;

            for (int i = 0; i < mipLevels; i++)
            {
                srvs[i] = device.CreateShaderResourceView(texture, new(ShaderResourceViewDimension.Texture2D, Format.R32Float, i, 1, 0, 1));
                uavs[i] = device.CreateUnorderedAccessView(texture, new(UnorderedAccessViewDimension.Texture2D, Format.R32Float, i, 0, 1, BufferUnorderedAccessViewFlags.None));
                pUavs[i] = (void*)uavs[i].NativePointer;
                viewports[i] = new(mipWidth, mipHeight);
                mipWidth /= 2;
                mipHeight /= 2;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthMipChain"/> class with a specified width and height.
        /// </summary>
        /// <param name="device">The graphics device for rendering.</param>
        /// <param name="width">The width of the depth texture.</param>
        /// <param name="height">The height of the depth texture.</param>
        /// <param name="filename">The name of the source file (automatically generated).</param>
        /// <param name="lineNumber">The line number in the source file (automatically generated).</param>
        public DepthMipChain(IGraphicsDevice device, int width, int height, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"DepthMipChain: {Path.GetFileName(filename)}, Line: {lineNumber}";
            this.width = width;
            this.height = height;

            mipLevels = TextureHelper.ComputeMipLevels(width, height);

            texture = new(device, Format.R32Float, width, height, 1, mipLevels, CpuAccessFlags.None, GpuAccessFlags.All);
            texture.DebugName = dbgName;
            srv = device.CreateShaderResourceView(texture);
            srv.DebugName = dbgName + ".SRV";
            rtv = device.CreateRenderTargetView(texture);
            rtv.DebugName = dbgName + ".RTV";

            srvs = new IShaderResourceView[mipLevels];
            uavs = new IUnorderedAccessView[mipLevels];
            pUavs = AllocArray((uint)mipLevels);
            viewports = new Viewport[mipLevels];
            int mipWidth = width;
            int mipHeight = height;

            for (int i = 0; i < mipLevels; i++)
            {
                srvs[i] = device.CreateShaderResourceView(texture, new(ShaderResourceViewDimension.Texture2D, Format.R32Float, i, 1, 0, 1));
                uavs[i] = device.CreateUnorderedAccessView(texture, new(UnorderedAccessViewDimension.Texture2D, Format.R32Float, i, 0, 1, BufferUnorderedAccessViewFlags.None));
                pUavs[i] = (void*)uavs[i].NativePointer;
                viewports[i] = new(mipWidth, mipHeight);
                mipWidth /= 2;
                mipHeight /= 2;
            }
        }

        /// <summary>
        /// Gets the array of viewports for different mip levels.
        /// </summary>
        public Viewport[] Viewports => viewports;

        /// <summary>
        /// Gets the render target view of the depth mip chain.
        /// </summary>
        public IRenderTargetView RTV => rtv;

        /// <summary>
        /// Gets the shader resource view of the depth mip chain.
        /// </summary>
        public IShaderResourceView SRV => srv;

        /// <summary>
        /// Gets an array of unordered access views for different mip levels.
        /// </summary>
        public IUnorderedAccessView[] UAVs => uavs;

        /// <summary>
        /// Gets an array of shader resource views for different mip levels.
        /// </summary>
        public IShaderResourceView[] SRVs => srvs;

        /// <summary>
        /// Gets the width of the depth mip chain.
        /// </summary>
        public int Width => width;

        /// <summary>
        /// Gets the height of the depth mip chain.
        /// </summary>
        public int Height => height;

        /// <summary>
        /// Gets the number of mip levels in the depth mip chain.
        /// </summary>
        public int MipLevels => mipLevels;

        /// <summary>
        /// Resizes the depth mip chain to the specified width and height.
        /// </summary>
        /// <param name="device">The graphics device for rendering.</param>
        /// <param name="width">The new width for the depth mip chain.</param>
        /// <param name="height">The new height for the depth mip chain.</param>
        public void Resize(IGraphicsDevice device, int width, int height)
        {
            this.width = width;
            this.height = height;
            for (int i = 0; i < mipLevels; i++)
            {
                srvs[i].Dispose();
                uavs[i].Dispose();
            }

            Free(pUavs);

            rtv.Dispose();
            srv.Dispose();
            texture.Dispose();

            mipLevels = TextureHelper.ComputeMipLevels(width, height);

            texture.Resize(device, Format.R32Float, width, height, 1, mipLevels, CpuAccessFlags.None, GpuAccessFlags.All);
            srv = device.CreateShaderResourceView(texture);
            rtv = device.CreateRenderTargetView(texture);

            srvs = new IShaderResourceView[mipLevels];
            uavs = new IUnorderedAccessView[mipLevels];
            pUavs = AllocArray((uint)mipLevels);
            viewports = new Viewport[mipLevels];
            int mipWidth = width;
            int mipHeight = height;

            for (int i = 0; i < mipLevels; i++)
            {
                srvs[i] = device.CreateShaderResourceView(texture, new(ShaderResourceViewDimension.Texture2D, Format.R32Float, i, 1, 0, 1));
                uavs[i] = device.CreateUnorderedAccessView(texture, new(UnorderedAccessViewDimension.Texture2D, Format.R32Float, i, 0, 1, BufferUnorderedAccessViewFlags.None));
                pUavs[i] = (void*)uavs[i].NativePointer;
                viewports[i] = new(mipWidth, mipHeight);
                mipWidth /= 2;
                mipHeight /= 2;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            for (int i = 0; i < mipLevels; i++)
            {
                srvs[i].Dispose();
                uavs[i].Dispose();
            }

            Free(pUavs);

            rtv.Dispose();
            srv.Dispose();
            texture.Dispose();
        }
    }
}