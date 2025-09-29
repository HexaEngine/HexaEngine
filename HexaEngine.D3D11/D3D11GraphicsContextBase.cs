namespace HexaEngine.D3D11
{
    using Hexa.NET.D3D11;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaGen.Runtime;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using Format = Core.Graphics.Format;
    using MapMode = Core.Graphics.MapMode;
    using MappedSubresource = Core.Graphics.MappedSubresource;
    using Viewport = Hexa.NET.Mathematics.Viewport;

    public unsafe class D3D11GraphicsContextBase
    {
        internal ComPtr<ID3D11DeviceContext3> DeviceContext;
        private readonly nint nativePointer;
        private bool disposedValue;

        public string? DebugName { get; set; } = string.Empty;

        public IGraphicsDevice Device { get; }

        public bool IsDisposed => disposedValue;

        public nint NativePointer => nativePointer;

        public event EventHandler? OnDisposed;

        protected D3D11GraphicsContextBase(IGraphicsDevice device, ComPtr<ID3D11DeviceContext4> context) : this(device, context.As<ID3D11DeviceContext3>())
        {
        }

        protected D3D11GraphicsContextBase(IGraphicsDevice device, ComPtr<ID3D11DeviceContext3> context)
        {
            Device = device;
            DeviceContext = context;
            nativePointer = (nint)context.Handle;
        }

        #region IGraphicsContext

        public void BeginDraw(BeginDrawDesc desc)
        {
            ID3D11RenderTargetView** rtvs = stackalloc ID3D11RenderTargetView*[BlendDescription.SimultaneousRenderTargetCount];
            ID3D11DepthStencilView* dsv = (ID3D11DepthStencilView*)(desc.DepthStencil?.NativePointer ?? 0);

            for (int i = 0; i < desc.NumViews; i++)
            {
                rtvs[i] = (ID3D11RenderTargetView*)desc.RenderTargets[i].NativePointer;
            }

            ClearFlag flag = (ClearFlag)(((int)desc.ClearFlags & 0b_0110) >> 1);

            if (flag != 0)
            {
                DeviceContext.ClearDepthStencilView(dsv, (uint)flag, desc.ClearDepthStencilValue.Depth, desc.ClearDepthStencilValue.Stencil);
            }

            if ((desc.ClearFlags & ClearFlags.Color) != 0)
            {
                for (int i = 0; i < desc.NumViews; i++)
                {
                    DeviceContext.ClearRenderTargetView(rtvs[i], (float*)&desc.ClearColors[i]);
                }
            }

            DeviceContext.OMSetRenderTargets(desc.NumViews, rtvs, dsv);

            Viewport* viewports = &desc.Viewports.Viewport;
            DeviceContext.RSSetViewports(desc.NumViews, (Hexa.NET.D3D11.Viewport*)viewports);
        }

        public void ExecuteCommandBuffer(ICommandBuffer commandBuffer)
        {
            D3D11CommandBuffer cmdBuf = (D3D11CommandBuffer)commandBuffer;
            DeviceContext.ExecuteCommandList(cmdBuf.commandList, true);
        }

        public void BeginEvent(string name)
        {
            fixed (char* pName = name)
            {
                DeviceContext.BeginEventInt(pName, 0);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResourceBindingList(IResourceBindingList rootDescriptorTable)
        {
            if (currentState is D3D11ComputePipelineState)
            {
                ((D3D11ResourceBindingList)rootDescriptorTable).BindCompute(DeviceContext);
                return;
            }

            ((D3D11ResourceBindingList)rootDescriptorTable).BindGraphics(DeviceContext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsetResourceBindingList(IResourceBindingList rootDescriptorTable)
        {
            ((D3D11ResourceBindingList)rootDescriptorTable).UnbindGraphics(DeviceContext);
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

        public void EndEvent()
        {
            DeviceContext.EndEvent();
        }

        [Obsolete("Use command buffers")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteCommandList(ICommandList commandList, bool restoreState)
        {
            DeviceContext.ExecuteCommandList((ID3D11CommandList*)commandList.NativePointer, restoreState);
        }

        [Obsolete("Use command buffers")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICommandList FinishCommandList(bool restoreState)
        {
            ComPtr<ID3D11CommandList> commandList = default;
            DeviceContext.FinishCommandList((Bool32)restoreState, out commandList);
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
        public MappedSubresource Map(IResource resource, int subresourceIndex, MapMode mode, MapFlags flags)
        {
            Hexa.NET.D3D11.MappedSubresource data;
            DeviceContext.Map((ID3D11Resource*)resource.NativePointer, (uint)subresourceIndex, (Map)Helper.Convert((MapMode)mode), (uint)Helper.Convert(flags), &data);
            return new(data.PData, data.RowPitch, data.DepthPitch);
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
            Hexa.NET.D3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Hexa.NET.D3D11.Map.Read, 0, &data);

            long destinationLength = length;
            long bytesToCopy = data.RowPitch > destinationLength ? destinationLength : data.RowPitch;
            Buffer.MemoryCopy(data.PData, values, destinationLength, bytesToCopy);

            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(IBuffer buffer, T* values, uint count) where T : unmanaged
        {
            Hexa.NET.D3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Hexa.NET.D3D11.Map.Read, 0, &data);

            long destinationLength = count * sizeof(T);
            long bytesToCopy = data.RowPitch > destinationLength ? destinationLength : data.RowPitch;
            Buffer.MemoryCopy(data.PData, values, destinationLength, bytesToCopy);

            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComputePipelineState(IComputePipelineState? state)
        {
            currentState?.UnsetState(DeviceContext);
            currentState = null;
            if (state == null)
            {
                return;
            }

            ((D3D11ComputePipelineState)state).SetState(DeviceContext);
            currentState = (D3D11ComputePipelineState)state;
        }

        private D3D11PipelineState? currentState;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetGraphicsPipelineState(IGraphicsPipelineState? state)
        {
            currentState?.UnsetState(DeviceContext);
            currentState = null;
            if (state == null)
            {
                return;
            }

            ((D3D11GraphicsPipelineState)state).SetState(DeviceContext);
            currentState = (D3D11GraphicsPipelineState)state;
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
            DeviceContext.OMSetRenderTargetsAndUnorderedAccessViews(1u, &rtv, dsv, uavSlot, 1u, &uav, &uavInitialCount);
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
            Rect32 rect = new(x, y, z, w);
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
            Hexa.NET.D3D11.Viewport* vps = stackalloc Hexa.NET.D3D11.Viewport[(int)count];
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
        public void Write(IBuffer buffer, void* value, int size)
        {
            Hexa.NET.D3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Hexa.NET.D3D11.Map.WriteDiscard, 0, &data).ThrowIf();
            Buffer.MemoryCopy(value, data.PData, data.RowPitch, size);
            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IBuffer buffer, void* value, int size, MapMode flags)
        {
            Hexa.NET.D3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, (Map)Helper.Convert((Core.Graphics.MapMode)flags), 0, &data).ThrowIf();
            Buffer.MemoryCopy(value, data.PData, data.RowPitch, size);
            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IBuffer buffer, T value) where T : unmanaged
        {
            Hexa.NET.D3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Hexa.NET.D3D11.Map.WriteDiscard, 0, &data).ThrowIf();

            Buffer.MemoryCopy(&value, data.PData, data.RowPitch, sizeof(T));

            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IBuffer buffer, T* value, int size) where T : unmanaged
        {
            Hexa.NET.D3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, Hexa.NET.D3D11.Map.WriteDiscard, 0, &data).ThrowIf();
            Buffer.MemoryCopy(value, data.PData, size, size);
            DeviceContext.Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IBuffer buffer, T* value, int size, MapMode flags) where T : unmanaged
        {
            Hexa.NET.D3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext.Map(resource, 0, (Map)Helper.Convert((Core.Graphics.MapMode)flags), 0, &data).ThrowIf();
            Buffer.MemoryCopy(value, data.PData, data.RowPitch, size);
            DeviceContext.Unmap(resource, 0);
        }

        #endregion IGraphicsContext

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