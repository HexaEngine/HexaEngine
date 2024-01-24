namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using Silk.NET.Core;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using Silk.NET.Maths;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using static System.Net.Mime.MediaTypeNames;
    using Format = Core.Graphics.Format;
    using MapMode = Core.Graphics.MapMode;
    using MappedSubresource = Core.Graphics.MappedSubresource;
    using Viewport = Mathematics.Viewport;

    public unsafe class D3D11GraphicsContextBase
    {
        internal ComPtr<ID3D11DeviceContext3> DeviceContext;
        private bool disposedValue;

        public string? DebugName { get; set; } = string.Empty;

        public IGraphicsDevice Device { get; }

        public bool IsDisposed => disposedValue;

        public IntPtr NativePointer { get; }

        public event EventHandler? OnDisposed;

        protected D3D11GraphicsContextBase(IGraphicsDevice device, ComPtr<ID3D11DeviceContext4> context)
        {
            Device = device;
            DeviceContext.Handle = (ID3D11DeviceContext3*)context.Handle;
        }

        protected D3D11GraphicsContextBase(IGraphicsDevice device, ComPtr<ID3D11DeviceContext3> context)
        {
            Device = device;
            DeviceContext = context;
        }

        #region IGraphicsContext

        public void BeginEvent(string name)
        {
            DeviceContext.BeginEventInt(name, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearDepthStencilView(IDepthStencilView depthStencilView, DepthStencilClearFlags flags, float depth, byte stencil)
        {
            DeviceContext.ClearDepthStencilView((ID3D11DepthStencilView*)depthStencilView.NativePointer, (uint)Helper.Convert(flags), depth, stencil);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearRenderTargetView(IRenderTargetView renderTargetView, Vector4 value)
        {
            DeviceContext.ClearRenderTargetView((ID3D11RenderTargetView*)renderTargetView.NativePointer, (float*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearRenderTargetViews(uint count, void** rtvs, Vector4 value)
        {
            for (int i = 0; i < count; i++)
            {
                DeviceContext.ClearRenderTargetView((ID3D11RenderTargetView*)rtvs[i], (float*)&value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearState()
        {
            DeviceContext.ClearState();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearUnorderedAccessViewFloat(IUnorderedAccessView uav, float r, float g, float b, float a)
        {
            float* values = stackalloc float[4] { r, g, b, a };
            DeviceContext.ClearUnorderedAccessViewFloat((ID3D11UnorderedAccessView*)uav.NativePointer, values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearUnorderedAccessViewUint(IUnorderedAccessView uav, uint r, uint g, uint b, uint a)
        {
            uint* values = stackalloc uint[4] { r, g, b, a };
            DeviceContext.ClearUnorderedAccessViewUint((ID3D11UnorderedAccessView*)uav.NativePointer, values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearView(IDepthStencilView dsv, Vector4 color, Rect rect)
        {
            var rec = Helper.Convert(rect);
            DeviceContext.ClearView((ID3D11View*)dsv.NativePointer, (float*)&color, &rec, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearView(IRenderTargetView rtv, Vector4 color, Rect rect)
        {
            var rec = Helper.Convert(rect);
            DeviceContext.ClearView((ID3D11View*)rtv.NativePointer, (float*)&color, &rec, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearView(IUnorderedAccessView uav, Vector4 color, Rect rect)
        {
            var rec = Helper.Convert(rect);
            DeviceContext.ClearView((ID3D11View*)uav.NativePointer, (float*)&color, &rec, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyResource(IResource dst, IResource src)
        {
            DeviceContext.CopyResource((ID3D11Resource*)dst.NativePointer, (ID3D11Resource*)src.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyStructureCount(IBuffer dst, uint alignedByteOffset, IUnorderedAccessView uav)
        {
            DeviceContext.CopyStructureCount((ID3D11Buffer*)dst.NativePointer, alignedByteOffset, (ID3D11UnorderedAccessView*)uav.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetConstantBuffer(uint slot, IBuffer? constantBuffer)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer?.NativePointer;
#nullable enable
            DeviceContext.CSSetConstantBuffers(slot, 1, &buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetConstantBuffers(uint slot, uint count, void** constantBuffers)
        {
            DeviceContext.CSSetConstantBuffers(slot, count, (ID3D11Buffer**)constantBuffers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetSampler(uint slot, ISamplerState? sampler)
        {
#nullable disable
            ID3D11SamplerState* smp = (ID3D11SamplerState*)sampler?.NativePointer;
#nullable enable
            DeviceContext.CSSetSamplers(slot, 1, &smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetSamplers(uint slot, uint count, void** samplers)
        {
            DeviceContext.CSSetSamplers(slot, count, (ID3D11SamplerState**)samplers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView)
        {
#nullable disable
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView?.NativePointer;
#nullable enable
            DeviceContext.CSSetShaderResources(slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetShaderResources(uint slot, uint count, void** shaderResourceViews)
        {
            DeviceContext.CSSetShaderResources(slot, count, (ID3D11ShaderResourceView**)shaderResourceViews);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessView(uint offset, void* view, uint uavInitialCount = unchecked((uint)-1))
        {
            uint pUAVInitialCount = uavInitialCount;
            DeviceContext.CSSetUnorderedAccessViews(offset, 1, (ID3D11UnorderedAccessView**)&view, &pUAVInitialCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessViews(uint offset, uint count, void** views, uint* uavInitialCounts)
        {
            DeviceContext.CSSetUnorderedAccessViews(offset, count, (ID3D11UnorderedAccessView**)views, uavInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessViews(uint count, void** views, uint* uavInitialCounts)
        {
            DeviceContext.CSSetUnorderedAccessViews(0, count, (ID3D11UnorderedAccessView**)views, uavInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispatch(uint threadGroupCountX, uint threadGroupCountY, uint threadGroupCountZ)
        {
            DeviceContext.Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
        }

        public void DispatchIndirect(IBuffer dispatchArgs, uint offset)
        {
            DeviceContext.DispatchIndirect((ID3D11Buffer*)dispatchArgs.NativePointer, offset);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawIndexedInstanced(uint indexCount, uint instanceCount, uint indexOffset, int vertexOffset, uint instanceOffset)
        {
            DeviceContext.DrawIndexedInstanced(indexCount, instanceCount, indexOffset, vertexOffset, instanceOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawIndexedInstancedIndirect(IBuffer bufferForArgs, uint alignedByteOffsetForArgs)
        {
            DeviceContext.DrawIndexedInstancedIndirect((ID3D11Buffer*)bufferForArgs.NativePointer, alignedByteOffsetForArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawIndexedInstancedIndirect(void* bufferForArgs, uint alignedByteOffsetForArgs)
        {
            DeviceContext.DrawIndexedInstancedIndirect((ID3D11Buffer*)bufferForArgs, alignedByteOffsetForArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawInstanced(uint vertexCount, uint instanceCount, uint vertexOffset, uint instanceOffset)
        {
            DeviceContext.DrawInstanced(vertexCount, instanceCount, vertexOffset, instanceOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawInstancedIndirect(IBuffer bufferForArgs, uint alignedByteOffsetForArgs)
        {
            DeviceContext.DrawInstancedIndirect((ID3D11Buffer*)bufferForArgs.NativePointer, alignedByteOffsetForArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawInstancedIndirect(void* bufferForArgs, uint alignedByteOffsetForArgs)
        {
            DeviceContext.DrawInstancedIndirect((ID3D11Buffer*)bufferForArgs, alignedByteOffsetForArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetConstantBuffer(uint slot, IBuffer? constantBuffer)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer?.NativePointer;
#nullable enable
            DeviceContext.DSSetConstantBuffers(slot, 1, &buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetConstantBuffers(uint slot, uint count, void** constantBuffers)
        {
            DeviceContext.DSSetConstantBuffers(slot, count, (ID3D11Buffer**)constantBuffers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetSampler(uint slot, ISamplerState? sampler)
        {
#nullable disable
            ID3D11SamplerState* smp = (ID3D11SamplerState*)sampler?.NativePointer;
#nullable enable
            DeviceContext.DSSetSamplers(slot, 1, &smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetSamplers(uint slot, uint count, void** samplers)
        {
            DeviceContext.DSSetSamplers(slot, count, (ID3D11SamplerState**)samplers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView)
        {
#nullable disable
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView?.NativePointer;
#nullable enable
            DeviceContext.DSSetShaderResources(slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetShaderResources(uint slot, uint count, void** shaderResourceViews)
        {
            DeviceContext.DSSetShaderResources(slot, count, (ID3D11ShaderResourceView**)shaderResourceViews);
        }

        public void EndEvent()
        {
            DeviceContext.EndEvent();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteCommandList(ICommandList commandList, bool restoreState)
        {
            DeviceContext.ExecuteCommandList((ID3D11CommandList*)commandList.NativePointer, restoreState);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICommandList FinishCommandList(bool restoreState)
        {
            ComPtr<ID3D11CommandList> commandList = default;
            DeviceContext.FinishCommandList((Bool32)restoreState, ref commandList);
            return new D3D11CommandList(commandList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Flush()
        {
            DeviceContext.Flush();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GenerateMips(IShaderResourceView resourceView)
        {
            DeviceContext.GenerateMips((ID3D11ShaderResourceView*)resourceView.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetConstantBuffer(uint slot, IBuffer? constantBuffer)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer?.NativePointer;
#nullable enable
            DeviceContext.GSSetConstantBuffers(slot, 1, &buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetConstantBuffers(uint slot, uint count, void** constantBuffers)
        {
            DeviceContext.GSSetConstantBuffers(slot, count, (ID3D11Buffer**)constantBuffers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetSampler(uint slot, ISamplerState? sampler)
        {
#nullable disable
            ID3D11SamplerState* smp = (ID3D11SamplerState*)sampler?.NativePointer;
#nullable enable
            DeviceContext.GSSetSamplers(slot, 1, &smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetSamplers(uint slot, uint count, void** samplers)
        {
            DeviceContext.GSSetSamplers(slot, count, (ID3D11SamplerState**)samplers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView)
        {
#nullable disable
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView?.NativePointer;
#nullable enable
            DeviceContext.GSSetShaderResources(slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetShaderResources(uint slot, uint count, void** shaderResourceViews)
        {
            DeviceContext.GSSetShaderResources(slot, count, (ID3D11ShaderResourceView**)shaderResourceViews);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetConstantBuffer(uint slot, IBuffer? constantBuffer)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer?.NativePointer;
#nullable enable
            DeviceContext.HSSetConstantBuffers(slot, 1, &buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetConstantBuffers(uint slot, uint count, void** constantBuffers)
        {
            DeviceContext.HSSetConstantBuffers(slot, count, (ID3D11Buffer**)constantBuffers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetSampler(uint slot, ISamplerState? sampler)
        {
#nullable disable
            ID3D11SamplerState* smp = (ID3D11SamplerState*)sampler?.NativePointer;
#nullable enable
            DeviceContext.HSSetSamplers(slot, 1, &smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetSamplers(uint slot, uint count, void** samplers)
        {
            DeviceContext.HSSetSamplers(slot, count, (ID3D11SamplerState**)samplers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView)
        {
#nullable disable
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView?.NativePointer;
#nullable enable
            DeviceContext.HSSetShaderResources(slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetShaderResources(uint slot, uint count, void** shaderResourceViews)
        {
            DeviceContext.HSSetShaderResources(slot, count, (ID3D11ShaderResourceView**)shaderResourceViews);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MappedSubresource Map(IResource resource, int subresourceIndex, MapMode mode, MapFlags flags)
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            DeviceContext.Map((ID3D11Resource*)resource.NativePointer, (uint)subresourceIndex, (Map)Helper.Convert((MapMode)mode), (uint)Helper.Convert(flags), &data);
            return new(data.PData, data.RowPitch, data.DepthPitch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetConstantBuffer(uint slot, IBuffer? constantBuffer)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer?.NativePointer;
#nullable enable
            DeviceContext.PSSetConstantBuffers(slot, 1, &buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetConstantBuffers(uint slot, uint count, void** constantBuffers)
        {
            DeviceContext.PSSetConstantBuffers(slot, count, (ID3D11Buffer**)constantBuffers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetSampler(uint slot, ISamplerState? sampler)
        {
#nullable disable
            ID3D11SamplerState* smp = (ID3D11SamplerState*)sampler?.NativePointer;
#nullable enable
            DeviceContext.PSSetSamplers(slot, 1, &smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetSamplers(uint slot, uint count, void** samplers)
        {
            DeviceContext.PSSetSamplers(slot, count, (ID3D11SamplerState**)samplers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView)
        {
#nullable disable
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView?.NativePointer;
#nullable enable
            DeviceContext.PSSetShaderResources(slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetShaderResources(uint slot, uint count, void** shaderResourceViews)
        {
            DeviceContext.PSSetShaderResources(slot, count, (ID3D11ShaderResourceView**)shaderResourceViews);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueryBegin(IQuery query)
        {
            DeviceContext.Begin((ID3D11Asynchronous*)query.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueryEnd(IQuery query)
        {
            DeviceContext.End((ID3D11Asynchronous*)query.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueryGetData(IQuery query)
        {
            DeviceContext.GetData((ID3D11Asynchronous*)query.NativePointer, null, 0, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(IBuffer buffer, void* values, int length)
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Silk.NET.Direct3D11.Map.Read, 0, &data);

            long destinationLength = length;
            long bytesToCopy = data.RowPitch > destinationLength ? destinationLength : data.RowPitch;
            Buffer.MemoryCopy(data.PData, values, destinationLength, bytesToCopy);

            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(IBuffer buffer, T* values, uint count) where T : unmanaged
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Silk.NET.Direct3D11.Map.Read, 0, &data);

            long destinationLength = count * sizeof(T);
            long bytesToCopy = data.RowPitch > destinationLength ? destinationLength : data.RowPitch;
            Buffer.MemoryCopy(data.PData, values, destinationLength, bytesToCopy);

            DeviceContext.Unmap(resource, 0);
        }

        public void SetComputePipeline(IComputePipeline? pipeline)
        {
            if (pipeline == null)
            {
                D3D11GraphicsPipeline.EndDraw(DeviceContext);
                return;
            }
            ((D3D11ComputePipeline)pipeline).SetComputePipeline(DeviceContext);
        }

        public void SetGraphicsPipeline(IGraphicsPipeline? pipeline)
        {
            if (pipeline == null)
            {
                D3D11GraphicsPipeline.EndDraw(DeviceContext);
                return;
            }
            ((D3D11GraphicsPipeline)pipeline).SetGraphicsPipeline(DeviceContext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetIndexBuffer(IBuffer? indexBuffer, Format format, int offset)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)indexBuffer?.NativePointer;
#nullable enable
            DeviceContext.IASetIndexBuffer(buffer, Helper.Convert(format), (uint)offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPrimitiveTopology(PrimitiveTopology topology)
        {
            DeviceContext.IASetPrimitiveTopology(Helper.Convert(topology));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRenderTarget(IRenderTargetView? renderTargetView, IDepthStencilView? depthStencilView)
        {
#nullable disable
            ID3D11RenderTargetView* rtv = (ID3D11RenderTargetView*)(renderTargetView?.NativePointer);
            ID3D11DepthStencilView* dsv = (ID3D11DepthStencilView*)(depthStencilView?.NativePointer);
#nullable enable
            DeviceContext.OMSetRenderTargets(1, &rtv, dsv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetRenderTargets(uint count, void** views, IDepthStencilView? depthStencilView)
        {
#nullable disable
            ID3D11RenderTargetView** ptr = (ID3D11RenderTargetView**)views;
            ID3D11DepthStencilView* dsv = (ID3D11DepthStencilView*)(depthStencilView?.NativePointer);
#nullable enable
            DeviceContext.OMSetRenderTargets(count, ptr, dsv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRenderTargetsAndUnorderedAccessViews(IRenderTargetView? renderTargetView, IDepthStencilView? depthStencilView, uint uavSlot, IUnorderedAccessView? unorderedAccessView, uint uavInitialCount = unchecked((uint)-1))
        {
#nullable disable
            ID3D11RenderTargetView* rtv = (ID3D11RenderTargetView*)(renderTargetView?.NativePointer);
            ID3D11DepthStencilView* dsv = (ID3D11DepthStencilView*)(depthStencilView?.NativePointer);
            ID3D11UnorderedAccessView* uav = (ID3D11UnorderedAccessView*)(unorderedAccessView?.NativePointer);
#nullable enable
            DeviceContext.OMSetRenderTargetsAndUnorderedAccessViews(1, &rtv, dsv, uavSlot, 1, &uav, uavInitialCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetRenderTargetsAndUnorderedAccessViews(uint count, void** views, IDepthStencilView? depthStencilView, uint uavSlot, uint uavCount, void** uavs, uint* pUavInitialCount)
        {
#nullable disable
            ID3D11RenderTargetView** ptr = (ID3D11RenderTargetView**)views;
            ID3D11DepthStencilView* dsv = (ID3D11DepthStencilView*)(depthStencilView?.NativePointer);
            ID3D11UnorderedAccessView** uavss = (ID3D11UnorderedAccessView**)uavs;
#nullable enable
            DeviceContext.OMSetRenderTargetsAndUnorderedAccessViews(count, ptr, dsv, uavSlot, uavCount, uavss, pUavInitialCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetScissorRect(int x, int y, int z, int w)
        {
            Box2D<int> rect = new(x, y, z, w);
            DeviceContext.RSSetScissorRects(1, &rect);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertexBuffer(IBuffer? vertexBuffer, uint stride)
        {
            uint ustride = stride;
            uint uoffset = 0;
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)vertexBuffer?.NativePointer;
#nullable enable
            DeviceContext.IASetVertexBuffers(0, 1, &buffer, &ustride, &uoffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertexBuffer(IBuffer? vertexBuffer, uint stride, uint offset)
        {
            uint ustride = stride;
            uint uoffset = offset;
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)vertexBuffer?.NativePointer;
#nullable enable
            DeviceContext.IASetVertexBuffers(0, 1, &buffer, &ustride, &uoffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertexBuffer(uint slot, IBuffer? vertexBuffer, uint stride)
        {
            uint uslot = slot;
            uint ustride = stride;
            uint uoffset = 0;
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)vertexBuffer?.NativePointer;
#nullable enable
            DeviceContext.IASetVertexBuffers(uslot, 1, &buffer, &ustride, &uoffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertexBuffer(uint slot, IBuffer? vertexBuffer, uint stride, uint offset)
        {
            uint uslot = slot;
            uint ustride = stride;
            uint uoffset = offset;
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)vertexBuffer?.NativePointer;
#nullable enable
            DeviceContext.IASetVertexBuffers(uslot, 1, &buffer, &ustride, &uoffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetViewport(Viewport viewport)
        {
            var vp = Helper.Convert(viewport);
            DeviceContext.RSSetViewports(1, &vp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetViewports(uint count, Viewport* viewports)
        {
            Silk.NET.Direct3D11.Viewport* vps = stackalloc Silk.NET.Direct3D11.Viewport[(int)count];
            Helper.Convert(viewports, vps, count);
            DeviceContext.RSSetViewports(count, vps);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unmap(IResource resource, int subresourceIndex)
        {
            DeviceContext.Unmap((ID3D11Resource*)resource.NativePointer, (uint)subresourceIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateSubresource(IResource resource, int destSubresource, MappedSubresource subresource)
        {
            DeviceContext.UpdateSubresource((ID3D11Resource*)resource.NativePointer, (uint)destSubresource, (Box*)null, subresource.PData, subresource.RowPitch, subresource.DepthPitch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetConstantBuffer(uint slot, IBuffer? constantBuffer)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer?.NativePointer;
#nullable enable
            DeviceContext.VSSetConstantBuffers(slot, 1, &buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetConstantBuffers(uint slot, uint count, void** constantBuffers)
        {
            DeviceContext.VSSetConstantBuffers(slot, count, (ID3D11Buffer**)constantBuffers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetSampler(uint slot, ISamplerState? sampler)
        {
#nullable disable
            ID3D11SamplerState* smp = (ID3D11SamplerState*)sampler?.NativePointer;
#nullable enable
            DeviceContext.VSSetSamplers(slot, 1, &smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetSamplers(uint slot, uint count, void** samplers)
        {
            DeviceContext.VSSetSamplers(slot, count, (ID3D11SamplerState**)samplers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetShaderResource(uint slot, IShaderResourceView? shaderResourceView)
        {
#nullable disable
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView?.NativePointer;
#nullable enable
            DeviceContext.VSSetShaderResources(slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetShaderResources(uint slot, uint count, void** shaderResourceViews)
        {
            DeviceContext.VSSetShaderResources(slot, count, (ID3D11ShaderResourceView**)shaderResourceViews);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IBuffer buffer, void* value, int size)
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Silk.NET.Direct3D11.Map.WriteDiscard, 0, &data).ThrowHResult();
            Buffer.MemoryCopy(value, data.PData, data.RowPitch, size);
            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IBuffer buffer, void* value, int size, MapMode flags)
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, (Map)Helper.Convert((Core.Graphics.MapMode)flags), 0, &data).ThrowHResult();
            Buffer.MemoryCopy(value, data.PData, data.RowPitch, size);
            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IBuffer buffer, T value) where T : unmanaged
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Silk.NET.Direct3D11.Map.WriteDiscard, 0, &data).ThrowHResult();

            Buffer.MemoryCopy(&value, data.PData, data.RowPitch, sizeof(T));

            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IBuffer buffer, T* value, int size) where T : unmanaged
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Silk.NET.Direct3D11.Map.WriteDiscard, 0, &data).ThrowHResult();
            Buffer.MemoryCopy(value, data.PData, data.RowPitch, size);
            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IBuffer buffer, T* value, int size, MapMode flags) where T : unmanaged
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, (Map)Helper.Convert((Core.Graphics.MapMode)flags), 0, &data).ThrowHResult();
            Buffer.MemoryCopy(value, data.PData, data.RowPitch, size);
            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IBuffer buffer, T[] values) where T : struct
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Silk.NET.Direct3D11.Map.WriteDiscard, 0, &data);
            var size = Marshal.SizeOf<T>();
            var basePtr = Marshal.AllocHGlobal(size * values.Length);
            var ptr = basePtr.ToInt64();
            for (int i = 0; i < values.Length; i++)
            {
                Marshal.StructureToPtr(values[i], (IntPtr)ptr, true);
                ptr += size;
            }
            Buffer.MemoryCopy((void*)basePtr, data.PData, data.RowPitch, size * values.Length);
            Marshal.FreeHGlobal(basePtr);
            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IBuffer buffer, T[] values, int structSize) where T : unmanaged
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Silk.NET.Direct3D11.Map.WriteDiscard, 0, &data);

            fixed (void* dataPtr = values)
            {
                Buffer.MemoryCopy(dataPtr, data.PData, data.RowPitch, structSize * values.Length);
            }

            DeviceContext.Unmap(resource, 0);
        }

        #endregion IGraphicsContext

        #region IGraphicsContext1

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetShaderResource(uint slot, ICombinedTex2D? tex)
        {
            var srv = ((D3D11CombinedTex2D?)tex)?.srv ?? null;
            DeviceContext.VSSetShaderResources(slot, 1, &srv.Handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetShaderResources(uint slot, IList<ICombinedTex2D?> tex)
        {
            var count = (uint)tex.Count;
            ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.srv.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            DeviceContext.VSSetShaderResources(slot, count, srvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetShaderResources(uint slot, Span<ICombinedTex2D?> tex)
        {
            var count = (uint)tex.Length;
            ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.srv.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            DeviceContext.VSSetShaderResources(slot, count, srvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetShaderResource(uint slot, ICombinedTex2D? tex)
        {
            var srv = ((D3D11CombinedTex2D?)tex)?.srv ?? null;
            DeviceContext.HSSetShaderResources(slot, 1, &srv.Handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetShaderResources(uint slot, IList<ICombinedTex2D?> tex)
        {
            var count = (uint)tex.Count;
            ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.srv.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            DeviceContext.HSSetShaderResources(slot, count, srvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetShaderResources(uint slot, Span<ICombinedTex2D?> tex)
        {
            var count = (uint)tex.Length;
            ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.srv.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            DeviceContext.HSSetShaderResources(slot, count, srvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetShaderResource(uint slot, ICombinedTex2D? tex)
        {
            var srv = ((D3D11CombinedTex2D?)tex)?.srv ?? null;
            DeviceContext.DSSetShaderResources(slot, 1, &srv.Handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetShaderResources(uint slot, IList<ICombinedTex2D?> tex)
        {
            var count = (uint)tex.Count;
            ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.srv.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            DeviceContext.DSSetShaderResources(slot, count, srvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetShaderResources(uint slot, Span<ICombinedTex2D?> tex)
        {
            var count = (uint)tex.Length;
            ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.srv.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            DeviceContext.DSSetShaderResources(slot, count, srvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetShaderResource(uint slot, ICombinedTex2D? tex)
        {
            var srv = ((D3D11CombinedTex2D?)tex)?.srv ?? null;
            DeviceContext.GSSetShaderResources(slot, 1, &srv.Handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetShaderResources(uint slot, IList<ICombinedTex2D?> tex)
        {
            var count = (uint)tex.Count;
            ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.srv.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            DeviceContext.GSSetShaderResources(slot, count, srvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetShaderResources(uint slot, Span<ICombinedTex2D?> tex)
        {
            var count = (uint)tex.Length;
            ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.srv.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            DeviceContext.GSSetShaderResources(slot, count, srvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetShaderResource(uint slot, ICombinedTex2D? tex)
        {
            var srv = ((D3D11CombinedTex2D?)tex)?.srv ?? null;
            DeviceContext.PSSetShaderResources(slot, 1, &srv.Handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetShaderResources(uint slot, IList<ICombinedTex2D?> tex)
        {
            var count = (uint)tex.Count;
            ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.srv.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            DeviceContext.PSSetShaderResources(slot, count, srvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetShaderResources(uint slot, Span<ICombinedTex2D?> tex)
        {
            var count = (uint)tex.Length;
            ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.srv.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            DeviceContext.PSSetShaderResources(slot, count, srvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetShaderResource(uint slot, ICombinedTex2D? tex)
        {
            var srv = ((D3D11CombinedTex2D?)tex)?.srv ?? null;
            DeviceContext.CSSetShaderResources(slot, 1, srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetShaderResources(uint slot, IList<ICombinedTex2D?> tex)
        {
            var count = (uint)tex.Count;
            ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.srv.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            DeviceContext.CSSetShaderResources(slot, count, srvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetShaderResources(uint slot, Span<ICombinedTex2D?> tex)
        {
            var count = (uint)tex.Length;
            ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.srv.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            DeviceContext.CSSetShaderResources(slot, count, srvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRenderTarget(ICombinedTex2D? view, ICombinedTex2D? depthStencilView)
        {
            ID3D11RenderTargetView* rtv = null;
            if (view != null)
            {
                rtv = ((D3D11CombinedTex2D)view).rtv;
            }

            ID3D11DepthStencilView* dsv = null;
            if (depthStencilView != null)
            {
                dsv = ((D3D11CombinedTex2D)depthStencilView).dsv;
            }

            DeviceContext.OMSetRenderTargets(1, &rtv, dsv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRenderTargets(IList<ICombinedTex2D?> views, ICombinedTex2D? depthStencilView)
        {
            uint count = (uint)views.Count;
            ID3D11RenderTargetView** rtvs = stackalloc ID3D11RenderTargetView*[views.Count];
            for (int i = 0; i < count; i++)
            {
                if (views[i] is D3D11CombinedTex2D texture)
                {
                    rtvs[i] = texture.rtv.Handle;
                }
                else
                {
                    rtvs[i] = null;
                }
            }

            ID3D11DepthStencilView* dsv = null;
            if (depthStencilView != null)
            {
                dsv = ((D3D11CombinedTex2D)depthStencilView).dsv;
            }

            DeviceContext.OMSetRenderTargets(count, rtvs, dsv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRenderTargets(Span<ICombinedTex2D?> views, ICombinedTex2D? depthStencilView)
        {
            uint count = (uint)views.Length;
            ID3D11RenderTargetView** rtvs = stackalloc ID3D11RenderTargetView*[views.Length];
            for (int i = 0; i < count; i++)
            {
                if (views[i] is D3D11CombinedTex2D texture)
                {
                    rtvs[i] = texture.rtv.Handle;
                }
                else
                {
                    rtvs[i] = null;
                }
            }

            ID3D11DepthStencilView* dsv = null;
            if (depthStencilView != null)
            {
                dsv = ((D3D11CombinedTex2D)depthStencilView).dsv;
            }

            DeviceContext.OMSetRenderTargets(count, rtvs, dsv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRenderTargetsAndUnorderedAccessViews(ICombinedTex2D? renderTargetView, ICombinedTex2D? depthStencilView, uint uavSlot, ICombinedTex2D? unorderedAccessView, uint uavInitialCount = unchecked((uint)-1))
        {
            ID3D11RenderTargetView* rtv = null;
            if (renderTargetView != null)
            {
                rtv = ((D3D11CombinedTex2D)renderTargetView).rtv;
            }
            ID3D11DepthStencilView* dsv = null;
            if (depthStencilView != null)
            {
                dsv = ((D3D11CombinedTex2D)depthStencilView).dsv;
            }
            ID3D11UnorderedAccessView* uav = null;
            if (unorderedAccessView != null)
            {
                uav = ((D3D11CombinedTex2D)unorderedAccessView).uav;
            }

            DeviceContext.OMSetRenderTargetsAndUnorderedAccessViews(1, &rtv, dsv, uavSlot, 1, &uav, uavInitialCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetRenderTargetsAndUnorderedAccessViews(IList<ICombinedTex2D?> views, ICombinedTex2D? depthStencilView, uint uavSlot, IList<ICombinedTex2D?> unorderedAccessViews, IList<uint> uavInitialCounts)
        {
            uint rtvCount = (uint)views.Count;
            ID3D11RenderTargetView** rtvs = stackalloc ID3D11RenderTargetView*[views.Count];
            for (int i = 0; i < rtvCount; i++)
            {
                if (views[i] is D3D11CombinedTex2D texture)
                {
                    rtvs[i] = texture.rtv.Handle;
                }
                else
                {
                    rtvs[i] = null;
                }
            }

            ID3D11DepthStencilView* dsv = null;
            if (depthStencilView != null)
            {
                dsv = ((D3D11CombinedTex2D)depthStencilView).dsv;
            }

            var uavCount = (uint)unorderedAccessViews.Count;
            ID3D11UnorderedAccessView** uavs = stackalloc ID3D11UnorderedAccessView*[(int)uavCount];
            for (int i = 0; i < uavCount; i++)
            {
                if (unorderedAccessViews[i] is D3D11CombinedTex2D texture)
                {
                    uavs[i] = texture.uav.Handle;
                }
                else
                {
                    uavs[i] = null;
                }
            }

            uint countInit = (uint)uavInitialCounts.Count;
            uint* pUavInitialCounts = stackalloc uint[uavInitialCounts.Count];
            for (int i = 0; i < countInit; i++)
            {
                pUavInitialCounts[i] = uavInitialCounts[i];
            }

            DeviceContext.OMSetRenderTargetsAndUnorderedAccessViews(rtvCount, rtvs, dsv, uavSlot, uavCount, uavs, pUavInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetRenderTargetsAndUnorderedAccessViews(Span<ICombinedTex2D?> views, ICombinedTex2D? depthStencilView, uint uavSlot, Span<ICombinedTex2D?> unorderedAccessViews, Span<uint> uavInitialCounts)
        {
            uint rtvCount = (uint)views.Length;
            ID3D11RenderTargetView** rtvs = stackalloc ID3D11RenderTargetView*[views.Length];
            for (int i = 0; i < rtvCount; i++)
            {
                if (views[i] is D3D11CombinedTex2D texture)
                {
                    rtvs[i] = texture.rtv.Handle;
                }
                else
                {
                    rtvs[i] = null;
                }
            }

            ID3D11DepthStencilView* dsv = null;
            if (depthStencilView != null)
            {
                dsv = ((D3D11CombinedTex2D)depthStencilView).dsv;
            }

            var uavCount = (uint)unorderedAccessViews.Length;
            ID3D11UnorderedAccessView** uavs = stackalloc ID3D11UnorderedAccessView*[(int)uavCount];
            for (int i = 0; i < uavCount; i++)
            {
                if (unorderedAccessViews[i] is D3D11CombinedTex2D texture)
                {
                    uavs[i] = texture.uav.Handle;
                }
                else
                {
                    uavs[i] = null;
                }
            }

            uint countInit = (uint)uavInitialCounts.Length;
            uint* pUavInitialCounts = stackalloc uint[uavInitialCounts.Length];
            for (int i = 0; i < countInit; i++)
            {
                pUavInitialCounts[i] = uavInitialCounts[i];
            }

            DeviceContext.OMSetRenderTargetsAndUnorderedAccessViews(rtvCount, rtvs, dsv, uavSlot, uavCount, uavs, pUavInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessView(uint slot, ICombinedTex2D? tex, uint uavInitialCount = unchecked((uint)-1))
        {
            var uav = ((D3D11CombinedTex2D?)tex)?.uav ?? null;
            DeviceContext.CSSetUnorderedAccessViews(slot, 1, uav, &uavInitialCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessViews(uint slot, IList<ICombinedTex2D?> tex, IList<uint> uavInitialCounts)
        {
            var count = (uint)tex.Count;
            ID3D11UnorderedAccessView** srvs = stackalloc ID3D11UnorderedAccessView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.uav.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            uint countInit = (uint)uavInitialCounts.Count;
            uint* pUavInitialCounts = stackalloc uint[uavInitialCounts.Count];
            for (int i = 0; i < countInit; i++)
            {
                pUavInitialCounts[i] = uavInitialCounts[i];
            }

            DeviceContext.CSSetUnorderedAccessViews(slot, count, srvs, pUavInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessViews(uint slot, Span<ICombinedTex2D?> tex, Span<uint> uavInitialCounts)
        {
            var count = (uint)tex.Length;
            ID3D11UnorderedAccessView** srvs = stackalloc ID3D11UnorderedAccessView*[(int)count];
            for (int i = 0; i < count; i++)
            {
                if (tex[i] is D3D11CombinedTex2D texture)
                {
                    srvs[i] = texture.uav.Handle;
                }
                else
                {
                    srvs[i] = null;
                }
            }

            uint countInit = (uint)uavInitialCounts.Length;
            uint* pUavInitialCounts = stackalloc uint[uavInitialCounts.Length];
            for (int i = 0; i < countInit; i++)
            {
                pUavInitialCounts[i] = uavInitialCounts[i];
            }

            DeviceContext.CSSetUnorderedAccessViews(slot, count, srvs, pUavInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearDepthStencilView(ICombinedTex2D depthStencilView, DepthStencilClearFlags flags, float depth, byte stencil)
        {
            DeviceContext.ClearDepthStencilView(((D3D11CombinedTex2D)depthStencilView).dsv, (uint)Helper.Convert(flags), depth, stencil);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearRenderTargetView(ICombinedTex2D renderTargetView, Vector4 value)
        {
            DeviceContext.ClearRenderTargetView(((D3D11CombinedTex2D)renderTargetView).rtv, (float*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearUnorderedAccessViewFloat(ICombinedTex2D uav, float r, float g, float b, float a)
        {
            float* values = stackalloc float[4] { r, g, b, a };
            DeviceContext.ClearUnorderedAccessViewFloat(((D3D11CombinedTex2D)uav).uav, values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearUnorderedAccessViewUint(ICombinedTex2D uav, uint r, uint g, uint b, uint a)
        {
            uint* values = stackalloc uint[4] { r, g, b, a };
            DeviceContext.ClearUnorderedAccessViewUint(((D3D11CombinedTex2D)uav).uav, values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearView(ICombinedTex2D tex, Vector4 color, Rect rect)
        {
            var rec = Helper.Convert(rect);

            var texture = (D3D11CombinedTex2D)tex;
            if (texture.IsRTV)
            {
                DeviceContext.ClearView((ID3D11View*)texture.rtv.Handle, (float*)&color, &rec, 1);
            }

            if (texture.IsUAV)
            {
                DeviceContext.ClearView((ID3D11View*)texture.uav.Handle, (float*)&color, &rec, 1);
            }

            if (texture.IsDSV)
            {
                DeviceContext.ClearView((ID3D11View*)texture.dsv.Handle, (float*)&color, &rec, 1);
            }
        }

        #endregion IGraphicsContext1

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                OnDisposed?.Invoke(this, EventArgs.Empty);
                DeviceContext.ClearState();
                DeviceContext.Flush();
                DeviceContext.Release();
                disposedValue = true;
            }
        }
    }
}