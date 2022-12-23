namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public interface IGraphicsContext : IDeviceChild
    {
        public IGraphicsDevice Device { get; }

        void CopyResource(IResource dst, IResource src);

        unsafe void Write(IBuffer buffer, void* value, int size);

        unsafe void Write<T>(IBuffer buffer, T* value, int size) where T : unmanaged;

        public void Write<T>(IBuffer buffer, T value) where T : struct;

        public void Write<T>(IBuffer buffer, T[] values) where T : struct;

        public void Write<T>(IBuffer buffer, T[] values, int structSize) where T : unmanaged;

        public void Read<T>(IBuffer buffer, T value) where T : struct;

        public void Read<T>(IBuffer buffer, T[] values) where T : struct;

        public MappedSubresource Map(IResource resource, int subresourceIndex, MapMode mode, MapFlags flags);

        public void Unmap(IResource resource, int subresourceIndex);

        public void SetVertexBuffer(IBuffer? vertexBuffer, int stride);

        public void SetVertexBuffer(IBuffer? vertexBuffer, int stride, int offset);

        public void SetVertexBuffer(int slot, IBuffer? vertexBuffer, int stride);

        public void SetVertexBuffer(int slot, IBuffer? vertexBuffer, int stride, int offset);

        public void SetIndexBuffer(IBuffer? indexBuffer, Format format, int offset);

        public void VSSetConstantBuffer(IBuffer? constantBuffer, int slot);

        public void HSSetConstantBuffer(IBuffer? constantBuffer, int slot);

        public void DSSetConstantBuffer(IBuffer? constantBuffer, int slot);

        public void GSSetConstantBuffer(IBuffer? constantBuffer, int slot);

        public void PSSetConstantBuffer(IBuffer? constantBuffer, int slot);

        public void CSSetConstantBuffer(IBuffer? constantBuffer, int slot);

        unsafe void VSSetConstantBuffers(void** constantBuffers, uint count, int slot);

        unsafe void HSSetConstantBuffers(void** constantBuffers, uint count, int slot);

        unsafe void DSSetConstantBuffers(void** constantBuffers, uint count, int slot);

        unsafe void GSSetConstantBuffers(void** constantBuffers, uint count, int slot);

        unsafe void PSSetConstantBuffers(void** constantBuffers, uint count, int slot);

        unsafe void CSSetConstantBuffers(void** constantBuffers, uint count, int slot);

        public void VSSetShaderResource(IShaderResourceView? shaderResourceView, int slot);

        unsafe void VSSetShaderResource(void* shaderResourceView, int slot);

        public void HSSetShaderResource(IShaderResourceView? shaderResourceView, int slot);

        unsafe void HSSetShaderResource(void* shaderResourceView, int slot);

        public void DSSetShaderResource(IShaderResourceView? shaderResourceView, int slot);

        unsafe void DSSetShaderResource(void* shaderResourceView, int slot);

        public void GSSetShaderResource(IShaderResourceView? shaderResourceView, int slot);

        unsafe void GSSetShaderResource(void* shaderResourceView, int slot);

        public void PSSetShaderResource(IShaderResourceView? shaderResourceView, int slot);

        public unsafe void PSSetShaderResource(void* shaderResourceView, int slot);

        unsafe void CSSetShaderResource(void* shaderResourceView, int slot);

        public void CSSetShaderResource(IShaderResourceView? shaderResourceView, int slot);

        unsafe void VSSetShaderResources(void** shaderResourceViews, uint count, int slot);

        unsafe void HSSetShaderResources(void** shaderResourceViews, uint count, int slot);

        unsafe void DSSetShaderResources(void** shaderResourceViews, uint count, int slot);

        unsafe void GSSetShaderResources(void** shaderResourceViews, uint count, int slot);

        unsafe void PSSetShaderResources(void** shaderResourceViews, uint count, int slot);

        unsafe void CSSetShaderResources(void** shaderResourceViews, uint count, int slot);

        void VSSetSampler(ISamplerState? sampler, int slot);

        void HSSetSampler(ISamplerState? sampler, int slot);

        void DSSetSampler(ISamplerState? sampler, int slot);

        void GSSetSampler(ISamplerState? sampler, int slot);

        void PSSetSampler(ISamplerState? sampler, int slot);

        void CSSetSampler(ISamplerState? sampler, int slot);

        unsafe void VSSetSamplers(void** samplers, uint count, int slot);

        unsafe void HSSetSamplers(void** samplers, uint count, int slot);

        unsafe void DSSetSamplers(void** samplers, uint count, int slot);

        unsafe void GSSetSamplers(void** samplers, uint count, int slot);

        unsafe void PSSetSamplers(void** samplers, uint count, int slot);

        unsafe void CSSetSamplers(void** samplers, uint count, int slot);

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

        void SetPrimitiveTopology(PrimitiveTopology topology);

        void VSSetShader(IVertexShader? vertexShader);

        void PSSetShader(IPixelShader? pixelShader);

        void GSSetShader(IGeometryShader? geometryShader);

        void HSSetShader(IHullShader? hullShader);

        void DSSetShader(IDomainShader? domainShader);

        void CSSetShader(IComputeShader? computeShader);

        void SetInputLayout(IInputLayout? inputLayout);

        void DrawInstanced(int vertexCount, int instanceCount, int vertexOffset, int instanceOffset);

        void DrawIndexedInstanced(int indexCount, int instanceCount, int indexOffset, int vertexOffset, int instanceOffset);

        void DrawIndexedInstancedIndirect(IBuffer bufferForArgs, uint alignedByteOffsetForArgs);

        unsafe void DrawIndexedInstancedIndirect(void* bufferForArgs, uint alignedByteOffsetForArgs);

        void DrawInstancedIndirect(IBuffer bufferForArgs, uint alignedByteOffsetForArgs);

        unsafe void DrawInstancedIndirect(void* bufferForArgs, uint alignedByteOffsetForArgs);

        void QueryBegin(IQuery query);

        void QueryEnd(IQuery query);

        void QueryGetData(IQuery query);

        void Flush();

        void Dispatch(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ);

        void GenerateMips(IShaderResourceView resourceView);

        void ExecuteCommandList(ICommandList commandList, int restoreState);

        ICommandList FinishCommandList(int restoreState);

        void UpdateSubresource(IResource resource, int destSubresource, MappedSubresource subresource);

        unsafe void CSSetUnorderedAccessViews(uint offset, void** views, uint count, int uavInitialCounts = -1);

        unsafe void CSSetUnorderedAccessViews(void** views, uint count, int uavInitialCounts = -1);

        unsafe void SetRenderTargets(void** views, uint count, IDepthStencilView? depthStencilView);

        unsafe void ClearRenderTargetViews(void** rtvs, uint count, Vector4 value);
    }

    public interface IQuery : IDeviceChild
    {
    }

    public interface IPredicate : IDeviceChild
    {
    }

    public interface ICommandList : IDeviceChild
    {
    }
}