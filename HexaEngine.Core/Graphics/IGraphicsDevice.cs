namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;

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
        /// Creates a <see cref="IBlendState"/> with the given <see cref="BlendDescription"/>
        /// </summary>
        /// <param name="blendDescription">The <see cref="BlendDescription"/> that describes the <see cref="IBlendState"/></param>
        /// <returns>The created <see cref="IBlendState"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public IBlendState CreateBlendState(BlendDescription blendDescription);

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

        public IBuffer CreateBuffer<T>(T[] values, BufferDescription description) where T : struct;

        public IBuffer CreateBuffer<T>(T[] values, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : struct;

        public IDepthStencilState CreateDepthStencilState(DepthStencilDescription description);

        public IDepthStencilView CreateDepthStencilView(IResource resource, DepthStencilViewDescription description);

        public IDepthStencilView CreateDepthStencilView(IResource resource);

        public IRasterizerState CreateRasterizerState(RasterizerDescription description);

        public IRenderTargetView CreateRenderTargetView(IResource resource, Viewport viewport);

        public IRenderTargetView CreateRenderTargetView(IResource resource, RenderTargetViewDescription description, Viewport viewport);

        public IShaderResourceView CreateShaderResourceView(IResource resource);

        public IShaderResourceView CreateShaderResourceView(IResource texture, ShaderResourceViewDescription description);

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

        public IVertexShader CreateVertexShader(byte[] bytecode);

        public IHullShader CreateHullShader(byte[] bytecode);

        public IDomainShader CreateDomainShader(byte[] bytecode);

        public IGeometryShader CreateGeometryShader(byte[] bytecode);

        public IPixelShader CreatePixelShader(byte[] bytecode);

        public IComputeShader CreateComputeShader(byte[] bytecode);

        public IVertexShader CreateVertexShader(Span<byte> bytecode) => CreateVertexShader(bytecode.ToArray());

        public IHullShader CreateHullShader(Span<byte> bytecode) => CreateHullShader(bytecode.ToArray());

        public IDomainShader CreateDomainShader(Span<byte> bytecode) => CreateDomainShader(bytecode.ToArray());

        public IGeometryShader CreateGeometryShader(Span<byte> bytecode) => CreateGeometryShader(bytecode.ToArray());

        public IPixelShader CreatePixelShader(Span<byte> bytecode) => CreatePixelShader(bytecode.ToArray());

        public IComputeShader CreateComputeShader(Span<byte> bytecode) => CreateComputeShader(bytecode.ToArray());

        public IVertexShader CreateVertexShader(Blob bytecode) => CreateVertexShader(bytecode.AsBytes());

        public IHullShader CreateHullShader(Blob bytecode) => CreateHullShader(bytecode.AsBytes());

        public IDomainShader CreateDomainShader(Blob bytecode) => CreateDomainShader(bytecode.AsBytes());

        public IGeometryShader CreateGeometryShader(Blob bytecode) => CreateGeometryShader(bytecode.AsBytes());

        public IPixelShader CreatePixelShader(Blob bytecode) => CreatePixelShader(bytecode.AsBytes());

        public IComputeShader CreateComputeShader(Blob bytecode) => CreateComputeShader(bytecode.AsBytes());

        public IInputLayout CreateInputLayout(InputElementDescription[] inputElements, Blob vertexShaderBlob);

        public IInputLayout CreateInputLayout(byte[] data);

        public IInputLayout CreateInputLayout(Span<byte> data) => CreateInputLayout(data.ToArray());

        public IInputLayout CreateInputLayout(Blob vBlob) => CreateInputLayout(vBlob.AsBytes());

        public void Compile(string code, string entry, string sourceName, string profile, out Blob? shaderBlob, out Blob? errorBlob);

        public void Compile(string code, string entry, string sourceName, string profile, out Blob? shaderBlob);

        public void CompileFromFile(string path, string entry, string profile, out Blob? shaderBlob, out Blob? errorBlob);

        public void CompileFromFile(string path, string entry, string profile, out Blob? shaderBlob);

        public void Compile(string code, ShaderMacro[] macros, string entry, string sourceName, string profile, out Blob? shaderBlob, out Blob? errorBlob);

        public void Compile(string code, ShaderMacro[] macros, string entry, string sourceName, string profile, out Blob? shaderBlob);

        public void CompileFromFile(string path, ShaderMacro[] macros, string entry, string profile, out Blob? shaderBlob, out Blob? errorBlob);

        public void CompileFromFile(string path, ShaderMacro[] macros, string entry, string profile, out Blob? shaderBlob);

        IQuery CreateQuery();
        IQuery CreateQuery(Query type);
        IInputLayout CreateInputLayout(InputElementDescription[] inputElements, byte[] data);
    }
}