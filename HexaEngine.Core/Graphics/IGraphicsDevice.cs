namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Windows;
    using HexaEngine.Mathematics;
    using Silk.NET.SDL;
    using System;
    using System.Runtime.CompilerServices;

    public interface IGraphicsDevice : IDeviceChild
    {
        GraphicsBackend Backend { get; }

        /// <summary>
        /// The immediate context of this device
        /// </summary>
        IGraphicsContext Context { get; }

        ITextureLoader TextureLoader { get; }

        IGPUProfiler Profiler { get; }

        ISwapChain CreateSwapChain(SdlWindow window);

        unsafe ISwapChain CreateSwapChain(Window* window);

        ISwapChain CreateSwapChain(SdlWindow window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription);

        unsafe ISwapChain CreateSwapChain(Window* window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription);

        /// <summary>
        /// Creates a <see cref="IBuffer"/> with the given <see cref="BufferDescription"/>
        /// </summary>
        /// <param name="description">The <see cref="BufferDescription"/> that describes the <see cref="IBuffer"/></param>
        /// <returns>The <see cref="IBuffer"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        IBuffer CreateBuffer(BufferDescription description);

        /// <summary>
        /// Creates a <see cref="IBuffer"/> with the given <see cref="BufferDescription"/>
        /// </summary>
        /// <param name="description">The <see cref="BufferDescription"/> that describes the <see cref="IBuffer"/></param>
        /// <returns>The <see cref="IBuffer"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        unsafe IBuffer CreateBuffer(void* src, uint length, BufferDescription description);

        /// <summary>
        /// Creates a <see cref="IBuffer"/> with the given <see cref="BufferDescription"/> and with the initial value <paramref name="value"/><br/>
        /// </summary>
        /// <remarks>If <see cref="BufferDescription.ByteWidth"/> is 0, then <see cref="BufferDescription.ByteWidth"/> will be automatically determent by <typeparamref name="T"/></remarks>
        /// <typeparam name="T">The initial value type, the size must be dividable by 16</typeparam>
        /// <param name="value">The initial value of type <typeparamref name="T"/></param>
        /// <param name="description">The <see cref="BufferDescription"/> that describes the <see cref="IBuffer"/></param>
        /// <returns>The <see cref="IBuffer"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : unmanaged;

        /// <summary>
        /// Creates a <see cref="IBuffer"/> with the given <see cref="BufferDescription"/> and with the initial value <paramref name="value"/><br/>
        /// The <see cref="BufferDescription.ByteWidth"/> will be automatically determent by <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The initial value type, the size must be dividable by 16</typeparam>
        /// <param name="value">The initial value of type <typeparamref name="T"/></param>
        /// <param name="bindFlags">Valid are <see cref="BindFlags.VertexBuffer"/>, <see cref="BindFlags.IndexBuffer"/>, <see cref="BindFlags.ConstantBuffer"/>, <see cref="BindFlags.ShaderResource"/>, <see cref="BindFlags.StreamOutput"/>, <see cref="BindFlags.UnorderedAccess"/></param>
        /// <param name="usage">The <see cref="IBuffer"/> usage</param>
        /// <param name="cpuAccessFlags"></param>
        /// <param name="miscFlags"></param>
        /// <returns></returns>
        IBuffer CreateBuffer<T>(T value, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged;

        unsafe IBuffer CreateBuffer<T>(T* values, uint count, BufferDescription description) where T : unmanaged;

        unsafe IBuffer CreateBuffer<T>(T* values, uint count, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged;

        IDepthStencilView CreateDepthStencilView(IResource resource, DepthStencilViewDescription description);

        IDepthStencilView CreateDepthStencilView(IResource resource);

        IRenderTargetView CreateRenderTargetView(IResource resource, Viewport viewport);

        IRenderTargetView CreateRenderTargetView(IResource resource, RenderTargetViewDescription description, Viewport viewport);

        IShaderResourceView CreateShaderResourceView(IResource resource);

        IShaderResourceView CreateShaderResourceView(IResource texture, ShaderResourceViewDescription description);

        IShaderResourceView CreateShaderResourceView(IBuffer buffer);

        IShaderResourceView CreateShaderResourceView(IBuffer buffer, ShaderResourceViewDescription description);

        ISamplerState CreateSamplerState(SamplerDescription sampler);

        ITexture1D CreateTexture1D(Texture1DDescription description);

        ITexture2D CreateTexture2D(Texture2DDescription description);

        ITexture3D CreateTexture3D(Texture3DDescription description);

        ITexture1D CreateTexture1D(Texture1DDescription description, SubresourceData[]? subresources);

        ITexture2D CreateTexture2D(Texture2DDescription description, SubresourceData[]? subresources);

        ITexture3D CreateTexture3D(Texture3DDescription description, SubresourceData[]? subresources);

        ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None);

        ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, int sampleCount = 1, int sampleQuality = 0, ResourceMiscFlag misc = ResourceMiscFlag.None);

        ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None);

        ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag none);

        ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag none);

        ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag none);

        IQuery CreateQuery();

        IQuery CreateQuery(Query type);

        IGraphicsContext CreateDeferredContext();

        IUnorderedAccessView CreateUnorderedAccessView(IResource resource, UnorderedAccessViewDescription description);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        IComputePipeline CreateComputePipeline(ComputePipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        IComputePipeline CreateComputePipeline(ComputePipelineDesc desc, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0);

        Task<IGraphicsPipeline> CreateGraphicsPipelineAsync(GraphicsPipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return Task.Factory.StartNew(() => CreateGraphicsPipeline(desc, filename, line));
        }

        Task<IGraphicsPipeline> CreateGraphicsPipelineAsync(GraphicsPipelineDesc desc, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return Task.Factory.StartNew(() => CreateGraphicsPipeline(desc, macros, filename, line));
        }

        Task<IGraphicsPipeline> CreateGraphicsPipelineAsync(GraphicsPipelineDesc desc, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return Task.Factory.StartNew(() => CreateGraphicsPipeline(desc, elementDescriptions, filename, line));
        }

        Task<IGraphicsPipeline> CreateGraphicsPipelineAsync(GraphicsPipelineDesc desc, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return Task.Factory.StartNew(() => CreateGraphicsPipeline(desc, inputElements, macros, filename, line));
        }

        Task<IGraphicsPipeline> CreateGraphicsPipelineAsync(GraphicsPipelineDesc desc, GraphicsPipelineState state, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return Task.Factory.StartNew(() => CreateGraphicsPipeline(desc, state, filename, line));
        }

        Task<IGraphicsPipeline> CreateGraphicsPipelineAsync(GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return Task.Factory.StartNew(() => CreateGraphicsPipeline(desc, state, macros, filename, line));
        }

        Task<IGraphicsPipeline> CreateGraphicsPipelineAsync(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return Task.Factory.StartNew(() => CreateGraphicsPipeline(desc, state, elementDescriptions, filename, line));
        }

        Task<IGraphicsPipeline> CreateGraphicsPipelineAsync(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return Task.Factory.StartNew(() => CreateGraphicsPipeline(desc, state, inputElements, macros, filename, line));
        }

        Task<IComputePipeline> CreateComputePipelineAsync(ComputePipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return Task.Factory.StartNew(() => CreateComputePipeline(desc, filename, line));
        }

        Task<IComputePipeline> CreateComputePipelineAsync(ComputePipelineDesc desc, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return Task.Factory.StartNew(() => CreateComputePipeline(desc, macros, filename, line));
        }
    }
}