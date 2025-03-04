namespace HexaEngine.Core.Graphics
{
    using Hexa.NET.SDL3;
    using HexaEngine.Core.Windows;
    using System.Runtime.CompilerServices;

    public struct GraphicsDeviceCapabilities
    {
        public GraphicsDeviceCapabilitiesFlags Flags;

        public readonly bool SupportsCommandLists => (Flags & GraphicsDeviceCapabilitiesFlags.SupportsCommandLists) != 0;

        public readonly bool SupportsComputeShaders => (Flags & GraphicsDeviceCapabilitiesFlags.SupportsComputeShaders) != 0;

        public readonly bool SupportsGeometryShaders => (Flags & GraphicsDeviceCapabilitiesFlags.SupportsGeometryShaders) != 0;

        public readonly bool SupportsRayTracing => (Flags & GraphicsDeviceCapabilitiesFlags.SupportsRayTracing) != 0;

        public readonly bool SupportsTessellationShaders => (Flags & GraphicsDeviceCapabilitiesFlags.SupportsTessellationShaders) != 0;
    }

    public enum GraphicsDeviceCapabilitiesFlags
    {
        None = 0,

        /// <summary>
        /// Indicates support for query objects.
        /// </summary>
        SupportsQuery = 1 << 0,

        /// <summary>
        /// Indicates support for geometry shaders.
        /// </summary>
        SupportsGeometryShaders = 1 << 1,

        /// <summary>
        /// Indicates support for compute shaders.
        /// </summary>
        SupportsComputeShaders = 1 << 2,

        /// <summary>
        /// Indicates support for tessellation shaders.
        /// </summary>
        SupportsTessellationShaders = 1 << 3,

        /// <summary>
        /// Indicates support for command lists, enabling pre-recorded command execution.
        /// </summary>
        SupportsCommandLists = 1 << 4,

        SupportsConservativeRasterization = 1 << 5,

        SupportsIndirectDraw = 1 << 6,

        /// <summary>
        /// Indicates support for ray tracing.
        /// </summary>
        SupportsRayTracing = 1 << 7,

        Minimal = SupportsQuery | SupportsGeometryShaders | SupportsComputeShaders | SupportsTessellationShaders,
        Full = SupportsGeometryShaders | SupportsComputeShaders | SupportsTessellationShaders | SupportsCommandLists | SupportsRayTracing,
    }

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
        /// Gets the capabilities of the graphics device.
        /// </summary>
        GraphicsDeviceCapabilities Capabilities { get; }

        /// <summary>
        /// Creates a swap chain associated with a SDL window for rendering.
        /// </summary>
        /// <param name="window">The SDL window to associate the swap chain with.</param>
        /// <returns>The created swap chain.</returns>
        ISwapChain CreateSwapChain(SdlWindow window);

        IResourceBindingList CreateResourceBindingList(IGraphicsPipeline pipeline);

        IResourceBindingList CreateResourceBindingList(IComputePipeline pipeline);

        /// <summary>
        /// Creates a swap chain associated with a SDL window for rendering.
        /// </summary>
        /// <param name="window">The SDL window to associate the swap chain with.</param>
        /// <returns>The created swap chain.</returns>
        unsafe ISwapChain CreateSwapChain(SDLWindow* window);

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
        unsafe ISwapChain CreateSwapChain(SDLWindow* window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription);

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
        /// Creates a buffer with the given description and initial values.
        /// </summary>
        /// <param name="values">The array of initial values.</param>
        /// <param name="stride">The stride of the buffer.</param>
        /// <param name="count">The number of values in the array.</param>
        /// <param name="description">The description of the buffer.</param>
        /// <returns>The created buffer.</returns>
        unsafe IBuffer CreateBuffer(void* values, int stride, uint count, BufferDescription description);

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
        [Obsolete("Use command buffers")]
        IGraphicsContext CreateDeferredContext();

        ICommandBuffer CreateCommandBuffer();

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
        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDescEx desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        /// <summary>
        /// Creates a graphics pipeline state object.
        /// </summary>
        /// <param name="pipeline">The graphics pipeline containing shaders.</param>
        /// <param name="desc">Description of the graphics pipeline state.</param>
        /// <param name="filename">The file path of the caller (automatically provided).</param>
        /// <param name="line">The line number of the caller (automatically provided).</param>
        /// <returns>The created graphics pipeline state object.</returns>
        IGraphicsPipelineState CreateGraphicsPipelineState(IGraphicsPipeline pipeline, GraphicsPipelineStateDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        /// <summary>
        /// Creates a graphics pipeline state based on the provided graphics pipeline description and state description.
        /// </summary>
        /// <param name="pipelineDesc">The description of the graphics pipeline.</param>
        /// <param name="desc">The description of the graphics pipeline state.</param>
        /// <param name="filename">The file path of the caller (automatically provided by the compiler).</param>
        /// <param name="line">The line number of the caller (automatically provided by the compiler).</param>
        /// <returns>The created graphics pipeline state.</returns>
        IGraphicsPipelineState CreateGraphicsPipelineState(GraphicsPipelineDescEx pipelineDesc, GraphicsPipelineStateDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = CreateGraphicsPipeline(pipelineDesc, filename, line);
            var pso = CreateGraphicsPipelineState(pipeline, desc, filename, line);
            pipeline.Dispose();
            return pso;
        }

        /// <summary>
        /// Creates a graphics pipeline state based on the provided combined description.
        /// </summary>
        /// <param name="desc">The combined description of the graphics pipeline and its state.</param>
        /// <param name="filename">The file path of the caller.</param>
        /// <param name="line">The line number of the caller.</param>
        /// <returns>The created graphics pipeline state.</returns>
        IGraphicsPipelineState CreateGraphicsPipelineState(GraphicsPipelineStateDescEx desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return CreateGraphicsPipelineState(desc.Pipeline, desc.State, filename, line);
        }

        /// <summary>
        /// Creates a graphics pipeline state object.
        /// </summary>
        /// <param name="pipeline">The graphics pipeline containing shaders.</param>
        /// <param name="filename">The file path of the caller (automatically provided).</param>
        /// <param name="line">The line number of the caller (automatically provided).</param>
        /// <returns>The created graphics pipeline state object.</returns>
        IComputePipelineState CreateComputePipelineState(IComputePipeline pipeline, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        /// <summary>
        /// Creates a graphics pipeline state object.
        /// </summary>
        /// <param name="desc">The description of the compute pipeline.</param>
        /// <param name="filename">The file path of the caller (automatically provided).</param>
        /// <param name="line">The line number of the caller (automatically provided).</param>
        /// <returns>The created graphics pipeline state object.</returns>
        IComputePipelineState CreateComputePipelineState(ComputePipelineDescEx desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = CreateComputePipeline(desc, filename, line);
            var pso = CreateComputePipelineState(pipeline, filename, line);
            pipeline.Dispose();
            return pso;
        }

        /// <summary>
        /// Creates a compute pipeline with the specified description.
        /// </summary>
        /// <param name="desc">The description of the compute pipeline.</param>
        /// <param name="filename">The file path of the caller (automatically filled by the compiler).</param>
        /// <param name="line">The line number of the caller (automatically filled by the compiler).</param>
        /// <returns>The created compute pipeline.</returns>
        IComputePipeline CreateComputePipeline(ComputePipelineDescEx desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        /// <summary>
        /// Creates a fence object.
        /// </summary>
        /// <param name="initialValue">The initial value of the fence.</param>
        /// <param name="flags">Flags to control fence behavior.</param>
        /// <returns>The created fence object.</returns>
        IFence CreateFence(ulong initialValue, FenceFlags flags);

        void SetGlobalSRV(string name, IShaderResourceView? srv);

        void SetGlobalCBV(string name, IBuffer? cbv);

        void SetGlobalSampler(string name, ISamplerState? sampler);

        void SetGlobalUAV(string name, IUnorderedAccessView? uav, uint initialCount = unchecked((uint)-1));
    }
}