namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Windows;
    using Silk.NET.SDL;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a graphics device used for rendering.
    /// </summary>
    public interface IGraphicsDevice : IDeviceChild
    {
        /// <summary>
        /// Gets the graphics backend used by the device.
        /// </summary>
        GraphicsBackend Backend { get; }

        /// <summary>
        /// Gets the immediate graphics context associated with the device.
        /// </summary>
        IGraphicsContext Context { get; }

        /// <summary>
        /// Gets the texture loader associated with the device.
        /// </summary>
        ITextureLoader TextureLoader { get; }

        /// <summary>
        /// Gets the GPU profiler associated with the device.
        /// </summary>
        IGPUProfiler Profiler { get; }

        /// <summary>
        /// Creates a swap chain associated with a SDL window for rendering.
        /// </summary>
        /// <param name="window">The SDL window to associate the swap chain with.</param>
        /// <returns>The created swap chain.</returns>
        ISwapChain CreateSwapChain(SdlWindow window);

        /// <summary>
        /// Creates a swap chain associated with a SDL window for rendering.
        /// </summary>
        /// <param name="window">The SDL window to associate the swap chain with.</param>
        /// <returns>The created swap chain.</returns>
        unsafe ISwapChain CreateSwapChain(Window* window);

        /// <summary>
        /// Creates a swap chain associated with a SDL window for rendering.
        /// </summary>
        /// <param name="window">The SDL window to associate the swap chain with.</param>
        /// <param name="swapChainDescription">The description of the swap chain.</param>
        /// <param name="fullscreenDescription">The description of the fullscreen mode.</param>
        /// <returns>The created swap chain.</returns>
        ISwapChain CreateSwapChain(SdlWindow window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription);

        /// <summary>
        /// Creates a swap chain associated with a SDL window for rendering.
        /// </summary>
        /// <param name="window">The SDL window to associate the swap chain with.</param>
        /// <param name="swapChainDescription">The description of the swap chain.</param>
        /// <param name="fullscreenDescription">The description of the fullscreen mode.</param>
        /// <returns>The created swap chain.</returns>
        unsafe ISwapChain CreateSwapChain(Window* window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription);

        /// <summary>
        /// Creates a buffer with the given description.
        /// </summary>
        /// <param name="description">The description of the buffer.</param>
        /// <returns>The created buffer.</returns>
        IBuffer CreateBuffer(BufferDescription description);

        /// <summary>
        /// Creates a buffer with the given description and initial value.
        /// </summary>
        /// <typeparam name="T">The type of the initial value.</typeparam>
        /// <param name="value">The initial value of the buffer.</param>
        /// <param name="description">The description of the buffer.</param>
        /// <returns>The created buffer.</returns>
        IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : unmanaged;

        /// <summary>
        /// Creates a buffer with the given description and initial values.
        /// </summary>
        /// <typeparam name="T">The type of the initial values.</typeparam>
        /// <param name="values">The array of initial values.</param>
        /// <param name="count">The number of values in the array.</param>
        /// <param name="description">The description of the buffer.</param>
        /// <returns>The created buffer.</returns>
        unsafe IBuffer CreateBuffer<T>(T* values, uint count, BufferDescription description) where T : unmanaged;

        /// <summary>
        /// Creates a depth-stencil view for a resource.
        /// </summary>
        /// <param name="resource">The resource to create the view for.</param>
        /// <param name="description">The description of the depth-stencil view.</param>
        /// <returns>The created depth-stencil view.</returns>
        IDepthStencilView CreateDepthStencilView(IResource resource, DepthStencilViewDescription description);

        /// <summary>
        /// Creates a depth-stencil view for a resource.
        /// </summary>
        /// <param name="resource">The resource to create the view for.</param>
        /// <returns>The created depth-stencil view.</returns>
        IDepthStencilView CreateDepthStencilView(IResource resource);

        /// <summary>
        /// Creates a render target view for a resource.
        /// </summary>
        /// <param name="resource">The resource to create the view for.</param>
        ///
        /// <returns>The created render target view.</returns>
        IRenderTargetView CreateRenderTargetView(IResource resource);

        /// <summary>
        /// Creates a render target view for a resource.
        /// </summary>
        /// <param name="resource">The resource to create the view for.</param>
        /// <param name="description">The description of the render target view.</param>
        ///
        /// <returns>The created render target view.</returns>
        IRenderTargetView CreateRenderTargetView(IResource resource, RenderTargetViewDescription description);

        /// <summary>
        /// Creates a shader resource view for a resource.
        /// </summary>
        /// <param name="resource">The resource to create the view for.</param>
        /// <returns>The created shader resource view.</returns>
        IShaderResourceView CreateShaderResourceView(IResource resource);

        /// <summary>
        /// Creates a shader resource view for a resource with the given description.
        /// </summary>
        /// <param name="texture">The resource to create the view for.</param>
        /// <param name="description">The description of the shader resource view.</param>
        /// <returns>The created shader resource view.</returns>
        IShaderResourceView CreateShaderResourceView(IResource texture, ShaderResourceViewDescription description);

        /// <summary>
        /// Creates a shader resource view for a buffer.
        /// </summary>
        /// <param name="buffer">The buffer to create the view for.</param>
        /// <returns>The created shader resource view.</returns>
        IShaderResourceView CreateShaderResourceView(IBuffer buffer);

        /// <summary>
        /// Creates a shader resource view for a buffer with the given description.
        /// </summary>
        /// <param name="buffer">The buffer to create the view for.</param>
        /// <param name="description">The description of the shader resource view.</param>
        /// <returns>The created shader resource view.</returns>
        IShaderResourceView CreateShaderResourceView(IBuffer buffer, ShaderResourceViewDescription description);

        /// <summary>
        /// Creates a sampler state for texture sampling.
        /// </summary>
        /// <param name="sampler">The description of the sampler state.</param>
        /// <returns>The created sampler state.</returns>
        ISamplerState CreateSamplerState(SamplerStateDescription sampler);

        /// <summary>
        /// Creates a 1D texture with the given description.
        /// </summary>
        /// <param name="description">The description of the texture.</param>
        /// <returns>The created 1D texture.</returns>
        ITexture1D CreateTexture1D(Texture1DDescription description);

        /// <summary>
        /// Creates a 2D texture with the given description.
        /// </summary>
        /// <param name="description">The description of the texture.</param>
        /// <returns>The created 2D texture.</returns>
        ITexture2D CreateTexture2D(Texture2DDescription description);

        /// <summary>
        /// Creates a 3D texture with the given description.
        /// </summary>
        /// <param name="description">The description of the texture.</param>
        /// <returns>The created 3D texture.</returns>
        ITexture3D CreateTexture3D(Texture3DDescription description);

        /// <summary>
        /// Creates a 1D texture with the given description and initial subresources.
        /// </summary>
        /// <param name="description">The description of the texture.</param>
        /// <param name="subresources">The initial subresources for the texture.</param>
        /// <returns>The created 1D texture.</returns>
        ITexture1D CreateTexture1D(Texture1DDescription description, SubresourceData[]? subresources);

        /// <summary>
        /// Creates a 2D texture with the given description and initial subresources.
        /// </summary>
        /// <param name="description">The description of the texture.</param>
        /// <param name="subresources">The initial subresources for the texture.</param>
        /// <returns>The created 2D texture.</returns>
        ITexture2D CreateTexture2D(Texture2DDescription description, SubresourceData[]? subresources);

        /// <summary>
        /// Creates a 3D texture with the given description and initial subresources.
        /// </summary>
        /// <param name="description">The description of the texture.</param>
        /// <param name="subresources">The initial subresources for the texture.</param>
        /// <returns>The created 3D texture.</returns>
        ITexture3D CreateTexture3D(Texture3DDescription description, SubresourceData[]? subresources);

        /// <summary>
        /// Creates a 1D texture with the given parameters.
        /// </summary>
        /// <param name="format">The format of the texture.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="arraySize">The number of textures in the array.</param>
        /// <param name="mipLevels">The number of mip levels.</param>
        /// <param name="subresources">The initial subresources for the texture.</param>
        /// <param name="bindFlags">The bind flags for the texture.</param>
        /// <param name="usage">The usage flags for the texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the texture.</param>
        /// <param name="misc">The miscellaneous flags for the texture.</param>
        /// <returns>The created 1D texture.</returns>
        ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None);

        /// <summary>
        /// Creates a 2D texture with the given parameters.
        /// </summary>
        /// <param name="format">The format of the texture.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="arraySize">The number of textures in the array.</param>
        /// <param name="mipLevels">The number of mip levels.</param>
        /// <param name="subresources">The initial subresources for the texture.</param>
        /// <param name="bindFlags">The bind flags for the texture.</param>
        /// <param name="usage">The usage flags for the texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the texture.</param>
        /// <param name="sampleCount">The number of samples per pixel for multisampled textures.</param>
        /// <param name="sampleQuality">The quality level for multisampled textures.</param>
        /// <param name="misc">The miscellaneous flags for the texture.</param>
        /// <returns>The created 2D texture.</returns>
        ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, int sampleCount = 1, int sampleQuality = 0, ResourceMiscFlag misc = ResourceMiscFlag.None);

        /// <summary>
        /// Creates a 3D texture with the given parameters.
        /// </summary>
        /// <param name="format">The format of the texture.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="depth">The depth of the texture.</param>
        /// <param name="mipLevels">The number of mip levels.</param>
        /// <param name="subresources">The initial subresources for the texture.</param>
        /// <param name="bindFlags">The bind flags for the texture.</param>
        /// <param name="usage">The usage flags for the texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the texture.</param>
        /// <param name="misc">The miscellaneous flags for the texture.</param>
        /// <returns>The created 3D texture.</returns>
        ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None);

        /// <summary>
        /// Creates a query object for GPU performance queries.
        /// </summary>
        /// <returns>The created query object.</returns>
        IQuery CreateQuery();

        /// <summary>
        /// Creates a query object of the specified type for GPU performance queries.
        /// </summary>
        /// <param name="type">The type of the query object.</param>
        /// <returns>The created query object.</returns>
        IQuery CreateQuery(Query type);

        /// <summary>
        /// Creates a deferred graphics context for command recording.
        /// </summary>
        /// <returns>The created deferred graphics context.</returns>
        IGraphicsContext CreateDeferredContext();

        /// <summary>
        /// Creates an unordered access view for a resource.
        /// </summary>
        /// <param name="resource">The resource to create the view for.</param>
        /// <param name="description">The description of the unordered access view.</param>
        /// <returns>The created unordered access view.</returns>
        IUnorderedAccessView CreateUnorderedAccessView(IResource resource, UnorderedAccessViewDescription description);

        /// <summary>
        /// Creates a graphics pipeline with the specified description.
        /// </summary>
        /// <param name="desc">The description of the graphics pipeline.</param>
        /// <param name="filename">The file path of the caller (automatically filled by the compiler).</param>
        /// <param name="line">The line number of the caller (automatically filled by the compiler).</param>
        /// <returns>The created graphics pipeline.</returns>
        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        /// <summary>
        /// Creates a compute pipeline with the specified description.
        /// </summary>
        /// <param name="desc">The description of the compute pipeline.</param>
        /// <param name="filename">The file path of the caller (automatically filled by the compiler).</param>
        /// <param name="line">The line number of the caller (automatically filled by the compiler).</param>
        /// <returns>The created compute pipeline.</returns>
        IComputePipeline CreateComputePipeline(ComputePipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        /// <summary>
        /// Asynchronously creates a graphics pipeline with the specified description.
        /// </summary>
        /// <param name="desc">The description of the graphics pipeline.</param>
        /// <param name="filename">The file path of the caller (automatically filled by the compiler).</param>
        /// <param name="line">The line number of the caller (automatically filled by the compiler).</param>
        /// <returns>A task that represents the asynchronous creation of the graphics pipeline.</returns>
        Task<IGraphicsPipeline> CreateGraphicsPipelineAsync(GraphicsPipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return Task.Factory.StartNew(() => CreateGraphicsPipeline(desc, filename, line));
        }

        /// <summary>
        /// Asynchronously creates a compute pipeline with the specified description.
        /// </summary>
        /// <param name="desc">The description of the compute pipeline.</param>
        /// <param name="filename">The file path of the caller (automatically filled by the compiler).</param>
        /// <param name="line">The line number of the caller (automatically filled by the compiler).</param>
        /// <returns>A task that represents the asynchronous creation of the compute pipeline.</returns>

        Task<IComputePipeline> CreateComputePipelineAsync(ComputePipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return Task.Factory.StartNew(() => CreateComputePipeline(desc, filename, line));
        }

        IFence CreateFence(ulong initialValue, FenceFlags flags);
        ICombinedTex2D CreateTex2D(CombinedTex2DDesc desc);
    }
}