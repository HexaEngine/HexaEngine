namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using Silk.NET.OpenGL;
    using System;
    using System.Numerics;

    public unsafe class OpenGLGraphicsContext : IGraphicsContext
    {
        private readonly GL GL;

        private PrimitiveType primitiveType;
        private void* indexBuffer;
        private DrawElementsType indexType;

        public OpenGLGraphicsContext(GL gL)
        {
            GL = gL;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IGraphicsDevice Device => throw new NotImplementedException();

        public string? DebugName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsDisposed => throw new NotImplementedException();

        public nint NativePointer => throw new NotImplementedException();

        public event EventHandler? OnDisposed;

        public void ClearDepthStencilView(IDepthStencilView depthStencilView, DepthStencilClearFlags flags, float depth, byte stencil)
        {
            throw new NotImplementedException();
        }

        public void ClearRenderTargetView(IRenderTargetView renderTargetView, Vector4 value)
        {
            throw new NotImplementedException();
        }

        public unsafe void ClearRenderTargetViews(void** rtvs, uint count, Vector4 value)
        {
            throw new NotImplementedException();
        }

        public void ClearState()
        {
            throw new NotImplementedException();
        }

        public void ClearUnorderedAccessViewUint(IUnorderedAccessView uav, uint r, uint g, uint b, uint a)
        {
            throw new NotImplementedException();
        }

        public void CopyResource(IResource dst, IResource src)
        {
            throw new NotImplementedException();
        }

        public void CSSetConstantBuffer(IBuffer? constantBuffer, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void CSSetConstantBuffers(void** constantBuffers, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void CSSetSampler(ISamplerState? sampler, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void CSSetSamplers(void** samplers, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void CSSetShaderResource(void* shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public void CSSetShaderResource(IShaderResourceView? shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void CSSetShaderResources(void** shaderResourceViews, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void CSSetUnorderedAccessView(void* view, uint offset, int uavInitialCounts = -1)
        {
            throw new NotImplementedException();
        }

        public unsafe void CSSetUnorderedAccessViews(uint offset, void** views, uint count, int uavInitialCounts = -1)
        {
            throw new NotImplementedException();
        }

        public unsafe void CSSetUnorderedAccessViews(void** views, uint count, int uavInitialCounts = -1)
        {
            throw new NotImplementedException();
        }

        public void Dispatch(uint threadGroupCountX, uint threadGroupCountY, uint threadGroupCountZ)
        {
            GL.DispatchCompute(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
        }

        public void DispatchIndirect(IBuffer dispatchArgs, uint offset)
        {
            GL.DispatchComputeIndirect(dispatchArgs.NativePointer);
        }

        public void DrawIndexedInstanced(uint indexCount, uint instanceCount, uint indexOffset, int vertexOffset, uint instanceOffset)
        {
            GL.DrawElementsInstancedBaseVertexBaseInstance(primitiveType, indexCount - indexOffset, indexType, indexBuffer, instanceCount, vertexOffset, instanceOffset);
        }

        public void DrawIndexedInstancedIndirect(IBuffer bufferForArgs, uint alignedByteOffsetForArgs)
        {
            GL.DrawElementsIndirect(primitiveType, indexType, bufferForArgs.NativePointer);
        }

        public unsafe void DrawIndexedInstancedIndirect(void* bufferForArgs, uint alignedByteOffsetForArgs)
        {
            throw new NotImplementedException();
        }

        public void DrawInstanced(uint vertexCount, uint instanceCount, uint vertexOffset, uint instanceOffset)
        {
            throw new NotImplementedException();
        }

        public void DrawInstancedIndirect(IBuffer bufferForArgs, uint alignedByteOffsetForArgs)
        {
            throw new NotImplementedException();
        }

        public unsafe void DrawInstancedIndirect(void* bufferForArgs, uint alignedByteOffsetForArgs)
        {
            throw new NotImplementedException();
        }

        public void DSSetConstantBuffer(IBuffer? constantBuffer, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void DSSetConstantBuffers(void** constantBuffers, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void DSSetSampler(ISamplerState? sampler, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void DSSetSamplers(void** samplers, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void DSSetShaderResource(IShaderResourceView? shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void DSSetShaderResource(void* shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void DSSetShaderResources(void** shaderResourceViews, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void ExecuteCommandList(ICommandList commandList, bool restoreState)
        {
            throw new NotImplementedException();
        }

        public ICommandList FinishCommandList(bool restoreState)
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

        public void GSSetConstantBuffer(IBuffer? constantBuffer, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void GSSetConstantBuffers(void** constantBuffers, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void GSSetSampler(ISamplerState? sampler, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void GSSetSamplers(void** samplers, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void GSSetShaderResource(IShaderResourceView? shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void GSSetShaderResource(void* shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void GSSetShaderResources(void** shaderResourceViews, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void HSSetConstantBuffer(IBuffer? constantBuffer, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void HSSetConstantBuffers(void** constantBuffers, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void HSSetSampler(ISamplerState? sampler, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void HSSetSamplers(void** samplers, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void HSSetShaderResource(IShaderResourceView? shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void HSSetShaderResource(void* shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void HSSetShaderResources(void** shaderResourceViews, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public MappedSubresource Map(IResource resource, int subresourceIndex, MapMode mode, MapFlags flags)
        {
            throw new NotImplementedException();
        }

        public void PSSetConstantBuffer(IBuffer? constantBuffer, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void PSSetConstantBuffers(void** constantBuffers, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void PSSetSampler(ISamplerState? sampler, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void PSSetSamplers(void** samplers, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void PSSetShaderResource(IShaderResourceView? shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void PSSetShaderResource(void* shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void PSSetShaderResources(void** shaderResourceViews, uint count, int slot)
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

        public unsafe void Read(IBuffer buffer, void* value, int size)
        {
            throw new NotImplementedException();
        }

        public unsafe void Read<T>(IBuffer buffer, T* values, uint count) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void SetComputePipeline(IComputePipeline? pipeline)
        {
            throw new NotImplementedException();
        }

        public void SetGraphicsPipeline(IGraphicsPipeline? pipeline)
        {
            throw new NotImplementedException();
        }

        public void SetIndexBuffer(IBuffer? indexBuffer, Format format, int offset)
        {
            throw new NotImplementedException();
        }

        public void SetPrimitiveTopology(PrimitiveTopology topology)
        {
            throw new NotImplementedException();
        }

        public void SetRenderTarget(IRenderTargetView? renderTargetView, IDepthStencilView? depthStencilView)
        {
            throw new NotImplementedException();
        }

        public unsafe void SetRenderTargets(void** views, uint count, IDepthStencilView? depthStencilView)
        {
            throw new NotImplementedException();
        }

        public void SetScissorRect(int x, int y, int z, int w)
        {
            throw new NotImplementedException();
        }

        public void SetVertexBuffer(IBuffer? vertexBuffer, uint stride)
        {
            throw new NotImplementedException();
        }

        public void SetVertexBuffer(IBuffer? vertexBuffer, uint stride, uint offset)
        {
            throw new NotImplementedException();
        }

        public void SetVertexBuffer(uint slot, IBuffer? vertexBuffer, uint stride)
        {
            throw new NotImplementedException();
        }

        public void SetVertexBuffer(uint slot, IBuffer? vertexBuffer, uint stride, uint offset)
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

        public void UpdateSubresource(IResource resource, int destSubresource, MappedSubresource subresource)
        {
            throw new NotImplementedException();
        }

        public void VSSetConstantBuffer(IBuffer? constantBuffer, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void VSSetConstantBuffers(void** constantBuffers, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void VSSetSampler(ISamplerState? sampler, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void VSSetSamplers(void** samplers, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public void VSSetShaderResource(IShaderResourceView? shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void VSSetShaderResource(void* shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void VSSetShaderResources(void** shaderResourceViews, uint count, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void Write(IBuffer buffer, void* value, int size)
        {
            throw new NotImplementedException();
        }

        public unsafe void Write(IBuffer buffer, void* value, int size, Map flags)
        {
            throw new NotImplementedException();
        }

        public unsafe void Write<T>(IBuffer buffer, T* value, int size) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public unsafe void Write<T>(IBuffer buffer, T* value, int size, Map flags) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void Write<T>(IBuffer buffer, T value) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        internal void SetRasterizerState(RasterizerDescription rasterizer)
        {
            var face = Helper.Convert(rasterizer.CullMode);
            GL.PolygonMode(face, Helper.Convert(rasterizer.FillMode));
            GL.CullFace(face);
            GL.FrontFace(rasterizer.FrontCounterClockwise ? FrontFaceDirection.Ccw : FrontFaceDirection.CW);

            if (rasterizer.DepthBias != 0)
            {
                GL.Enable(EnableCap.PolygonOffsetFill);
            }
            else
            {
                GL.Disable(EnableCap.PolygonOffsetFill);
            }

            GL.PolygonOffset(rasterizer.SlopeScaledDepthBias, 1);
            GL.PolygonOffsetClamp(rasterizer.SlopeScaledDepthBias, 1, rasterizer.DepthBiasClamp);

            if (rasterizer.DepthClipEnable)
            {
                GL.Enable(EnableCap.DepthClamp);
            }
            else
            {
                GL.Disable(EnableCap.DepthClamp);
            }

            if (rasterizer.ScissorEnable)
            {
                GL.Enable(EnableCap.ScissorTest);
            }
            else
            {
                GL.Disable(EnableCap.ScissorTest);
            }

            if (rasterizer.MultisampleEnable)
            {
                GL.Enable(EnableCap.Multisample);
            }
            else
            {
                GL.Disable(EnableCap.Multisample);
            }

            if (rasterizer.AntialiasedLineEnable)
            {
                GL.Enable(EnableCap.LineSmooth);
            }
            else
            {
                GL.Disable(EnableCap.LineSmooth);
            }
        }

        internal void SetDepthStencilState(DepthStencilDescription depthStencil, uint stencilRef)
        {
            if (depthStencil.DepthEnable)
            {
                GL.Enable(EnableCap.DepthTest);
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
            }

            GL.DepthFunc(Helper.Convert(depthStencil.DepthFunc));
            GL.DepthMask(depthStencil.DepthWriteMask == DepthWriteMask.All);

            if (depthStencil.StencilEnable)
            {
                GL.Enable(EnableCap.StencilTest);
            }
            else
            {
                GL.Disable(EnableCap.StencilTest);
            }

            GL.StencilMask(depthStencil.StencilWriteMask);
            GL.StencilOpSeparate(TriangleFace.Front, Helper.Convert(depthStencil.FrontFace.StencilFailOp), Helper.Convert(depthStencil.FrontFace.StencilDepthFailOp), Helper.Convert(depthStencil.FrontFace.StencilPassOp));
            GL.StencilFuncSeparate(TriangleFace.Front, Helper.Convert2(depthStencil.FrontFace.StencilFunc), (int)stencilRef, depthStencil.StencilWriteMask);
            GL.StencilOpSeparate(TriangleFace.Back, Helper.Convert(depthStencil.BackFace.StencilFailOp), Helper.Convert(depthStencil.BackFace.StencilDepthFailOp), Helper.Convert(depthStencil.BackFace.StencilPassOp));
            GL.StencilFuncSeparate(TriangleFace.Back, Helper.Convert2(depthStencil.BackFace.StencilFunc), (int)stencilRef, depthStencil.StencilWriteMask);
        }

        internal void SetBlendState(BlendDescription blend, Vector4 blendFactor, uint sampleMask)
        {
            if (blend.AlphaToCoverageEnable)
            {
                GL.Enable(EnableCap.SampleAlphaToCoverage);
            }
            else
            {
                GL.Enable(EnableCap.SampleAlphaToCoverage);
            }

            for (uint i = 0; i < blend.RenderTarget.Length; i++)
            {
                var blendRT = blend.RenderTarget[i];
                if (blendRT.IsBlendEnabled)
                {
                    GL.Enable(EnableCap.Blend, i);
                }
                else
                {
                    GL.Disable(EnableCap.Blend, i);
                }

                GL.BlendEquationSeparate(i, Helper.Convert(blendRT.BlendOperation), Helper.Convert(blendRT.BlendOperationAlpha));
                GL.BlendFuncSeparate(i, Helper.Convert(blendRT.SourceBlend), Helper.Convert(blendRT.DestinationBlend), Helper.Convert(blendRT.SourceBlendAlpha), Helper.Convert(blendRT.DestinationBlendAlpha));
            }
            GL.BlendColor(blendFactor.X, blendFactor.Y, blendFactor.Z, blendFactor.W);
            GL.SampleMask(sampleMask, 1);
        }
    }
}