﻿namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using Silk.NET.DXGI;
    using Silk.NET.Maths;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Format = Core.Graphics.Format;
    using MappedSubresource = Core.Graphics.MappedSubresource;
    using Viewport = Mathematics.Viewport;

    public unsafe class D3D11GraphicsContext : IGraphicsContext
    {
        private bool disposedValue;

        internal ID3D11DeviceContext1* DeviceContext;

        internal D3D11GraphicsContext(D3D11GraphicsDevice device)
        {
            DeviceContext = device.DeviceContext;
            NativePointer = new(device.DeviceContext);
            Device = device;
        }

        internal D3D11GraphicsContext(D3D11GraphicsDevice device, ID3D11DeviceContext1* context)
        {
            DeviceContext = context;
            NativePointer = (nint)context;
            Device = device;
        }

        public IGraphicsDevice Device { get; }

        public IntPtr NativePointer { get; }

        public bool IsDisposed => disposedValue;

        public string? DebugName { get; set; } = string.Empty;

        public event EventHandler? OnDisposed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearDepthStencilView(IDepthStencilView depthStencilView, DepthStencilClearFlags flags, float depth, byte stencil)
        {
            DeviceContext->ClearDepthStencilView((ID3D11DepthStencilView*)depthStencilView.NativePointer, (uint)Helper.Convert(flags), depth, stencil);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearRenderTargetView(IRenderTargetView renderTargetView, Vector4 value)
        {
            DeviceContext->ClearRenderTargetView((ID3D11RenderTargetView*)renderTargetView.NativePointer, (float*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearRenderTargetViews(IRenderTargetView[] rtvs, Vector4 value)
        {
            for (int i = 0; i < rtvs.Length; i++)
            {
                ClearRenderTargetView(rtvs[i], value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearRenderTargetViews(void** rtvs, uint count, Vector4 value)
        {
            for (int i = 0; i < count; i++)
            {
                DeviceContext->ClearRenderTargetView((ID3D11RenderTargetView*)rtvs[i], (float*)&value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearState()
        {
            DeviceContext->ClearState();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Draw(int vertexCount, int offset)
        {
            DeviceContext->Draw((uint)vertexCount, (uint)offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawIndexed(int indexCount, int indexOffset, int vertexOffset)
        {
            DeviceContext->DrawIndexed((uint)indexCount, (uint)indexOffset, vertexOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawInstanced(int vertexCount, int instanceCount, int vertexOffset, int instanceOffset)
        {
            DeviceContext->DrawInstanced((uint)vertexCount, (uint)instanceCount, (uint)vertexOffset, (uint)instanceOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawIndexedInstanced(int indexCount, int instanceCount, int indexOffset, int vertexOffset, int instanceOffset)
        {
            DeviceContext->DrawIndexedInstanced((uint)indexCount, (uint)instanceCount, (uint)indexOffset, vertexOffset, (uint)instanceOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MappedSubresource Map(IResource resource, int subresourceIndex, MapMode mode, MapFlags flags)
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            DeviceContext->Map((ID3D11Resource*)resource.NativePointer, (uint)subresourceIndex, Helper.Convert(mode), (uint)Helper.Convert(flags), &data);
            return new(data.PData, data.RowPitch, data.DepthPitch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateSubresource(IResource resource, int destSubresource, MappedSubresource subresource)
        {
            DeviceContext->UpdateSubresource((ID3D11Resource*)resource.NativePointer, (uint)destSubresource, null, subresource.PData, subresource.RowPitch, subresource.DepthPitch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unmap(IResource resource, int subresourceIndex)
        {
            DeviceContext->Unmap((ID3D11Resource*)resource.NativePointer, (uint)subresourceIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GenerateMips(IShaderResourceView resourceView)
        {
            DeviceContext->GenerateMips((ID3D11ShaderResourceView*)resourceView.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetConstantBuffer(IBuffer? constantBuffer, ShaderStage stage, int slot)
        {
            throw new NotSupportedException();
            if (constantBuffer != null)
            {
                ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer.NativePointer;
                switch (stage)
                {
                    case ShaderStage.Vertex:
                        DeviceContext->VSSetConstantBuffers((uint)slot, 1, &buffer);
                        break;

                    case ShaderStage.Hull:
                        DeviceContext->HSSetConstantBuffers((uint)slot, 1, &buffer);
                        break;

                    case ShaderStage.Domain:
                        DeviceContext->DSSetConstantBuffers((uint)slot, 1, &buffer);
                        break;

                    case ShaderStage.Geometry:
                        DeviceContext->GSSetConstantBuffers((uint)slot, 1, &buffer);
                        break;

                    case ShaderStage.Pixel:
                        DeviceContext->PSSetConstantBuffers((uint)slot, 1, &buffer);
                        break;

                    case ShaderStage.Compute:
                        DeviceContext->CSSetConstantBuffers((uint)slot, 1, &buffer);
                        break;
                }
            }
            else
            {
                ID3D11Buffer* buffer = null;
                switch (stage)
                {
                    case ShaderStage.Vertex:
                        DeviceContext->VSSetConstantBuffers((uint)slot, 1, &buffer);
                        break;

                    case ShaderStage.Hull:
                        DeviceContext->HSSetConstantBuffers((uint)slot, 1, &buffer);
                        break;

                    case ShaderStage.Domain:
                        DeviceContext->DSSetConstantBuffers((uint)slot, 1, &buffer);
                        break;

                    case ShaderStage.Geometry:
                        DeviceContext->GSSetConstantBuffers((uint)slot, 1, &buffer);
                        break;

                    case ShaderStage.Pixel:
                        DeviceContext->PSSetConstantBuffers((uint)slot, 1, &buffer);
                        break;

                    case ShaderStage.Compute:
                        DeviceContext->CSSetConstantBuffers((uint)slot, 1, &buffer);
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetConstantBuffer(IBuffer? constantBuffer, ShaderStage stage, int slot, uint firstConstant, uint constantCount)
        {
            throw new NotSupportedException();
            uint* firstConstantPtr = &firstConstant;
            uint* constantCountPtr = &constantCount;
            if (constantBuffer != null)
            {
                ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer.NativePointer;
                switch (stage)
                {
                    case ShaderStage.Vertex:
                        DeviceContext->VSSetConstantBuffers1((uint)slot, 1, &buffer, &firstConstant, &constantCount);
                        break;

                    case ShaderStage.Hull:
                        DeviceContext->HSSetConstantBuffers1((uint)slot, 1, &buffer, &firstConstant, &constantCount);
                        break;

                    case ShaderStage.Domain:
                        DeviceContext->DSSetConstantBuffers1((uint)slot, 1, &buffer, &firstConstant, &constantCount);
                        break;

                    case ShaderStage.Geometry:
                        DeviceContext->GSSetConstantBuffers1((uint)slot, 1, &buffer, &firstConstant, &constantCount);
                        break;

                    case ShaderStage.Pixel:
                        DeviceContext->PSSetConstantBuffers1((uint)slot, 1, &buffer, &firstConstant, &constantCount);
                        break;

                    case ShaderStage.Compute:
                        DeviceContext->CSSetConstantBuffers1((uint)slot, 1, &buffer, &firstConstant, &constantCount);
                        break;
                }
            }
            else
                switch (stage)
                {
                    case ShaderStage.Vertex:
                        DeviceContext->VSSetConstantBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)null));
                        break;

                    case ShaderStage.Hull:
                        DeviceContext->HSSetConstantBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)null));
                        break;

                    case ShaderStage.Domain:
                        DeviceContext->DSSetConstantBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)null));
                        break;

                    case ShaderStage.Geometry:
                        DeviceContext->GSSetConstantBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)null));
                        break;

                    case ShaderStage.Pixel:
                        DeviceContext->PSSetConstantBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)null));
                        break;

                    case ShaderStage.Compute:
                        DeviceContext->CSSetConstantBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)null));
                        break;
                }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetConstantBuffers(IBuffer[] constantBuffers, ShaderStage stage, int slot)
        {
            throw new NotSupportedException();
            ID3D11Buffer** ptr = Utils.ToPointerArray<IBuffer, ID3D11Buffer>(constantBuffers);
            uint count = (uint)constantBuffers.Length;
            if (count == 0) return;
            switch (stage)
            {
                case ShaderStage.Vertex:
                    DeviceContext->VSSetConstantBuffers((uint)slot, count, ptr);
                    break;

                case ShaderStage.Hull:
                    DeviceContext->HSSetConstantBuffers((uint)slot, count, ptr);
                    break;

                case ShaderStage.Domain:
                    DeviceContext->DSSetConstantBuffers((uint)slot, count, ptr);
                    break;

                case ShaderStage.Geometry:
                    DeviceContext->GSSetConstantBuffers((uint)slot, count, ptr);
                    break;

                case ShaderStage.Pixel:
                    DeviceContext->PSSetConstantBuffers((uint)slot, count, ptr);
                    break;

                case ShaderStage.Compute:
                    DeviceContext->CSSetConstantBuffers((uint)slot, count, ptr);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetConstantBuffers(IBuffer[] constantBuffers, ShaderStage stage, int slot, uint firstConstant, uint constantCount)
        {
            throw new NotSupportedException();
            ID3D11Buffer** ptr = Utils.ToPointerArray<IBuffer, ID3D11Buffer>(constantBuffers);
            uint count = (uint)constantBuffers.Length;
            if (count == 0) return;
            switch (stage)
            {
                case ShaderStage.Vertex:
                    DeviceContext->VSSetConstantBuffers1((uint)slot, count, ptr, &firstConstant, &constantCount);
                    break;

                case ShaderStage.Hull:
                    DeviceContext->HSSetConstantBuffers1((uint)slot, count, ptr, &firstConstant, &constantCount);
                    break;

                case ShaderStage.Domain:
                    DeviceContext->DSSetConstantBuffers1((uint)slot, count, ptr, &firstConstant, &constantCount);
                    break;

                case ShaderStage.Geometry:
                    DeviceContext->GSSetConstantBuffers1((uint)slot, count, ptr, &firstConstant, &constantCount);
                    break;

                case ShaderStage.Pixel:
                    DeviceContext->PSSetConstantBuffers1((uint)slot, count, ptr, &firstConstant, &constantCount);
                    break;

                case ShaderStage.Compute:
                    DeviceContext->CSSetConstantBuffers1((uint)slot, count, ptr, &firstConstant, &constantCount);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetIndexBuffer(IBuffer? indexBuffer, Format format, int offset)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)indexBuffer?.NativePointer;
#nullable enable
            DeviceContext->IASetIndexBuffer(buffer, Helper.Convert(format), (uint)offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetInputLayout(IInputLayout? inputLayout)
        {
#nullable disable
            ID3D11InputLayout* layout = (ID3D11InputLayout*)inputLayout?.NativePointer;
#nullable enable
            DeviceContext->IASetInputLayout(layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPrimitiveTopology(PrimitiveTopology topology)
        {
            DeviceContext->IASetPrimitiveTopology(Helper.Convert(topology));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRenderTarget(IRenderTargetView? renderTargetView, IDepthStencilView? depthStencilView)
        {
#nullable disable
            ID3D11RenderTargetView* rtv = (ID3D11RenderTargetView*)(renderTargetView?.NativePointer);
            ID3D11DepthStencilView* dsv = (ID3D11DepthStencilView*)(depthStencilView?.NativePointer);
#nullable enable
            DeviceContext->OMSetRenderTargets(1, &rtv, dsv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRenderTargets(IRenderTargetView[] views, IDepthStencilView? depthStencilView)
        {
#nullable disable
            ID3D11RenderTargetView** ptr = Utils.ToPointerArray<IRenderTargetView, ID3D11RenderTargetView>(views);
            ID3D11DepthStencilView* dsv = (ID3D11DepthStencilView*)(depthStencilView?.NativePointer);
#nullable enable
            DeviceContext->OMSetRenderTargets((uint)views.Length, ptr, dsv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetRenderTargets(void** views, uint count, IDepthStencilView? depthStencilView)
        {
#nullable disable
            ID3D11RenderTargetView** ptr = (ID3D11RenderTargetView**)views;
            ID3D11DepthStencilView* dsv = (ID3D11DepthStencilView*)(depthStencilView?.NativePointer);
#nullable enable
            DeviceContext->OMSetRenderTargets(count, ptr, dsv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetScissorRect(int x, int y, int z, int w)
        {
            Rectangle<int> rect = new(x, y, z, w);
            DeviceContext->RSSetScissorRects(1, &rect);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRasterizerState(IRasterizerState? rasterizerState)
        {
#nullable disable
            ID3D11RasterizerState* rs = (ID3D11RasterizerState*)(rasterizerState?.NativePointer);
#nullable enable
            DeviceContext->RSSetState(rs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetDepthStencilState(IDepthStencilState? depthStencilState, int stencilRef = 0)
        {
#nullable disable
            ID3D11DepthStencilState* ds = (ID3D11DepthStencilState*)(depthStencilState?.NativePointer);
#nullable enable
            DeviceContext->OMSetDepthStencilState(ds, (uint)stencilRef);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlendState(IBlendState? blendState, Vector4? factor, uint sampleMask = uint.MaxValue)
        {
#nullable disable
            ID3D11BlendState* state = (ID3D11BlendState*)blendState?.NativePointer;
#nullable enable
            float* fac = (float*)&factor;

            DeviceContext->OMSetBlendState(state, fac, sampleMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertexBuffer(IBuffer? vertexBuffer, int stride, int offset)
        {
            uint ustride = (uint)stride;
            uint uoffset = (uint)offset;
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)vertexBuffer?.NativePointer;
#nullable enable
            DeviceContext->IASetVertexBuffers(0, 1, &buffer, &ustride, &uoffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertexBuffer(IBuffer? vertexBuffer, int stride)
        {
            uint ustride = (uint)stride;
            uint uoffset = 0;
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)vertexBuffer?.NativePointer;
#nullable enable
            DeviceContext->IASetVertexBuffers(0, 1, &buffer, &ustride, &uoffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertexBuffer(int slot, IBuffer? vertexBuffer, int stride, int offset)
        {
            uint uslot = (uint)slot;
            uint ustride = (uint)stride;
            uint uoffset = (uint)offset;
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)vertexBuffer?.NativePointer;
#nullable enable
            DeviceContext->IASetVertexBuffers(uslot, 1, &buffer, &ustride, &uoffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertexBuffer(int slot, IBuffer? vertexBuffer, int stride)
        {
            uint uslot = (uint)slot;
            uint ustride = (uint)stride;
            uint uoffset = 0;
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)vertexBuffer?.NativePointer;
#nullable enable
            DeviceContext->IASetVertexBuffers(uslot, 1, &buffer, &ustride, &uoffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetViewport(Viewport viewport)
        {
            var vp = Helper.Convert(viewport);
            DeviceContext->RSSetViewports(1, &vp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetShader(IVertexShader? vertexShader)
        {
#nullable disable
            ID3D11VertexShader* s = (ID3D11VertexShader*)vertexShader?.NativePointer;
#nullable enable
            DeviceContext->VSSetShader(s, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetShader(IHullShader? hullShader)
        {
#nullable disable
            ID3D11HullShader* s = (ID3D11HullShader*)hullShader?.NativePointer;
#nullable enable
            DeviceContext->HSSetShader(s, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetShader(IDomainShader? domainShader)
        {
#nullable disable
            ID3D11DomainShader* s = (ID3D11DomainShader*)domainShader?.NativePointer;
#nullable enable
            DeviceContext->DSSetShader(s, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetShader(IGeometryShader? geometryShader)
        {
#nullable disable
            ID3D11GeometryShader* s = (ID3D11GeometryShader*)geometryShader?.NativePointer;
#nullable enable
            DeviceContext->GSSetShader(s, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetShader(IPixelShader? pixelShader)
        {
#nullable disable
            ID3D11PixelShader* s = (ID3D11PixelShader*)pixelShader?.NativePointer;
#nullable enable
            DeviceContext->PSSetShader(s, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetShader(IComputeShader? computeShader)
        {
#nullable disable
            ID3D11ComputeShader* s = (ID3D11ComputeShader*)computeShader?.NativePointer;
#nullable enable
            DeviceContext->CSSetShader(s, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IBuffer buffer, void* value, int size)
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext->Map(resource, 0, Silk.NET.Direct3D11.Map.WriteDiscard, 0, &data).ThrowHResult();
            Buffer.MemoryCopy(value, data.PData, data.RowPitch, size);
            DeviceContext->Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IBuffer buffer, T* value, int size) where T : unmanaged
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext->Map(resource, 0, Silk.NET.Direct3D11.Map.WriteDiscard, 0, &data).ThrowHResult();
            Buffer.MemoryCopy(value, data.PData, data.RowPitch, size);
            DeviceContext->Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IBuffer buffer, T value) where T : struct
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext->Map(resource, 0, Silk.NET.Direct3D11.Map.WriteDiscard, 0, &data).ThrowHResult();
            var size = Marshal.SizeOf<T>();
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            Buffer.MemoryCopy((void*)ptr, data.PData, data.RowPitch, size);
            Marshal.FreeHGlobal(ptr);
            DeviceContext->Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IBuffer buffer, T[] values) where T : struct
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext->Map(resource, 0, Silk.NET.Direct3D11.Map.WriteDiscard, 0, &data);
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
            DeviceContext->Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IBuffer buffer, T[] values, int structSize) where T : unmanaged
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext->Map(resource, 0, Silk.NET.Direct3D11.Map.WriteDiscard, 0, &data);

            fixed (void* dataPtr = values)
            {
                Buffer.MemoryCopy(dataPtr, data.PData, data.RowPitch, structSize * values.Length);
            }

            DeviceContext->Unmap(resource, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(IBuffer buffer, T value) where T : struct
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(IBuffer buffer, T[] values) where T : struct
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DeviceContext->ClearState();
                DeviceContext->Flush();
                DeviceContext->Release();
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
            DeviceContext->Begin((ID3D11Asynchronous*)query.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueryEnd(IQuery query)
        {
            DeviceContext->End((ID3D11Asynchronous*)query.NativePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueryGetData(IQuery query)
        {
            DeviceContext->GetData((ID3D11Asynchronous*)query.NativePointer, null, 0, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Flush()
        {
            DeviceContext->Flush();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispatch(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
        {
            DeviceContext->Dispatch((uint)threadGroupCountX, (uint)threadGroupCountY, (uint)threadGroupCountZ);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessViews(int startSlot, int count, IUnorderedAccessView[] views, int uavInitialCounts = -1)
        {
            uint pUAVInitialCounts = unchecked((uint)uavInitialCounts);
            DeviceContext->CSSetUnorderedAccessViews((uint)startSlot, (uint)count, (ID3D11UnorderedAccessView**)Utils.ToPointerArray(views), &pUAVInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessViews(int startSlot, IUnorderedAccessView[] views, int uavInitialCounts = -1)
        {
            uint pUAVInitialCounts = unchecked((uint)uavInitialCounts);
            DeviceContext->CSSetUnorderedAccessViews((uint)startSlot, (uint)views.Length, (ID3D11UnorderedAccessView**)Utils.ToPointerArray(views), &pUAVInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessViews(IUnorderedAccessView[] views, int uavInitialCounts = -1)
        {
            uint pUAVInitialCounts = unchecked((uint)uavInitialCounts);
            DeviceContext->CSSetUnorderedAccessViews(0, (uint)views.Length, (ID3D11UnorderedAccessView**)Utils.ToPointerArray(views), &pUAVInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessViews(IUnorderedAccessView[] views)
        {
            uint pUAVInitialCounts = unchecked((uint)-1);
            DeviceContext->CSSetUnorderedAccessViews(0, (uint)views.Length, (ID3D11UnorderedAccessView**)Utils.ToPointerArray(views), &pUAVInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICommandList FinishCommandList(int restoreState)
        {
            ID3D11CommandList* commandList;
            DeviceContext->FinishCommandList(restoreState, &commandList);
            return new D3D11CommandList(commandList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteCommandList(ICommandList commandList, int restoreState)
        {
            DeviceContext->ExecuteCommandList((ID3D11CommandList*)commandList.NativePointer, restoreState);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetConstantBuffer(IBuffer? constantBuffer, int slot)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer?.NativePointer;
#nullable enable
            DeviceContext->VSSetConstantBuffers((uint)slot, 1, &buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetConstantBuffers(void** constantBuffers, uint count, int slot)
        {
            DeviceContext->VSSetConstantBuffers((uint)slot, count, (ID3D11Buffer**)constantBuffers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetConstantBuffer(IBuffer? constantBuffer, int slot)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer?.NativePointer;
#nullable enable
            DeviceContext->HSSetConstantBuffers((uint)slot, 1, &buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetConstantBuffers(void** constantBuffers, uint count, int slot)
        {
            DeviceContext->HSSetConstantBuffers((uint)slot, count, (ID3D11Buffer**)constantBuffers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetConstantBuffer(IBuffer? constantBuffer, int slot)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer?.NativePointer;
#nullable enable
            DeviceContext->DSSetConstantBuffers((uint)slot, 1, &buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetConstantBuffers(void** constantBuffers, uint count, int slot)
        {
            DeviceContext->DSSetConstantBuffers((uint)slot, count, (ID3D11Buffer**)constantBuffers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetConstantBuffer(IBuffer? constantBuffer, int slot)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer?.NativePointer;
#nullable enable
            DeviceContext->GSSetConstantBuffers((uint)slot, 1, &buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetConstantBuffers(void** constantBuffers, uint count, int slot)
        {
            DeviceContext->GSSetConstantBuffers((uint)slot, count, (ID3D11Buffer**)constantBuffers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetConstantBuffer(IBuffer? constantBuffer, int slot)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer?.NativePointer;
#nullable enable
            DeviceContext->PSSetConstantBuffers((uint)slot, 1, &buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetConstantBuffers(void** constantBuffers, uint count, int slot)
        {
            DeviceContext->PSSetConstantBuffers((uint)slot, count, (ID3D11Buffer**)constantBuffers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetConstantBuffer(IBuffer? constantBuffer, int slot)
        {
#nullable disable
            ID3D11Buffer* buffer = (ID3D11Buffer*)constantBuffer?.NativePointer;
#nullable enable
            DeviceContext->CSSetConstantBuffers((uint)slot, 1, &buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetConstantBuffers(void** constantBuffers, uint count, int slot)
        {
            DeviceContext->CSSetConstantBuffers((uint)slot, count, (ID3D11Buffer**)constantBuffers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetShaderResource(IShaderResourceView? shaderResourceView, int slot)
        {
#nullable disable
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView?.NativePointer;
#nullable enable
            DeviceContext->VSSetShaderResources((uint)slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetShaderResource(void* shaderResourceView, int slot)
        {
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView;
            DeviceContext->VSSetShaderResources((uint)slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetShaderResources(void** shaderResourceViews, uint count, int slot)
        {
            DeviceContext->VSSetShaderResources((uint)slot, count, (ID3D11ShaderResourceView**)shaderResourceViews);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetShaderResource(IShaderResourceView? shaderResourceView, int slot)
        {
#nullable disable
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView?.NativePointer;
#nullable enable
            DeviceContext->HSSetShaderResources((uint)slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetShaderResource(void* shaderResourceView, int slot)
        {
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView;
            DeviceContext->HSSetShaderResources((uint)slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetShaderResources(void** shaderResourceViews, uint count, int slot)
        {
            DeviceContext->HSSetShaderResources((uint)slot, count, (ID3D11ShaderResourceView**)shaderResourceViews);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetShaderResource(IShaderResourceView? shaderResourceView, int slot)
        {
#nullable disable
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView?.NativePointer;
#nullable enable
            DeviceContext->DSSetShaderResources((uint)slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetShaderResource(void* shaderResourceView, int slot)
        {
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView;
            DeviceContext->DSSetShaderResources((uint)slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetShaderResources(void** shaderResourceViews, uint count, int slot)
        {
            DeviceContext->DSSetShaderResources((uint)slot, count, (ID3D11ShaderResourceView**)shaderResourceViews);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetShaderResource(IShaderResourceView? shaderResourceView, int slot)
        {
#nullable disable
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView?.NativePointer;
#nullable enable
            DeviceContext->GSSetShaderResources((uint)slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetShaderResource(void* shaderResourceView, int slot)
        {
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView;
            DeviceContext->GSSetShaderResources((uint)slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetShaderResources(void** shaderResourceViews, uint count, int slot)
        {
            DeviceContext->GSSetShaderResources((uint)slot, count, (ID3D11ShaderResourceView**)shaderResourceViews);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetShaderResource(IShaderResourceView? shaderResourceView, int slot)
        {
#nullable disable
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView?.NativePointer;
#nullable enable
            DeviceContext->PSSetShaderResources((uint)slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetShaderResource(void* shaderResourceView, int slot)
        {
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView;
            DeviceContext->PSSetShaderResources((uint)slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetShaderResources(void** shaderResourceViews, uint count, int slot)
        {
            DeviceContext->PSSetShaderResources((uint)slot, count, (ID3D11ShaderResourceView**)shaderResourceViews);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetShaderResource(IShaderResourceView? shaderResourceView, int slot)
        {
#nullable disable
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView?.NativePointer;
#nullable enable
            DeviceContext->CSSetShaderResources((uint)slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetShaderResource(void* shaderResourceView, int slot)
        {
            ID3D11ShaderResourceView* srv = (ID3D11ShaderResourceView*)shaderResourceView;
            DeviceContext->CSSetShaderResources((uint)slot, 1, &srv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetShaderResources(void** shaderResourceViews, uint count, int slot)
        {
            DeviceContext->CSSetShaderResources((uint)slot, count, (ID3D11ShaderResourceView**)shaderResourceViews);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetSampler(ISamplerState? sampler, int slot)
        {
#nullable disable
            ID3D11SamplerState* smp = (ID3D11SamplerState*)sampler?.NativePointer;
#nullable enable
            DeviceContext->VSSetSamplers((uint)slot, 1, &smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetSamplers(void** samplers, uint count, int slot)
        {
            DeviceContext->VSSetSamplers((uint)slot, count, (ID3D11SamplerState**)samplers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetSampler(ISamplerState? sampler, int slot)
        {
#nullable disable
            ID3D11SamplerState* smp = (ID3D11SamplerState*)sampler?.NativePointer;
#nullable enable
            DeviceContext->HSSetSamplers((uint)slot, 1, &smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetSamplers(void** samplers, uint count, int slot)
        {
            DeviceContext->HSSetSamplers((uint)slot, count, (ID3D11SamplerState**)samplers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetSampler(ISamplerState? sampler, int slot)
        {
#nullable disable
            ID3D11SamplerState* smp = (ID3D11SamplerState*)sampler?.NativePointer;
#nullable enable
            DeviceContext->DSSetSamplers((uint)slot, 1, &smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetSamplers(void** samplers, uint count, int slot)
        {
            DeviceContext->DSSetSamplers((uint)slot, count, (ID3D11SamplerState**)samplers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetSampler(ISamplerState? sampler, int slot)
        {
#nullable disable
            ID3D11SamplerState* smp = (ID3D11SamplerState*)sampler?.NativePointer;
#nullable enable
            DeviceContext->GSSetSamplers((uint)slot, 1, &smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetSamplers(void** samplers, uint count, int slot)
        {
            DeviceContext->GSSetSamplers((uint)slot, count, (ID3D11SamplerState**)samplers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetSampler(ISamplerState? sampler, int slot)
        {
#nullable disable
            ID3D11SamplerState* smp = (ID3D11SamplerState*)sampler?.NativePointer;
#nullable enable
            DeviceContext->PSSetSamplers((uint)slot, 1, &smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetSamplers(void** samplers, uint count, int slot)
        {
            DeviceContext->PSSetSamplers((uint)slot, count, (ID3D11SamplerState**)samplers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetSampler(ISamplerState? sampler, int slot)
        {
#nullable disable
            ID3D11SamplerState* smp = (ID3D11SamplerState*)sampler?.NativePointer;
#nullable enable
            DeviceContext->CSSetSamplers((uint)slot, 1, &smp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetSamplers(void** samplers, uint count, int slot)
        {
            DeviceContext->CSSetSamplers((uint)slot, count, (ID3D11SamplerState**)samplers);
        }
    }
}