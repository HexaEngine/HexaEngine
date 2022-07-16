namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public interface IGraphicsContext : IDeviceChild
    {
        public IGraphicsDevice Device { get; }

        public void Write<T>(IBuffer buffer, T value) where T : struct;

        public void Write<T>(IBuffer buffer, T[] values) where T : struct;

        public void Read<T>(IBuffer buffer, T value) where T : struct;

        public void Read<T>(IBuffer buffer, T[] values) where T : struct;

        public MappedSubresource Map(IResource resource, int subresourceIndex, MapMode mode, MapFlags flags);

        public void Unmap(IResource resource, int subresourceIndex);

        public void SetVertexBuffer(IBuffer? vertexBuffer, int stride);

        public void SetVertexBuffer(IBuffer? vertexBuffer, int stride, int offset);

        public void SetVertexBuffer(int slot, IBuffer? vertexBuffer, int stride);

        public void SetVertexBuffer(int slot, IBuffer? vertexBuffer, int stride, int offset);

        public void SetIndexBuffer(IBuffer? indexBuffer, Format format, int offset);

        public void SetConstantBuffer(IBuffer? constantBuffer, ShaderStage stage, int slot);

        public void SetConstantBuffers(Unsafes.Vector<IBuffer> constantBuffers, ShaderStage stage, int slot);

        public void SetShaderResource(IShaderResourceView? shaderResourceView, ShaderStage stage, int slot);

        public void SetShaderResources(Unsafes.Vector<IShaderResourceView> shaderResourceViews, ShaderStage stage, int slot);

        public void ClearRenderTargetView(IRenderTargetView renderTargetView, Vector4 value);

        public void ClearDepthStencilView(IDepthStencilView depthStencilView, DepthStencilClearFlags flags, float depth, byte stencil);

        public void SetRenderTarget(IRenderTargetView? renderTargetView, IDepthStencilView? depthStencilView);

        public void SetRenderTargets(IRenderTargetView[]? views, IDepthStencilView? depthStencilView);

        public void SetRenderTargets(Unsafes.Vector<IRenderTargetView> views, IDepthStencilView? depthStencilView);

        void SetScissorRect(int x, int y, int z, int w);

        void ClearState();

        void SetViewport(Viewport viewport);

        void SetBlendState(IBlendState? blendState, Vector4? factor = null, uint sampleMask = uint.MaxValue);

        void SetDepthStencilState(IDepthStencilState? depthStencilState, int stencilRef = 0);

        void SetRasterizerState(IRasterizerState? rasterizerState);

        void SetSampler(ISamplerState? sampler, ShaderStage stage, int slot);

        void SetSamplers(Unsafes.Vector<ISamplerState> samplers, ShaderStage stage, int slot);

        void SetPrimitiveTopology(PrimitiveTopology topology);

        void VSSetShader(IVertexShader? vertexShader);

        void PSSetShader(IPixelShader? pixelShader);

        void GSSetShader(IGeometryShader? geometryShader);

        void HSSetShader(IHullShader? hullShader);

        void DSSetShader(IDomainShader? domainShader);

        void CSSetShader(IComputeShader? computeShader);

        void SetInputLayout(IInputLayout? inputLayout);

        void Draw(int vertexCount, int offset);

        void DrawIndexed(int indexCount, int indexOffset, int vertexOffset);

        void DrawInstanced(int vertexCount, int instanceCount, int vertexOffset, int instanceOffset);

        void DrawIndexedInstanced(int indexCount, int instanceCount, int indexOffset, int vertexOffset, int instanceOffset);
    }
}