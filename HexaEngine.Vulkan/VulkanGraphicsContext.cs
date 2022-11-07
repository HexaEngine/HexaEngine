namespace HexaEngine.Vulkan
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public class VulkanGraphicsContext : IGraphicsContext
    {
        public IGraphicsDevice Device { get; }
        public IntPtr NativePointer { get; }
        public string? DebugName { get; set; }
        public bool IsDisposed { get; }

        public event EventHandler? OnDisposed;

        public VulkanGraphicsContext()
        {
            throw new NotImplementedException();
        }

        public void ClearDepthStencilView(IDepthStencilView depthStencilView, DepthStencilClearFlags flags, float depth, byte stencil)
        {
            throw new NotImplementedException();
        }

        public void ClearRenderTargetView(IRenderTargetView renderTargetView, Vector4 value)
        {
            throw new NotImplementedException();
        }

        public void ClearRenderTargetViews(IRenderTargetView[] rtvs, Vector4 value)
        {
            throw new NotImplementedException();
        }

        public void ClearState()
        {
            throw new NotImplementedException();
        }

        public void CSSetShader(IComputeShader? computeShader)
        {
            throw new NotImplementedException();
        }

        public void Dispatch(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Draw(int vertexCount, int offset)
        {
            throw new NotImplementedException();
        }

        public void DrawIndexed(int indexCount, int indexOffset, int vertexOffset)
        {
            throw new NotImplementedException();
        }

        public void DrawIndexedInstanced(int indexCount, int instanceCount, int indexOffset, int vertexOffset, int instanceOffset)
        {
            throw new NotImplementedException();
        }

        public void DrawInstanced(int vertexCount, int instanceCount, int vertexOffset, int instanceOffset)
        {
            throw new NotImplementedException();
        }

        public void DSSetShader(IDomainShader? domainShader)
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public void GenerateMips(IShaderResourceView resourceView)
        {
            throw new NotImplementedException();
        }

        public void GSSetShader(IGeometryShader? geometryShader)
        {
            throw new NotImplementedException();
        }

        public void HSSetShader(IHullShader? hullShader)
        {
            throw new NotImplementedException();
        }

        public MappedSubresource Map(IResource resource, int subresourceIndex, MapMode mode, MapFlags flags)
        {
            throw new NotImplementedException();
        }

        public void PSSetShader(IPixelShader? pixelShader)
        {
            throw new NotImplementedException();
        }

        public void QueryBegin(IQuery query)
        {
            throw new NotImplementedException();
        }

        public void QueryEnd(IQuery query)
        {
            throw new NotImplementedException();
        }

        public void QueryGetData(IQuery query)
        {
            throw new NotImplementedException();
        }

        public void Read<T>(IBuffer buffer, T value) where T : struct
        {
            throw new NotImplementedException();
        }

        public void Read<T>(IBuffer buffer, T[] values) where T : struct
        {
            throw new NotImplementedException();
        }

        public void SetBlendState(IBlendState? blendState, Vector4? factor = null, uint sampleMask = uint.MaxValue)
        {
            throw new NotImplementedException();
        }

        public void SetConstantBuffer(IBuffer? constantBuffer, ShaderStage stage, int slot)
        {
            throw new NotImplementedException();
        }

        public void SetConstantBuffer(IBuffer? constantBuffer, ShaderStage stage, int slot, uint firstConstant, uint constantCount)
        {
            throw new NotImplementedException();
        }

        public void SetConstantBuffers(IBuffer[] constantBuffers, ShaderStage stage, int slot)
        {
            throw new NotImplementedException();
        }

        public void SetConstantBuffers(IBuffer[] constantBuffers, ShaderStage stage, int slot, uint firstConstant, uint constantCount)
        {
            throw new NotImplementedException();
        }

        public void SetDepthStencilState(IDepthStencilState? depthStencilState, int stencilRef = 0)
        {
            throw new NotImplementedException();
        }

        public void SetIndexBuffer(IBuffer? indexBuffer, Format format, int offset)
        {
            throw new NotImplementedException();
        }

        public void SetInputLayout(IInputLayout? inputLayout)
        {
            throw new NotImplementedException();
        }

        public void SetPrimitiveTopology(PrimitiveTopology topology)
        {
            throw new NotImplementedException();
        }

        public void SetRasterizerState(IRasterizerState? rasterizerState)
        {
            throw new NotImplementedException();
        }

        public void SetRenderTarget(IRenderTargetView? renderTargetView, IDepthStencilView? depthStencilView)
        {
            throw new NotImplementedException();
        }

        public void SetRenderTargets(IRenderTargetView[]? views, IDepthStencilView? depthStencilView)
        {
            throw new NotImplementedException();
        }

        public void SetSampler(ISamplerState? sampler, ShaderStage stage, int slot)
        {
            throw new NotImplementedException();
        }

        public void SetSamplers(ISamplerState[] samplers, ShaderStage stage, int slot)
        {
            throw new NotImplementedException();
        }

        public void SetScissorRect(int x, int y, int z, int w)
        {
            throw new NotImplementedException();
        }

        public void SetShaderResource(IShaderResourceView? shaderResourceView, ShaderStage stage, int slot)
        {
            throw new NotImplementedException();
        }

        public void SetShaderResources(IShaderResourceView[] shaderResourceViews, ShaderStage stage, int slot)
        {
            throw new NotImplementedException();
        }

        public void SetVertexBuffer(IBuffer? vertexBuffer, int stride)
        {
            throw new NotImplementedException();
        }

        public void SetVertexBuffer(IBuffer? vertexBuffer, int stride, int offset)
        {
            throw new NotImplementedException();
        }

        public void SetVertexBuffer(int slot, IBuffer? vertexBuffer, int stride)
        {
            throw new NotImplementedException();
        }

        public void SetVertexBuffer(int slot, IBuffer? vertexBuffer, int stride, int offset)
        {
            throw new NotImplementedException();
        }

        public void SetViewport(Viewport viewport)
        {
            throw new NotImplementedException();
        }

        public void Unmap(IResource resource, int subresourceIndex)
        {
            throw new NotImplementedException();
        }

        public void VSSetShader(IVertexShader? vertexShader)
        {
            throw new NotImplementedException();
        }

        public void Write<T>(IBuffer buffer, T value) where T : struct
        {
            throw new NotImplementedException();
        }

        public void Write<T>(IBuffer buffer, T[] values) where T : struct
        {
            throw new NotImplementedException();
        }

        public void ExecuteCommandList(ICommandList commandList, int restoreState)
        {
            throw new NotImplementedException();
        }

        public ICommandList FinishCommandList(int restoreState)
        {
            throw new NotImplementedException();
        }

        public void UpdateSubresource(IResource resource, int destSubresource, MappedSubresource subresource)
        {
            throw new NotImplementedException();
        }
    }
}