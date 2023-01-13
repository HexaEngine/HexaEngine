namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Graphics.Reflection;
    using HexaEngine.Mathematics;
    using System;
    using System.Threading.Tasks;

    public interface IGraphicsDevice : IDeviceChild
    {
        /// <summary>
        /// The immediate context of this device
        /// </summary>
        public IGraphicsContext Context { get; }

        /// <summary>
        /// The SwapChain of the associated window
        /// </summary>
        public ISwapChain? SwapChain { get; }

        /// <summary>
        /// Creates a <see cref="IBuffer"/> with the given <see cref="BufferDescription"/>
        /// </summary>
        /// <param name="description">The <see cref="BufferDescription"/> that describes the <see cref="IBuffer"/></param>
        /// <returns>The <see cref="IBuffer"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="OutOfMemoryException"></exception>
        public IBuffer CreateBuffer(BufferDescription description);

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
        public IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : struct;

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
        public IBuffer CreateBuffer<T>(T value, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : struct;

        unsafe IBuffer CreateBuffer<T>(T* values, uint count, BufferDescription description) where T : unmanaged;

        unsafe IBuffer CreateBuffer<T>(T* values, uint count, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged;

        public IDepthStencilView CreateDepthStencilView(IResource resource, DepthStencilViewDescription description);

        public IDepthStencilView CreateDepthStencilView(IResource resource);

        public IRenderTargetView CreateRenderTargetView(IResource resource, Viewport viewport);

        public IRenderTargetView CreateRenderTargetView(IResource resource, RenderTargetViewDescription description, Viewport viewport);

        public IShaderResourceView CreateShaderResourceView(IResource resource);

        public IShaderResourceView CreateShaderResourceView(IResource texture, ShaderResourceViewDescription description);

        public IShaderResourceView CreateShaderResourceView(IBuffer buffer);

        public ISamplerState CreateSamplerState(SamplerDescription sampler);

        public ITexture1D CreateTexture1D(Texture1DDescription description);

        public ITexture2D CreateTexture2D(Texture2DDescription description);

        public ITexture3D CreateTexture3D(Texture3DDescription description);

        public ITexture1D CreateTexture1D(Texture1DDescription description, SubresourceData[]? subresources);

        public ITexture2D CreateTexture2D(Texture2DDescription description, SubresourceData[]? subresources);

        public ITexture3D CreateTexture3D(Texture3DDescription description, SubresourceData[]? subresources);

        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None);

        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, int sampleCount = 1, int sampleQuality = 0, ResourceMiscFlag misc = ResourceMiscFlag.None);

        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None);

        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag none);

        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag none);

        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag none);

        public ITexture1D LoadTexture1D(string path);

        public ITexture2D LoadTexture2D(string path);

        public ITexture3D LoadTexture3D(string path);

        public ITexture2D LoadTextureCube(string path);

        public void SaveTexture1D(ITexture1D texture, string path);

        public void SaveTexture2D(ITexture2D texture, string path);

        public void SaveTexture3D(ITexture3D texture, string path);

        public void SaveTextureCube(ITexture2D texture, string path);

        public void SaveTexture1D(ITexture1D texture, Format format, string path);

        public void SaveTexture2D(ITexture2D texture, Format format, string path);

        public void SaveTexture3D(ITexture3D texture, Format format, string path);

        public void SaveTextureCube(ITexture2D texture, Format format, string path);

        IQuery CreateQuery();

        IQuery CreateQuery(Query type);

        IGraphicsContext CreateDeferredContext();

        ITexture1D LoadTexture1D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc);

        ITexture2D LoadTexture2D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc);

        ITexture3D LoadTexture3D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc);

        IUnorderedAccessView CreateUnorderedAccessView(IResource resource, UnorderedAccessViewDescription description);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, ShaderMacro[] macros);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] elementDescriptions);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] inputElements, ShaderMacro[] macros);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] elementDescriptions);

        IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements, ShaderMacro[] macros);

        IComputePipeline CreateComputePipeline(ComputePipelineDesc desc);
    }
}