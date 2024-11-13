namespace HexaEngine.OpenGL
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using Hexa.NET.OpenGL;
    using System;
    using System.Numerics;

    public unsafe class OpenGLGraphicsContext : IGraphicsContext
    {
        private readonly GL GL;

        private GLPrimitiveType primitiveType;
        private void* indexBuffer;
        private GLDrawElementsType indexType;

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

        public unsafe void ClearRenderTargetViews(uint count, void** rtvs, Vector4 value)
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

        public void CSSetConstantBuffer(uint slot, IBuffer? constantBuffer)
        {
            throw new NotImplementedException();
        }

        public unsafe void CSSetConstantBuffers(uint slot, uint count, void** constantBuffers)
        {
            throw new NotImplementedException();
        }

        public void CSSetSampler(uint slot, ISamplerState? sampler)
        {
            throw new NotImplementedException();
        }

        public unsafe void CSSetSamplers(uint slot, uint count, void** samplers)
        {
            throw new NotImplementedException();
        }

        public unsafe void CSSetShaderResource(uint slot, void* shaderResourceView)
        {
            throw new NotImplementedException();
        }

        public void CSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView)
        {
            throw new NotImplementedException();
        }

        public unsafe void CSSetShaderResources(uint slot, uint count, void** shaderResourceViews)
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
            GL.DrawElementsInstancedBaseVertexBaseInstance(primitiveType, (int)(indexCount - indexOffset), indexType, indexBuffer, (int)instanceCount, vertexOffset, instanceOffset);
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

        public void DSSetConstantBuffer(uint slot, IBuffer? constantBuffer)
        {
            throw new NotImplementedException();
        }

        public unsafe void DSSetConstantBuffers(uint slot, uint count, void** constantBuffers)
        {
            throw new NotImplementedException();
        }

        public void DSSetSampler(uint slot, ISamplerState? sampler)
        {
            throw new NotImplementedException();
        }

        public unsafe void DSSetSamplers(uint slot, uint count, void** samplers)
        {
            throw new NotImplementedException();
        }

        public void DSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView)
        {
            throw new NotImplementedException();
        }

        public unsafe void DSSetShaderResource(void* shaderResourceView, int slot)
        {
            throw new NotImplementedException();
        }

        public unsafe void DSSetShaderResources(uint slot, uint count, void** shaderResourceViews)
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

        public void GSSetConstantBuffer(uint slot, IBuffer? constantBuffer)
        {
            throw new NotImplementedException();
        }

        public unsafe void GSSetConstantBuffers(uint slot, uint count, void** constantBuffers)
        {
            throw new NotImplementedException();
        }

        public void GSSetSampler(uint slot, ISamplerState? sampler)
        {
            throw new NotImplementedException();
        }

        public unsafe void GSSetSamplers(uint slot, uint count, void** samplers)
        {
            throw new NotImplementedException();
        }

        public void GSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView)
        {
            throw new NotImplementedException();
        }

        public unsafe void GSSetShaderResources(uint slot, uint count, void** shaderResourceViews)
        {
            throw new NotImplementedException();
        }

        public void HSSetConstantBuffer(uint slot, IBuffer? constantBuffer)
        {
            throw new NotImplementedException();
        }

        public unsafe void HSSetConstantBuffers(uint slot, uint count, void** constantBuffers)
        {
            throw new NotImplementedException();
        }

        public void HSSetSampler(uint slot, ISamplerState? sampler)
        {
            throw new NotImplementedException();
        }

        public unsafe void HSSetSamplers(uint slot, uint count, void** samplers)
        {
            throw new NotImplementedException();
        }

        public void HSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView)
        {
            throw new NotImplementedException();
        }

        public unsafe void HSSetShaderResources(uint slot, uint count, void** shaderResourceViews)
        {
            throw new NotImplementedException();
        }

        public MappedSubresource Map(IResource resource, int subresourceIndex, MapMode mode, MapFlags flags)
        {
            throw new NotImplementedException();
        }

        public void PSSetConstantBuffer(uint slot, IBuffer? constantBuffer)
        {
            throw new NotImplementedException();
        }

        public unsafe void PSSetConstantBuffers(uint slot, uint count, void** constantBuffers)
        {
            throw new NotImplementedException();
        }

        public void PSSetSampler(uint slot, ISamplerState? sampler)
        {
            throw new NotImplementedException();
        }

        public unsafe void PSSetSamplers(uint slot, uint count, void** samplers)
        {
            throw new NotImplementedException();
        }

        public void PSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView)
        {
            throw new NotImplementedException();
        }

        public unsafe void PSSetShaderResources(uint slot, uint count, void** shaderResourceViews)
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

        public unsafe void SetRenderTargets(uint count, void** views, IDepthStencilView? depthStencilView)
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

        public void VSSetConstantBuffer(uint slot, IBuffer? constantBuffer)
        {
            throw new NotImplementedException();
        }

        public unsafe void VSSetConstantBuffers(uint slot, uint count, void** constantBuffers)
        {
            throw new NotImplementedException();
        }

        public void VSSetSampler(uint slot, ISamplerState? sampler)
        {
            throw new NotImplementedException();
        }

        public unsafe void VSSetSamplers(uint slot, uint count, void** samplers)
        {
            throw new NotImplementedException();
        }

        public void VSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView)
        {
            throw new NotImplementedException();
        }

        public unsafe void VSSetShaderResources(uint slot, uint count, void** shaderResourceViews)
        {
            throw new NotImplementedException();
        }

        public unsafe void Write(IBuffer buffer, void* value, int size)
        {
            throw new NotImplementedException();
        }

        public unsafe void Write(IBuffer buffer, void* value, int size, MapMode flags)
        {
            throw new NotImplementedException();
        }

        public unsafe void Write<T>(IBuffer buffer, T* value, int size) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public unsafe void Write<T>(IBuffer buffer, T* value, int size, MapMode flags) where T : unmanaged
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
            GL.FrontFace(rasterizer.FrontCounterClockwise ? GLFrontFaceDirection.Ccw : GLFrontFaceDirection.Cw);

            if (rasterizer.DepthBias != 0)
            {
                GL.Enable(GLEnableCap.PolygonOffsetFill);
            }
            else
            {
                GL.Disable(GLEnableCap.PolygonOffsetFill);
            }

            GL.PolygonOffset(rasterizer.SlopeScaledDepthBias, 1);
            GL.PolygonOffsetClamp(rasterizer.SlopeScaledDepthBias, 1, rasterizer.DepthBiasClamp);

            if (rasterizer.DepthClipEnable)
            {
                GL.Enable(GLEnableCap.DepthClamp);
            }
            else
            {
                GL.Disable(GLEnableCap.DepthClamp);
            }

            if (rasterizer.ScissorEnable)
            {
                GL.Enable(GLEnableCap.ScissorTest);
            }
            else
            {
                GL.Disable(GLEnableCap.ScissorTest);
            }

            if (rasterizer.MultisampleEnable)
            {
                GL.Enable(GLEnableCap.Multisample);
            }
            else
            {
                GL.Disable(GLEnableCap.Multisample);
            }

            if (rasterizer.AntialiasedLineEnable)
            {
                GL.Enable(GLEnableCap.LineSmooth);
            }
            else
            {
                GL.Disable(GLEnableCap.LineSmooth);
            }
        }

        internal void SetDepthStencilState(DepthStencilDescription depthStencil, uint stencilRef)
        {
            if (depthStencil.DepthEnable)
            {
                GL.Enable(GLEnableCap.DepthTest);
            }
            else
            {
                GL.Disable(GLEnableCap.DepthTest);
            }

            GL.DepthFunc(Helper.Convert(depthStencil.DepthFunc));
            GL.DepthMask(depthStencil.DepthWriteMask == DepthWriteMask.All);

            if (depthStencil.StencilEnable)
            {
                GL.Enable(GLEnableCap.StencilTest);
            }
            else
            {
                GL.Disable(GLEnableCap.StencilTest);
            }

            GL.StencilMask(depthStencil.StencilWriteMask);
            GL.StencilOpSeparate(GLTriangleFace.Front, Helper.Convert(depthStencil.FrontFace.StencilFailOp), Helper.Convert(depthStencil.FrontFace.StencilDepthFailOp), Helper.Convert(depthStencil.FrontFace.StencilPassOp));
            GL.StencilFuncSeparate(GLTriangleFace.Front, Helper.Convert2(depthStencil.FrontFace.StencilFunc), (int)stencilRef, depthStencil.StencilWriteMask);
            GL.StencilOpSeparate(GLTriangleFace.Back, Helper.Convert(depthStencil.BackFace.StencilFailOp), Helper.Convert(depthStencil.BackFace.StencilDepthFailOp), Helper.Convert(depthStencil.BackFace.StencilPassOp));
            GL.StencilFuncSeparate(GLTriangleFace.Back, Helper.Convert2(depthStencil.BackFace.StencilFunc), (int)stencilRef, depthStencil.StencilWriteMask);
        }

        internal void SetBlendState(BlendDescription blend, Vector4 blendFactor, uint sampleMask)
        {
            if (blend.AlphaToCoverageEnable)
            {
                GL.Enable(GLEnableCap.SampleAlphaToCoverage);
            }
            else
            {
                GL.Disable(GLEnableCap.SampleAlphaToCoverage);
            }

            for (uint i = 0; i < blend.RenderTarget.Length; i++)
            {
                var blendRT = blend.RenderTarget[i];
                if (blendRT.IsBlendEnabled)
                {
                    GL.Enablei(GLEnableCap.Blend, i);
                }
                else
                {
                    GL.Disablei(GLEnableCap.Blend, i);
                }

                GL.BlendEquationSeparatei(i, Helper.Convert(blendRT.BlendOperation), Helper.Convert(blendRT.BlendOperationAlpha));
                GL.BlendFuncSeparatei(i, Helper.Convert(blendRT.SourceBlend), Helper.Convert(blendRT.DestinationBlend), Helper.Convert(blendRT.SourceBlendAlpha), Helper.Convert(blendRT.DestinationBlendAlpha));
            }
            GL.BlendColor(blendFactor.X, blendFactor.Y, blendFactor.Z, blendFactor.W);
            GL.SampleMaski(sampleMask, 1);
        }

        public void CopyStructureCount(IBuffer dst, uint alignedByteOffset, IUnorderedAccessView uav)
        {
            throw new NotImplementedException();
        }

        public void CSSetUnorderedAccessViews(uint offset, uint count, void** views, uint* uavInitialCounts)
        {
            throw new NotImplementedException();
        }

        public void CSSetUnorderedAccessViews(uint count, void** views, uint* uavInitialCounts)
        {
            throw new NotImplementedException();
        }

        public void CSSetUnorderedAccessView(uint offset, void* view, uint uavInitialCounts = uint.MaxValue)
        {
            throw new NotImplementedException();
        }

        public void SetViewports(uint count, Viewport* viewports)
        {
            throw new NotImplementedException();
        }

        public void ClearView(IRenderTargetView rtv, Vector4 color, Rect rect)
        {
            throw new NotImplementedException();
        }

        public void ClearView(IDepthStencilView dsv, Vector4 color, Rect rect)
        {
            throw new NotImplementedException();
        }

        public void ClearView(IUnorderedAccessView uav, Vector4 color, Rect rect)
        {
            throw new NotImplementedException();
        }

        public void SetRenderTargetsAndUnorderedAccessViews(IRenderTargetView? renderTargetView, IDepthStencilView? depthStencilView, uint uavSlot, IUnorderedAccessView? unorderedAccessView, uint uavInitialCount = uint.MaxValue)
        {
            throw new NotImplementedException();
        }

        public void SetRenderTargetsAndUnorderedAccessViews(uint count, void** views, IDepthStencilView? depthStencilView, uint uavSlot, uint uavCount, void** uavs, uint* pUavInitialCount)
        {
            throw new NotImplementedException();
        }

        public void BeginEvent(string name)
        {
            throw new NotImplementedException();
        }

        public void EndEvent()
        {
            throw new NotImplementedException();
        }

        public void Signal(IFence fence, ulong value)
        {
            throw new NotImplementedException();
        }

        public void Wait(IFence fence, ulong value)
        {
            throw new NotImplementedException();
        }

        public void SetGraphicsPipelineState(IGraphicsPipelineState? state)
        {
            throw new NotImplementedException();
        }

        public void SetResourceBindingList(IResourceBindingList rootDescriptorTable)
        {
            throw new NotImplementedException();
        }

        public void UnsetResourceBindingList(IResourceBindingList rootDescriptorTable)
        {
            throw new NotImplementedException();
        }

        public void ExecuteCommandBuffer(ICommandBuffer commandBuffer)
        {
            throw new NotImplementedException();
        }

        public void SetComputePipelineState(IComputePipelineState? state)
        {
            throw new NotImplementedException();
        }
    }
}