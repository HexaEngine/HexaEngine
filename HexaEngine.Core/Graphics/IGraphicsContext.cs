namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public interface IGraphicsContext : IDeviceChild
    {
        public IGraphicsDevice Device { get; }

        public void SetGraphicsPipeline(IGraphicsPipeline? pipeline);

        public void SetComputePipeline(IComputePipeline? pipeline);

        void CopyResource(IResource dst, IResource src);

        void CopyStructureCount(IBuffer dst, uint alignedByteOffset, IUnorderedAccessView uav);

        unsafe void Write(IBuffer buffer, void* value, int size);

        unsafe void Write(IBuffer buffer, void* value, int size, Map flags);

        unsafe void Write<T>(IBuffer buffer, T* value, int size) where T : unmanaged;

        unsafe void Write<T>(IBuffer buffer, T* value, int size, Map flags) where T : unmanaged;

        public void Write<T>(IBuffer buffer, T value) where T : unmanaged;

        unsafe void Read(IBuffer buffer, void* value, int size);

        unsafe void Read<T>(IBuffer buffer, T* values, uint count) where T : unmanaged;

        public MappedSubresource Map(IResource resource, int subresourceIndex, MapMode mode, MapFlags flags);

        public void Unmap(IResource resource, int subresourceIndex);

        public void SetVertexBuffer(IBuffer? vertexBuffer, uint stride);

        public void SetVertexBuffer(IBuffer? vertexBuffer, uint stride, uint offset);

        public void SetVertexBuffer(uint slot, IBuffer? vertexBuffer, uint stride);

        public void SetVertexBuffer(uint slot, IBuffer? vertexBuffer, uint stride, uint offset);

        public void SetIndexBuffer(IBuffer? indexBuffer, Format format, int offset);

        public void VSSetConstantBuffer(uint slot, IBuffer? constantBuffer);

        public void HSSetConstantBuffer(uint slot, IBuffer? constantBuffer);

        public void DSSetConstantBuffer(uint slot, IBuffer? constantBuffer);

        public void GSSetConstantBuffer(uint slot, IBuffer? constantBuffer);

        public void PSSetConstantBuffer(uint slot, IBuffer? constantBuffer);

        public void CSSetConstantBuffer(uint slot, IBuffer? constantBuffer);

        unsafe void VSSetConstantBuffers(uint slot, uint count, void** constantBuffers);

        unsafe void HSSetConstantBuffers(uint slot, uint count, void** constantBuffers);

        unsafe void DSSetConstantBuffers(uint slot, uint count, void** constantBuffers);

        unsafe void GSSetConstantBuffers(uint slot, uint count, void** constantBuffers);

        unsafe void PSSetConstantBuffers(uint slot, uint count, void** constantBuffers);

        unsafe void CSSetConstantBuffers(uint slot, uint count, void** constantBuffers);

        public void VSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView);

        public void HSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView);

        public void DSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView);

        public void GSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView);

        public void PSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView);

        public void CSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView);

        unsafe void VSSetShaderResources(uint slot, uint count, void** shaderResourceViews);

        unsafe void HSSetShaderResources(uint slot, uint count, void** shaderResourceViews);

        unsafe void DSSetShaderResources(uint slot, uint count, void** shaderResourceViews);

        unsafe void GSSetShaderResources(uint slot, uint count, void** shaderResourceViews);

        unsafe void PSSetShaderResources(uint slot, uint count, void** shaderResourceViews);

        unsafe void CSSetShaderResources(uint slot, uint count, void** shaderResourceViews);

        void VSSetSampler(uint slot, ISamplerState? sampler);

        void HSSetSampler(uint slot, ISamplerState? sampler);

        void DSSetSampler(uint slot, ISamplerState? sampler);

        void GSSetSampler(uint slot, ISamplerState? sampler);

        void PSSetSampler(uint slot, ISamplerState? sampler);

        void CSSetSampler(uint slot, ISamplerState? sampler);

        unsafe void VSSetSamplers(uint slot, uint count, void** samplers);

        unsafe void HSSetSamplers(uint slot, uint count, void** samplers);

        unsafe void DSSetSamplers(uint slot, uint count, void** samplers);

        unsafe void GSSetSamplers(uint slot, uint count, void** samplers);

        unsafe void PSSetSamplers(uint slot, uint count, void** samplers);

        unsafe void CSSetSamplers(uint slot, uint count, void** samplers);

        public void ClearRenderTargetView(IRenderTargetView renderTargetView, Vector4 value);

        public void ClearDepthStencilView(IDepthStencilView depthStencilView, DepthStencilClearFlags flags, float depth, byte stencil);

        public void SetRenderTarget(IRenderTargetView? renderTargetView, IDepthStencilView? depthStencilView);

        void SetScissorRect(int x, int y, int z, int w);

        void ClearState();

        void SetViewport(Viewport viewport);

        unsafe void SetViewports(uint count, Viewport* viewports);

        void SetPrimitiveTopology(PrimitiveTopology topology);

        void DrawInstanced(uint vertexCount, uint instanceCount, uint vertexOffset, uint instanceOffset);

        void DrawIndexedInstanced(uint indexCount, uint instanceCount, uint indexOffset, int vertexOffset, uint instanceOffset);

        void DrawIndexedInstancedIndirect(IBuffer bufferForArgs, uint alignedByteOffsetForArgs);

        unsafe void DrawIndexedInstancedIndirect(void* bufferForArgs, uint alignedByteOffsetForArgs);

        void DrawInstancedIndirect(IBuffer bufferForArgs, uint alignedByteOffsetForArgs);

        unsafe void DrawInstancedIndirect(void* bufferForArgs, uint alignedByteOffsetForArgs);

        void QueryBegin(IQuery query);

        void QueryEnd(IQuery query);

        void QueryGetData(IQuery query);

        void Flush();

        void Dispatch(uint threadGroupCountX, uint threadGroupCountY, uint threadGroupCountZ);

        void DispatchIndirect(IBuffer dispatchArgs, uint offset);

        void GenerateMips(IShaderResourceView resourceView);

        void ExecuteCommandList(ICommandList commandList, bool restoreState);

        ICommandList FinishCommandList(bool restoreState);

        void UpdateSubresource(IResource resource, int destSubresource, MappedSubresource subresource);

        unsafe void CSSetUnorderedAccessViews(uint offset, uint count, void** views, uint* uavInitialCounts);

        unsafe void CSSetUnorderedAccessViews(uint count, void** views, uint* uavInitialCounts);

        unsafe void CSSetUnorderedAccessView(uint offset, void* view, uint uavInitialCounts = unchecked((uint)-1));

        unsafe void CSSetUnorderedAccessView(void* view, uint uavInitialCounts = unchecked((uint)-1)) => CSSetUnorderedAccessView(0, view, uavInitialCounts);

        unsafe void SetRenderTargets(uint count, void** views, IDepthStencilView? depthStencilView);

        unsafe void ClearRenderTargetViews(uint count, void** rtvs, Vector4 value);

        void ClearUnorderedAccessViewUint(IUnorderedAccessView uav, uint r, uint g, uint b, uint a);

        void ClearView(IRenderTargetView rtv, Vector4 color, Rect rect);

        void ClearView(IDepthStencilView dsv, Vector4 color, Rect rect);

        void ClearView(IUnorderedAccessView uav, Vector4 color, Rect rect);
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