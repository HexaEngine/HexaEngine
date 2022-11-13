namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
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
            DeviceContext->ClearRenderTargetView((ID3D11RenderTargetView*)renderTargetView.NativePointer, value.ToFloatPtr());
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
            if (constantBuffer != null)
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
        public void SetConstantBuffer(IBuffer? constantBuffer, ShaderStage stage, int slot, uint firstConstant, uint constantCount)
        {
            uint* firstConstantPtr = Utils.AsPointer(firstConstant);
            uint* constantCountPtr = Utils.AsPointer(constantCount);
            if (constantBuffer != null)
                switch (stage)
                {
                    case ShaderStage.Vertex:
                        DeviceContext->VSSetConstantBuffers1((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)constantBuffer.NativePointer), firstConstantPtr, constantCountPtr);
                        break;

                    case ShaderStage.Hull:
                        DeviceContext->HSSetConstantBuffers1((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)constantBuffer.NativePointer), firstConstantPtr, constantCountPtr);
                        break;

                    case ShaderStage.Domain:
                        DeviceContext->DSSetConstantBuffers1((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)constantBuffer.NativePointer), firstConstantPtr, constantCountPtr);
                        break;

                    case ShaderStage.Geometry:
                        DeviceContext->GSSetConstantBuffers1((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)constantBuffer.NativePointer), firstConstantPtr, constantCountPtr);
                        break;

                    case ShaderStage.Pixel:
                        DeviceContext->PSSetConstantBuffers1((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)constantBuffer.NativePointer), firstConstantPtr, constantCountPtr);
                        break;

                    case ShaderStage.Compute:
                        DeviceContext->CSSetConstantBuffers1((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)constantBuffer.NativePointer), firstConstantPtr, constantCountPtr);
                        break;
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
            ID3D11Buffer** ptr = Utils.ToPointerArray<IBuffer, ID3D11Buffer>(constantBuffers);
            uint* firstConstantPtr = Utils.AsPointer(firstConstant);
            uint* constantCountPtr = Utils.AsPointer(constantCount);
            uint count = (uint)constantBuffers.Length;
            if (count == 0) return;
            switch (stage)
            {
                case ShaderStage.Vertex:
                    DeviceContext->VSSetConstantBuffers1((uint)slot, count, ptr, firstConstantPtr, constantCountPtr);
                    break;

                case ShaderStage.Hull:
                    DeviceContext->HSSetConstantBuffers1((uint)slot, count, ptr, firstConstantPtr, constantCountPtr);
                    break;

                case ShaderStage.Domain:
                    DeviceContext->DSSetConstantBuffers1((uint)slot, count, ptr, firstConstantPtr, constantCountPtr);
                    break;

                case ShaderStage.Geometry:
                    DeviceContext->GSSetConstantBuffers1((uint)slot, count, ptr, firstConstantPtr, constantCountPtr);
                    break;

                case ShaderStage.Pixel:
                    DeviceContext->PSSetConstantBuffers1((uint)slot, count, ptr, firstConstantPtr, constantCountPtr);
                    break;

                case ShaderStage.Compute:
                    DeviceContext->CSSetConstantBuffers1((uint)slot, count, ptr, firstConstantPtr, constantCountPtr);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetIndexBuffer(IBuffer? indexBuffer, Format format, int offset)
        {
            if (indexBuffer != null)
                DeviceContext->IASetIndexBuffer((ID3D11Buffer*)indexBuffer.NativePointer, Helper.Convert(format), (uint)offset);
            else
                DeviceContext->IASetIndexBuffer((ID3D11Buffer*)null, Helper.Convert(format), (uint)offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetInputLayout(IInputLayout? inputLayout)
        {
            if (inputLayout != null)
                DeviceContext->IASetInputLayout((ID3D11InputLayout*)inputLayout.NativePointer);
            else
                DeviceContext->IASetInputLayout((ID3D11InputLayout*)null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPrimitiveTopology(PrimitiveTopology topology)
        {
            DeviceContext->IASetPrimitiveTopology(Helper.Convert(topology));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRenderTarget(IRenderTargetView? renderTargetView, IDepthStencilView? depthStencilView)
        {
            if (renderTargetView != null)
                if (depthStencilView is not null)
                    DeviceContext->OMSetRenderTargets(1, Utils.AsPointer((ID3D11RenderTargetView*)renderTargetView.NativePointer), (ID3D11DepthStencilView*)depthStencilView.NativePointer);
                else
                    DeviceContext->OMSetRenderTargets(1, Utils.AsPointer((ID3D11RenderTargetView*)renderTargetView.NativePointer), null);
            else
                DeviceContext->OMSetRenderTargets(1, Utils.AsPointer((ID3D11RenderTargetView*)null), null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRenderTargets(IRenderTargetView[] views, IDepthStencilView? depthStencilView)
        {
            ID3D11RenderTargetView** ptr = Utils.ToPointerArray<IRenderTargetView, ID3D11RenderTargetView>(views);
            if (depthStencilView != null)
                DeviceContext->OMSetRenderTargets((uint)views.Length, ptr, (ID3D11DepthStencilView*)depthStencilView.NativePointer);
            else
                DeviceContext->OMSetRenderTargets((uint)views.Length, ptr, (ID3D11DepthStencilView*)null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSampler(ISamplerState? sampler, ShaderStage stage, int slot)
        {
            if (sampler != null)
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
            else
                switch (stage)
                {
                    case ShaderStage.Vertex:
                        DeviceContext->VSSetSamplers((uint)slot, 1, Utils.AsPointer((ID3D11SamplerState*)null));
                        break;

                    case ShaderStage.Hull:
                        DeviceContext->HSSetSamplers((uint)slot, 1, Utils.AsPointer((ID3D11SamplerState*)null));
                        break;

                    case ShaderStage.Domain:
                        DeviceContext->DSSetSamplers((uint)slot, 1, Utils.AsPointer((ID3D11SamplerState*)null));
                        break;

                    case ShaderStage.Geometry:
                        DeviceContext->GSSetSamplers((uint)slot, 1, Utils.AsPointer((ID3D11SamplerState*)null));
                        break;

                    case ShaderStage.Pixel:
                        DeviceContext->PSSetSamplers((uint)slot, 1, Utils.AsPointer((ID3D11SamplerState*)null));
                        break;

                    case ShaderStage.Compute:
                        DeviceContext->CSSetSamplers((uint)slot, 1, Utils.AsPointer((ID3D11SamplerState*)null));
                        break;
                }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSamplers(ISamplerState[] samplers, ShaderStage stage, int slot)
        {
            ID3D11SamplerState** ptr = Utils.ToPointerArray<ISamplerState, ID3D11SamplerState>(samplers);
            switch (stage)
            {
                case ShaderStage.Vertex:
                    DeviceContext->VSSetSamplers((uint)slot, (uint)samplers.Length, ptr);
                    break;

                case ShaderStage.Hull:
                    DeviceContext->HSSetSamplers((uint)slot, (uint)samplers.Length, ptr);
                    break;

                case ShaderStage.Domain:
                    DeviceContext->DSSetSamplers((uint)slot, (uint)samplers.Length, ptr);
                    break;

                case ShaderStage.Geometry:
                    DeviceContext->GSSetSamplers((uint)slot, (uint)samplers.Length, ptr);
                    break;

                case ShaderStage.Pixel:
                    DeviceContext->PSSetSamplers((uint)slot, (uint)samplers.Length, ptr);
                    break;

                case ShaderStage.Compute:
                    DeviceContext->CSSetSamplers((uint)slot, (uint)samplers.Length, ptr);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetScissorRect(int x, int y, int z, int w)
        {
            Rectangle<int> rect = new(x, y, z, w);
            DeviceContext->RSSetScissorRects(1, Utils.AsPointer(rect));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetShaderResource(IShaderResourceView? shaderResourceView, ShaderStage stage, int slot)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetShaderResources(IShaderResourceView[] shaderResourceViews, ShaderStage stage, int slot)
        {
            ID3D11ShaderResourceView** ptr = Utils.ToPointerArray<IShaderResourceView, ID3D11ShaderResourceView>(shaderResourceViews);
            for (int i = 0; i < shaderResourceViews.Length; i++)
            {
                var ptr2 = ptr[i];
            }
            switch (stage)
            {
                case ShaderStage.Vertex:
                    DeviceContext->VSSetShaderResources((uint)slot, (uint)shaderResourceViews.Length, ptr);
                    break;

                case ShaderStage.Hull:
                    DeviceContext->HSSetShaderResources((uint)slot, (uint)shaderResourceViews.Length, ptr);
                    break;

                case ShaderStage.Domain:
                    DeviceContext->DSSetShaderResources((uint)slot, (uint)shaderResourceViews.Length, ptr);
                    break;

                case ShaderStage.Geometry:
                    DeviceContext->GSSetShaderResources((uint)slot, (uint)shaderResourceViews.Length, ptr);
                    break;

                case ShaderStage.Pixel:
                    DeviceContext->PSSetShaderResources((uint)slot, (uint)shaderResourceViews.Length, ptr);
                    break;

                case ShaderStage.Compute:
                    DeviceContext->CSSetShaderResources((uint)slot, (uint)shaderResourceViews.Length, ptr);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRasterizerState(IRasterizerState? rasterizerState)
        {
            if (rasterizerState is not null)
                DeviceContext->RSSetState((ID3D11RasterizerState*)rasterizerState.NativePointer);
            else
                DeviceContext->RSSetState(null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetDepthStencilState(IDepthStencilState? depthStencilState, int stencilRef = 0)
        {
            if (depthStencilState is not null)
                DeviceContext->OMSetDepthStencilState((ID3D11DepthStencilState*)depthStencilState.NativePointer, (uint)stencilRef);
            else
                DeviceContext->OMSetDepthStencilState(null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlendState(IBlendState? blendState, Vector4? factor, uint sampleMask = uint.MaxValue)
        {
            if (blendState is not null)
                if (factor.HasValue)
                    DeviceContext->OMSetBlendState((ID3D11BlendState*)blendState.NativePointer, factor.Value.ToFloatPtr(), sampleMask);
                else
                    DeviceContext->OMSetBlendState((ID3D11BlendState*)blendState.NativePointer, null, sampleMask);
            else
                DeviceContext->OMSetBlendState(null, null, sampleMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertexBuffer(IBuffer? vertexBuffer, int stride, int offset)
        {
            if (vertexBuffer is not null)
                DeviceContext->IASetVertexBuffers(0, 1, Utils.AsPointer((ID3D11Buffer*)vertexBuffer.NativePointer), Utils.AsPointer((uint)stride), Utils.AsPointer((uint)offset));
            else
                DeviceContext->IASetVertexBuffers(0, 1, Utils.AsPointer((ID3D11Buffer*)null), Utils.AsPointer((uint)stride), Utils.AsPointer((uint)offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertexBuffer(IBuffer? vertexBuffer, int stride)
        {
            if (vertexBuffer is not null)
                DeviceContext->IASetVertexBuffers(0, 1, Utils.AsPointer((ID3D11Buffer*)vertexBuffer.NativePointer), Utils.AsPointer((uint)stride), Utils.AsPointer(0u));
            else
                DeviceContext->IASetVertexBuffers(0, 1, Utils.AsPointer((ID3D11Buffer*)null), Utils.AsPointer((uint)stride), Utils.AsPointer(0u));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertexBuffer(int slot, IBuffer? vertexBuffer, int stride, int offset)
        {
            if (vertexBuffer is not null)
                DeviceContext->IASetVertexBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)vertexBuffer.NativePointer), Utils.AsPointer((uint)stride), Utils.AsPointer((uint)offset));
            else
                DeviceContext->IASetVertexBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)null), Utils.AsPointer((uint)stride), Utils.AsPointer((uint)offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertexBuffer(int slot, IBuffer? vertexBuffer, int stride)
        {
            if (vertexBuffer is not null)
                DeviceContext->IASetVertexBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)vertexBuffer.NativePointer), Utils.AsPointer((uint)stride), Utils.AsPointer(0u));
            else
                DeviceContext->IASetVertexBuffers((uint)slot, 1, Utils.AsPointer((ID3D11Buffer*)null), Utils.AsPointer((uint)stride), Utils.AsPointer(0u));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetViewport(Viewport viewport)
        {
            DeviceContext->RSSetViewports(1, Utils.AsPointer(Helper.Convert(viewport)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VSSetShader(IVertexShader? vertexShader)
        {
            if (vertexShader is not null && !vertexShader.IsDisposed)
                DeviceContext->VSSetShader((ID3D11VertexShader*)vertexShader.NativePointer, null, 0);
            else
                DeviceContext->VSSetShader(null, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HSSetShader(IHullShader? hullShader)
        {
            if (hullShader is not null && !hullShader.IsDisposed)
                DeviceContext->HSSetShader((ID3D11HullShader*)hullShader.NativePointer, null, 0);
            else
                DeviceContext->HSSetShader(null, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DSSetShader(IDomainShader? domainShader)
        {
            if (domainShader is not null && !domainShader.IsDisposed)
                DeviceContext->DSSetShader((ID3D11DomainShader*)domainShader.NativePointer, null, 0);
            else
                DeviceContext->DSSetShader(null, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GSSetShader(IGeometryShader? geometryShader)
        {
            if (geometryShader is not null && !geometryShader.IsDisposed)
                DeviceContext->GSSetShader((ID3D11GeometryShader*)geometryShader.NativePointer, null, 0);
            else
                DeviceContext->GSSetShader(null, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PSSetShader(IPixelShader? pixelShader)
        {
            if (pixelShader is not null && !pixelShader.IsDisposed)
                DeviceContext->PSSetShader((ID3D11PixelShader*)pixelShader.NativePointer, null, 0);
            else
                DeviceContext->PSSetShader(null, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetShader(IComputeShader? computeShader)
        {
            if (computeShader is not null && !computeShader.IsDisposed)
                DeviceContext->CSSetShader((ID3D11ComputeShader*)computeShader.NativePointer, null, 0);
            else
                DeviceContext->CSSetShader(null, null, 0);
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
            DeviceContext->CSSetUnorderedAccessViews((uint)startSlot, (uint)count, (ID3D11UnorderedAccessView**)Utils.ToPointerArray(views), (uint*)pUAVInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessViews(int startSlot, IUnorderedAccessView[] views, int uavInitialCounts = -1)
        {
            uint pUAVInitialCounts = unchecked((uint)uavInitialCounts);
            DeviceContext->CSSetUnorderedAccessViews((uint)startSlot, (uint)views.Length, (ID3D11UnorderedAccessView**)Utils.ToPointerArray(views), (uint*)pUAVInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessViews(IUnorderedAccessView[] views, int uavInitialCounts = -1)
        {
            uint pUAVInitialCounts = unchecked((uint)uavInitialCounts);
            DeviceContext->CSSetUnorderedAccessViews(0, (uint)views.Length, (ID3D11UnorderedAccessView**)Utils.ToPointerArray(views), (uint*)pUAVInitialCounts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CSSetUnorderedAccessViews(IUnorderedAccessView[] views)
        {
            uint pUAVInitialCounts = unchecked((uint)-1);
            DeviceContext->CSSetUnorderedAccessViews(0, (uint)views.Length, (ID3D11UnorderedAccessView**)Utils.ToPointerArray(views), (uint*)pUAVInitialCounts);
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
    }
}