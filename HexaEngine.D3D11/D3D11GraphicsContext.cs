namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using Silk.NET.Maths;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Format = Core.Graphics.Format;
    using Map = Core.Graphics.Map;
    using MappedSubresource = Core.Graphics.MappedSubresource;
    using Viewport = Mathematics.Viewport;

    public unsafe class D3D11GraphicsContext : IGraphicsContext
    {
        private bool disposedValue;

        internal ComPtr<ID3D11DeviceContext3> DeviceContext;

        internal D3D11GraphicsContext(D3D11GraphicsDevice device)
        {
            DeviceContext = device.DeviceContext;
            NativePointer = new(device.DeviceContext);
            Device = device;
        }

        internal D3D11GraphicsContext(D3D11GraphicsDevice device, ComPtr<ID3D11DeviceContext3> context)
        {
            DeviceContext = context;
            NativePointer = new(context);
            Device = device;
        }

        public IGraphicsDevice Device { get; }

        public IntPtr NativePointer { get; }

        public bool IsDisposed => disposedValue;

        public string? DebugName { get; set; } = string.Empty;

        public event EventHandler? OnDisposed;

        public void SetGraphicsPipeline(IGraphicsPipeline? pipeline)
        {
            if (pipeline == null)
            {
                D3D11GraphicsPipeline.EndDraw(DeviceContext);
                return;
            }
            ((D3D11GraphicsPipeline)pipeline).SetGraphicsPipeline(DeviceContext);
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

        public void ClearView(IRenderTargetView rtv, Vector4 color, Rect rect)
        {
            var rec = Helper.Convert(rect);
            DeviceContext.ClearView((ID3D11View*)rtv.NativePointer, (float*)&color, &rec, 1);
        }

        public void ClearView(IDepthStencilView dsv, Vector4 color, Rect rect)
        {
            var rec = Helper.Convert(rect);
            DeviceContext.ClearView((ID3D11View*)dsv.NativePointer, (float*)&color, &rec, 1);
        }

        public void ClearView(IUnorderedAccessView uav, Vector4 color, Rect rect)
        {
            var rec = Helper.Convert(rect);
            DeviceContext.ClearView((ID3D11View*)uav.NativePointer, (float*)&color, &rec, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearUnorderedAccessViewUint(IUnorderedAccessView uav, uint r, uint g, uint b, uint a)
        {
            uint* values = stackalloc uint[4] { r, g, b, a };
            DeviceContext.ClearUnorderedAccessViewUint((ID3D11UnorderedAccessView*)uav.NativePointer, values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearUnorderedAccessViewFloat(IUnorderedAccessView uav, float r, float g, float b, float a)
        {
            float* values = stackalloc float[4] { r, g, b, a };
            DeviceContext.ClearUnorderedAccessViewFloat((ID3D11UnorderedAccessView*)uav.NativePointer, values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearState()
        {
            DeviceContext.ClearState();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawInstanced(uint vertexCount, uint instanceCount, uint vertexOffset, uint instanceOffset)
        {
            DeviceContext.DrawInstanced(vertexCount, instanceCount, vertexOffset, instanceOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawIndexedInstanced(uint indexCount, uint instanceCount, uint indexOffset, int vertexOffset, uint instanceOffset)
        {
            DeviceContext.DrawIndexedInstanced(indexCount, instanceCount, indexOffset, vertexOffset, instanceOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawIndexedInstancedIndirect(void* bufferForArgs, uint alignedByteOffsetForArgs)
        {
            DeviceContext.DrawIndexedInstancedIndirect((ID3D11Buffer*)bufferForArgs, alignedByteOffsetForArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawIndexedInstancedIndirect(IBuffer bufferForArgs, uint alignedByteOffsetForArgs)
        {
            DeviceContext.DrawIndexedInstancedIndirect((ID3D11Buffer*)bufferForArgs.NativePointer, alignedByteOffsetForArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawInstancedIndirect(void* bufferForArgs, uint alignedByteOffsetForArgs)
        {
            DeviceContext.DrawInstancedIndirect((ID3D11Buffer*)bufferForArgs, alignedByteOffsetForArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawInstancedIndirect(IBuffer bufferForArgs, uint alignedByteOffsetForArgs)
        {
            DeviceContext.DrawInstancedIndirect((ID3D11Buffer*)bufferForArgs.NativePointer, alignedByteOffsetForArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MappedSubresource Map(IResource resource, int subresourceIndex, MapMode mode, MapFlags flags)
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            DeviceContext.Map((ID3D11Resource*)resource.NativePointer, (uint)subresourceIndex, Helper.Convert(mode), (uint)Helper.Convert(flags), &data);
            return new(data.PData, data.RowPitch, data.DepthPitch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateSubresource(IResource resource, int destSubresource, MappedSubresource subresource)
        {
            DeviceContext.UpdateSubresource((ID3D11Resource*)resource.NativePointer, (uint)destSubresource, (Box*)null, subresource.PData, subresource.RowPitch, subresource.DepthPitch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unmap(IResource resource, int subresourceIndex)
        {
            DeviceContext.Unmap((ID3D11Resource*)resource.NativePointer, (uint)subresourceIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GenerateMips(IShaderResourceView resourceView)
        {
            DeviceContext.GenerateMips((ID3D11ShaderResourceView*)resourceView.NativePointer);
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
        public void Write(IBuffer buffer, void* value, int size)
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Silk.NET.Direct3D11.Map.WriteDiscard, 0, &data).ThrowHResult();
            Buffer.MemoryCopy(value, data.PData, data.RowPitch, size);
            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IBuffer buffer, void* value, int size, Map flags)
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Helper.Convert(flags), 0, &data).ThrowHResult();
            Buffer.MemoryCopy(value, data.PData, data.RowPitch, size);
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
        public void Write<T>(IBuffer buffer, T* value, int size, Map flags) where T : unmanaged
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Helper.Convert(flags), 0, &data).ThrowHResult();
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

        ~D3D11GraphicsContext()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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
        public void Flush()
        {
            DeviceContext.Flush();
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
        public void CSSetUnorderedAccessView(uint offset, void* view, uint uavInitialCount = unchecked((uint)-1))
        {
            uint pUAVInitialCount = uavInitialCount;
            DeviceContext.CSSetUnorderedAccessViews(offset, 1, (ID3D11UnorderedAccessView**)&view, &pUAVInitialCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICommandList FinishCommandList(bool restoreState)
        {
            ID3D11CommandList* commandList;
            DeviceContext.FinishCommandList(restoreState, &commandList);
            return new D3D11CommandList(commandList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteCommandList(ICommandList commandList, bool restoreState)
        {
            DeviceContext.ExecuteCommandList((ID3D11CommandList*)commandList.NativePointer, restoreState);
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

        public void CopyResource(IResource dst, IResource src)
        {
            DeviceContext.CopyResource((ID3D11Resource*)dst.NativePointer, (ID3D11Resource*)src.NativePointer);
        }

        public void CopyStructureCount(IBuffer dst, uint alignedByteOffset, IUnorderedAccessView uav)
        {
            DeviceContext.CopyStructureCount((ID3D11Buffer*)dst.NativePointer, alignedByteOffset, (ID3D11UnorderedAccessView*)uav.NativePointer);
        }
    }
}