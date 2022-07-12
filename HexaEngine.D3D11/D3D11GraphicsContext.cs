﻿namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using Silk.NET.Maths;
    using System;
    using System.Numerics;
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

        public IGraphicsDevice Device { get; }

        public IntPtr NativePointer { get; }

        public string? DebugName { get; set; } = string.Empty;

        public void ClearDepthStencilView(IDepthStencilView depthStencilView, DepthStencilClearFlags flags, float depth, byte stencil)
        {
            DeviceContext->ClearDepthStencilView((ID3D11DepthStencilView*)depthStencilView.NativePointer, (uint)Helper.Convert(flags), depth, stencil);
        }

        public void ClearRenderTargetView(IRenderTargetView renderTargetView, Vector4 value)
        {
            DeviceContext->ClearRenderTargetView((ID3D11RenderTargetView*)renderTargetView.NativePointer, value.ToFloatPtr());
        }

        public void ClearState()
        {
            DeviceContext->ClearState();
        }

        public void Draw(int vertexCount, int offset)
        {
            DeviceContext->Draw((uint)vertexCount, (uint)offset);
        }

        public void DrawIndexed(int indexCount, int indexOffset, int vertexOffset)
        {
            DeviceContext->DrawIndexed((uint)indexCount, (uint)indexOffset, vertexOffset);
        }

        public void DrawInstanced(int vertexCount, int instanceCount, int vertexOffset, int instanceOffset)
        {
            DeviceContext->DrawInstanced((uint)vertexCount, (uint)instanceCount, (uint)vertexOffset, (uint)instanceOffset);
        }

        public void DrawIndexedInstanced(int indexCount, int instanceCount, int indexOffset, int vertexOffset, int instanceOffset)
        {
            DeviceContext->DrawIndexedInstanced((uint)indexCount, (uint)instanceCount, (uint)indexOffset, vertexOffset, (uint)instanceOffset);
        }

        public MappedSubresource Map(IResource resource, int subresourceIndex, MapMode mode, MapFlags flags)
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            DeviceContext->Map((ID3D11Resource*)resource.NativePointer, (uint)subresourceIndex, Helper.Convert(mode), (uint)Helper.Convert(flags), &data);
            return new(data.PData, data.RowPitch, data.DepthPitch);
        }

        public void Unmap(IResource resource, int subresourceIndex)
        {
            DeviceContext->Unmap((ID3D11Resource*)resource.NativePointer, (uint)subresourceIndex);
        }

        public void GenerateMips(IShaderResourceView resourceView)
        {
            DeviceContext->GenerateMips((ID3D11ShaderResourceView*)resourceView.NativePointer);
        }

        public void SetConstantBuffer(IBuffer constantBuffer, ShaderStage stage, int slot)
        {
            switch (stage)
            {
                case ShaderStage.Vertex:
                    DeviceContext->VSSetConstantBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)constantBuffer.NativePointer));
                    break;

                case ShaderStage.Hull:
                    DeviceContext->HSSetConstantBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)constantBuffer.NativePointer));
                    break;

                case ShaderStage.Domain:
                    DeviceContext->DSSetConstantBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)constantBuffer.NativePointer));
                    break;

                case ShaderStage.Geometry:
                    DeviceContext->GSSetConstantBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)constantBuffer.NativePointer));
                    break;

                case ShaderStage.Pixel:
                    DeviceContext->PSSetConstantBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)constantBuffer.NativePointer));
                    break;

                case ShaderStage.Compute:
                    DeviceContext->CSSetConstantBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)constantBuffer.NativePointer));
                    break;
            }
        }

        public void SetIndexBuffer(IBuffer indexBuffer, Format format, int offset)
        {
            DeviceContext->IASetIndexBuffer((ID3D11Buffer*)indexBuffer.NativePointer, Helper.Convert(format), (uint)offset);
        }

        public void SetInputLayout(IInputLayout inputLayout)
        {
            DeviceContext->IASetInputLayout((ID3D11InputLayout*)inputLayout.NativePointer);
        }

        public void SetPrimitiveTopology(PrimitiveTopology topology)
        {
            DeviceContext->IASetPrimitiveTopology(Helper.Convert(topology));
        }

        public void SetRenderTargets(IRenderTargetView renderTargetView, IDepthStencilView depthStencilView)
        {
            if (depthStencilView is not null)
                DeviceContext->OMSetRenderTargets(1, Utils.AsPointer((ID3D11RenderTargetView*)renderTargetView.NativePointer), (ID3D11DepthStencilView*)depthStencilView.NativePointer);
            else
                DeviceContext->OMSetRenderTargets(1, Utils.AsPointer((ID3D11RenderTargetView*)renderTargetView.NativePointer), null);
        }

        public void SetRenderTargets(IRenderTargetView[] views, IDepthStencilView depthStencilView)
        {
            DeviceContext->OMSetRenderTargets((uint)views.Length, Utils.ToPointerArray<IRenderTargetView, ID3D11RenderTargetView>(views), (ID3D11DepthStencilView*)depthStencilView?.NativePointer);
        }

        public void SetSampler(ISamplerState sampler, ShaderStage stage, int slot)
        {
            switch (stage)
            {
                case ShaderStage.Vertex:
                    DeviceContext->VSSetSamplers((uint)slot, 1, Utils.AsPointer((ID3D11SamplerState*)sampler.NativePointer));
                    break;

                case ShaderStage.Hull:
                    DeviceContext->HSSetSamplers((uint)slot, 1, Utils.AsPointer((ID3D11SamplerState*)sampler.NativePointer));
                    break;

                case ShaderStage.Domain:
                    DeviceContext->DSSetSamplers((uint)slot, 1, Utils.AsPointer((ID3D11SamplerState*)sampler.NativePointer));
                    break;

                case ShaderStage.Geometry:
                    DeviceContext->GSSetSamplers((uint)slot, 1, Utils.AsPointer((ID3D11SamplerState*)sampler.NativePointer));
                    break;

                case ShaderStage.Pixel:
                    DeviceContext->PSSetSamplers((uint)slot, 1, Utils.AsPointer((ID3D11SamplerState*)sampler.NativePointer));
                    break;

                case ShaderStage.Compute:
                    DeviceContext->CSSetSamplers((uint)slot, 1, Utils.AsPointer((ID3D11SamplerState*)sampler.NativePointer));
                    break;
            }
        }

        public void SetScissorRect(int x, int y, int z, int w)
        {
            Rectangle<int> rect = new(x, y, z, w);
            DeviceContext->RSSetScissorRects(1, Utils.AsPointer(rect));
        }

        public void SetShaderResource(IShaderResourceView shaderResourceView, ShaderStage stage, int slot)
        {
            if (shaderResourceView is not null && !shaderResourceView.IsDisposed)
                switch (stage)
                {
                    case ShaderStage.Vertex:
                        DeviceContext->VSSetShaderResources((uint)slot, 1, Utils.AsPointer((ID3D11ShaderResourceView*)shaderResourceView.NativePointer));
                        break;

                    case ShaderStage.Hull:
                        DeviceContext->HSSetShaderResources((uint)slot, 1, Utils.AsPointer((ID3D11ShaderResourceView*)shaderResourceView.NativePointer));
                        break;

                    case ShaderStage.Domain:
                        DeviceContext->DSSetShaderResources((uint)slot, 1, Utils.AsPointer((ID3D11ShaderResourceView*)shaderResourceView.NativePointer));
                        break;

                    case ShaderStage.Geometry:
                        DeviceContext->GSSetShaderResources((uint)slot, 1, Utils.AsPointer((ID3D11ShaderResourceView*)shaderResourceView.NativePointer));
                        break;

                    case ShaderStage.Pixel:
                        DeviceContext->PSSetShaderResources((uint)slot, 1, Utils.AsPointer((ID3D11ShaderResourceView*)shaderResourceView.NativePointer));
                        break;

                    case ShaderStage.Compute:
                        DeviceContext->CSSetShaderResources((uint)slot, 1, Utils.AsPointer((ID3D11ShaderResourceView*)shaderResourceView.NativePointer));
                        break;
                }
            else
                switch (stage)
                {
                    case ShaderStage.Vertex:
                        DeviceContext->VSSetShaderResources((uint)slot, 1, Utils.AsPointer((ID3D11ShaderResourceView*)null));
                        break;

                    case ShaderStage.Hull:
                        DeviceContext->HSSetShaderResources((uint)slot, 1, Utils.AsPointer((ID3D11ShaderResourceView*)null));
                        break;

                    case ShaderStage.Domain:
                        DeviceContext->DSSetShaderResources((uint)slot, 1, Utils.AsPointer((ID3D11ShaderResourceView*)null));
                        break;

                    case ShaderStage.Geometry:
                        DeviceContext->GSSetShaderResources((uint)slot, 1, Utils.AsPointer((ID3D11ShaderResourceView*)null));
                        break;

                    case ShaderStage.Pixel:
                        DeviceContext->PSSetShaderResources((uint)slot, 1, Utils.AsPointer((ID3D11ShaderResourceView*)null));
                        break;

                    case ShaderStage.Compute:
                        DeviceContext->CSSetShaderResources((uint)slot, 1, Utils.AsPointer((ID3D11ShaderResourceView*)null));
                        break;
                }
        }

        public void SetRasterizerState(IRasterizerState rasterizerState)
        {
            if (rasterizerState is not null)
                DeviceContext->RSSetState((ID3D11RasterizerState*)rasterizerState.NativePointer);
            else
                DeviceContext->RSSetState(null);
        }

        public void SetDepthStencilState(IDepthStencilState depthStencilState, int stencilRef = 0)
        {
            if (depthStencilState is not null)
                DeviceContext->OMSetDepthStencilState((ID3D11DepthStencilState*)depthStencilState.NativePointer, (uint)stencilRef);
            else
                DeviceContext->OMSetDepthStencilState(null, 0);
        }

        public void SetBlendState(IBlendState blendState, Vector4? factor, uint sampleMask = uint.MaxValue)
        {
            if (blendState is not null)
                if (factor.HasValue)
                    DeviceContext->OMSetBlendState((ID3D11BlendState*)blendState.NativePointer, factor.Value.ToFloatPtr(), sampleMask);
                else
                    DeviceContext->OMSetBlendState((ID3D11BlendState*)blendState.NativePointer, null, sampleMask);
            else
                DeviceContext->OMSetBlendState(null, null, sampleMask);
        }

        public void SetVertexBuffer(IBuffer vertexBuffer, int stride, int offset)
        {
            DeviceContext->IASetVertexBuffers(0, 1, Utils.AsPointer((ID3D11Buffer*)vertexBuffer.NativePointer), Utils.AsPointer((uint)stride), Utils.AsPointer((uint)offset));
        }

        public void SetVertexBuffer(IBuffer vertexBuffer, int stride)
        {
            DeviceContext->IASetVertexBuffers(0, 1, Utils.AsPointer((ID3D11Buffer*)vertexBuffer.NativePointer), Utils.AsPointer((uint)stride), Utils.AsPointer(0u));
        }

        public void SetVertexBuffer(int slot, IBuffer vertexBuffer, int stride, int offset)
        {
            DeviceContext->IASetVertexBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)vertexBuffer.NativePointer), Utils.AsPointer((uint)stride), Utils.AsPointer((uint)offset));
        }

        public void SetVertexBuffer(int slot, IBuffer vertexBuffer, int stride)
        {
            DeviceContext->IASetVertexBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)vertexBuffer.NativePointer), Utils.AsPointer((uint)stride), Utils.AsPointer(0u));
        }

        public void SetViewport(Viewport viewport)
        {
            DeviceContext->RSSetViewports(1, Utils.AsPointer(Helper.Convert(viewport)));
        }

        public void VSSetShader(IVertexShader vertexShader)
        {
            if (vertexShader is not null)
                DeviceContext->VSSetShader((ID3D11VertexShader*)vertexShader.NativePointer, null, 0);
            else
                DeviceContext->VSSetShader(null, null, 0);
        }

        public void HSSetShader(IHullShader hullShader)
        {
            if (hullShader is not null)
                DeviceContext->HSSetShader((ID3D11HullShader*)hullShader.NativePointer, null, 0);
            else
                DeviceContext->HSSetShader(null, null, 0);
        }

        public void DSSetShader(IDomainShader domainShader)
        {
            if (domainShader is not null)
                DeviceContext->DSSetShader((ID3D11DomainShader*)domainShader.NativePointer, null, 0);
            else
                DeviceContext->DSSetShader(null, null, 0);
        }

        public void GSSetShader(IGeometryShader geometryShader)
        {
            if (geometryShader is not null)
                DeviceContext->GSSetShader((ID3D11GeometryShader*)geometryShader.NativePointer, null, 0);
            else
                DeviceContext->GSSetShader(null, null, 0);
        }

        public void PSSetShader(IPixelShader pixelShader)
        {
            if (pixelShader is not null)
                DeviceContext->PSSetShader((ID3D11PixelShader*)pixelShader.NativePointer, null, 0);
            else
                DeviceContext->PSSetShader(null, null, 0);
        }

        public void CSSetShader(IComputeShader computeShader)
        {
            if (computeShader is not null)
                DeviceContext->CSSetShader((ID3D11ComputeShader*)computeShader.NativePointer, null, 0);
            else
                DeviceContext->CSSetShader(null, null, 0);
        }

        public void Write<T>(IBuffer buffer, T value) where T : struct
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext->Map(resource, 0, Silk.NET.Direct3D11.Map.MapWriteDiscard, 0, &data);
            var size = Marshal.SizeOf<T>();
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            Buffer.MemoryCopy((void*)ptr, data.PData, data.RowPitch, size);
            Marshal.FreeHGlobal(ptr);
            DeviceContext->Unmap(resource, 0);
        }

        public void Write<T>(IBuffer buffer, T[] values) where T : struct
        {
            Silk.NET.Direct3D11.MappedSubresource data;
            ID3D11Resource* resource = (ID3D11Resource*)buffer.NativePointer;
            DeviceContext->Map(resource, 0, Silk.NET.Direct3D11.Map.MapWriteDiscard, 0, &data);
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

        public void Read<T>(IBuffer buffer, T value) where T : struct
        {
            throw new NotImplementedException();
        }

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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~D3D11GraphicsContext()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}