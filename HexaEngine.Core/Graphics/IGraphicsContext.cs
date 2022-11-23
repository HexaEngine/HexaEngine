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

        public void SetConstantBuffers(IBuffer[] constantBuffers, ShaderStage stage, int slot);

        public void VSSetConstantBuffer(IBuffer? constantBuffer, int slot);

        public void VSSetConstantBuffers(IBuffer[] constantBuffers, int slot);

        public void HSSetConstantBuffer(IBuffer? constantBuffer, int slot);

        public void HSSetConstantBuffers(IBuffer[] constantBuffers, int slot);

        public void DSSetConstantBuffer(IBuffer? constantBuffer, int slot);

        public void DSSetConstantBuffers(IBuffer[] constantBuffers, int slot);

        public void GSSetConstantBuffer(IBuffer? constantBuffer, int slot);

        public void GSSetConstantBuffers(IBuffer[] constantBuffers, int slot);

        public void PSSetConstantBuffer(IBuffer? constantBuffer, int slot);

        public void PSSetConstantBuffers(IBuffer[] constantBuffers, int slot);

        public void CSSetConstantBuffer(IBuffer? constantBuffer, int slot);

        public void CSSetConstantBuffers(IBuffer[] constantBuffers, int slot);

        public void SetShaderResource(IShaderResourceView? shaderResourceView, ShaderStage stage, int slot);

        public void SetShaderResources(IShaderResourceView[] shaderResourceViews, ShaderStage stage, int slot);

        public void VSSetShaderResource(IShaderResourceView? shaderResourceView, int slot);

        public void VSSetShaderResources(IShaderResourceView[] shaderResourceViews, int slot);

        public void HSSetShaderResource(IShaderResourceView? shaderResourceView, int slot);

        public void HSSetShaderResources(IShaderResourceView[] shaderResourceViews, int slot);

        public void DSSetShaderResource(IShaderResourceView? shaderResourceView, int slot);

        public void DSSetShaderResources(IShaderResourceView[] shaderResourceViews, int slot);

        public void GSSetShaderResource(IShaderResourceView? shaderResourceView, int slot);

        public void GSSetShaderResources(IShaderResourceView[] shaderResourceViews, int slot);

        public void PSSetShaderResource(IShaderResourceView? shaderResourceView, int slot);

        public void PSSetShaderResources(IShaderResourceView[] shaderResourceViews, int slot);

        public void CSSetShaderResource(IShaderResourceView? shaderResourceView, int slot);

        public void CSSetShaderResources(IShaderResourceView[] shaderResourceViews, int slot);

        public void ClearRenderTargetView(IRenderTargetView renderTargetView, Vector4 value);

        public void ClearRenderTargetViews(IRenderTargetView[] rtvs, Vector4 value);

        public void ClearDepthStencilView(IDepthStencilView depthStencilView, DepthStencilClearFlags flags, float depth, byte stencil);

        public void SetRenderTarget(IRenderTargetView? renderTargetView, IDepthStencilView? depthStencilView);

        public void SetRenderTargets(IRenderTargetView[] views, IDepthStencilView? depthStencilView);

        void SetScissorRect(int x, int y, int z, int w);

        void ClearState();

        void SetViewport(Viewport viewport);

        void SetBlendState(IBlendState? blendState, Vector4? factor = null, uint sampleMask = uint.MaxValue);

        void SetDepthStencilState(IDepthStencilState? depthStencilState, int stencilRef = 0);

        void SetRasterizerState(IRasterizerState? rasterizerState);

        void SetSampler(ISamplerState? sampler, ShaderStage stage, int slot);

        void SetSamplers(ISamplerState[] samplers, ShaderStage stage, int slot);

        void VSSetSampler(ISamplerState? sampler, int slot);

        void VSSetSamplers(ISamplerState[] samplers, int slot);

        void HSSetSampler(ISamplerState? sampler, int slot);

        void HSSetSamplers(ISamplerState[] samplers, int slot);

        void DSSetSampler(ISamplerState? sampler, int slot);

        void DSSetSamplers(ISamplerState[] samplers, int slot);

        void GSSetSampler(ISamplerState? sampler, int slot);

        void GSSetSamplers(ISamplerState[] samplers, int slot);

        void PSSetSampler(ISamplerState? sampler, int slot);

        void PSSetSamplers(ISamplerState[] samplers, int slot);

        void CSSetSampler(ISamplerState? sampler, int slot);

        void CSSetSamplers(ISamplerState[] samplers, int slot);

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

        void QueryBegin(IQuery query);

        void QueryEnd(IQuery query);

        void QueryGetData(IQuery query);

        void Flush();

        void SetConstantBuffers(IBuffer[] constantBuffers, ShaderStage stage, int slot, uint firstConstant, uint constantCount);

        void SetConstantBuffer(IBuffer? constantBuffer, ShaderStage stage, int slot, uint firstConstant, uint constantCount);

        void Dispatch(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ);

        void GenerateMips(IShaderResourceView resourceView);

        void ExecuteCommandList(ICommandList commandList, int restoreState);

        ICommandList FinishCommandList(int restoreState);

        void UpdateSubresource(IResource resource, int destSubresource, MappedSubresource subresource);

        void CSSetUnorderedAccessViews(int startSlot, int count, IUnorderedAccessView[] views, int uavInitialCounts = -1);

        void CSSetUnorderedAccessViews(int startSlot, IUnorderedAccessView[] views, int uavInitialCounts = -1);

        void CSSetUnorderedAccessViews(IUnorderedAccessView[] views, int uavInitialCounts = -1);

        void CSSetUnorderedAccessViews(IUnorderedAccessView[] views);
    }

    public interface IQuery : IDeviceChild
    {
    }

    public interface ICommandList : IDeviceChild
    {
    }
}