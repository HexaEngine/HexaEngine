namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.UI;
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public unsafe class UIRenderer
    {
        private static int vertexBufferSize = 5000, indexBufferSize = 10000;
        private readonly IGraphicsDevice device;

        private IBuffer vertexBuffer;
        private IBuffer indexBuffer;
        private IBuffer constantBuffer;
        private IGraphicsPipelineState pso;

        private IShaderResourceView fontTextureView;
        private ISamplerState fontSampler;

        public UIRenderer(IGraphicsDevice device)
        {
            CreateDeviceObjects(device);
            this.device = device;
        }

        private void CreateFontsTexture(IGraphicsDevice device)
        {
            int width = 2;
            int height = 2;
            uint* pixels = (uint*)Marshal.AllocHGlobal(width * height * sizeof(uint));

            var texDesc = new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8UNorm,
                SampleDescription = new SampleDescription { Count = 1 },
                Usage = Usage.Default,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None
            };

            var subResource = new SubresourceData
            {
                DataPointer = (nint)pixels,
                RowPitch = texDesc.Width * 4,
                SlicePitch = 0
            };

            var texture = device.CreateTexture2D(texDesc, new[] { subResource });

            Marshal.FreeHGlobal((nint)pixels);

            var resViewDesc = new ShaderResourceViewDescription
            {
                Format = Format.R8G8B8A8UNorm,
                ViewDimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new Texture2DShaderResourceView { MipLevels = texDesc.MipLevels, MostDetailedMip = 0 }
            };
            fontTextureView = device.CreateShaderResourceView(texture, resViewDesc);
            texture.Dispose();

            var samplerDesc = new SamplerStateDescription
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MipLODBias = 0f,
                ComparisonFunction = ComparisonFunction.Always,
                MinLOD = 0f,
                MaxLOD = 0f
            };
            fontSampler = device.CreateSamplerState(samplerDesc);
        }

        private void CreateDeviceObjects(IGraphicsDevice device)
        {
            var blendDesc = new BlendDescription
            {
                AlphaToCoverageEnable = false
            };

            blendDesc.RenderTarget[0] = new RenderTargetBlendDescription
            {
                BlendOperationAlpha = BlendOperation.Add,
                IsBlendEnabled = true,
                BlendOperation = BlendOperation.Add,
                DestinationBlendAlpha = Blend.InverseSourceAlpha,
                DestinationBlend = Blend.InverseSourceAlpha,
                SourceBlend = Blend.SourceAlpha,
                SourceBlendAlpha = Blend.SourceAlpha,
                RenderTargetWriteMask = ColorWriteEnable.All
            };

            var rasterDesc = new RasterizerDescription
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                ScissorEnable = false,
                DepthClipEnable = false,
            };

            var stencilOpDesc = new DepthStencilOperationDescription(StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, ComparisonFunction.Never);
            var depthDesc = new DepthStencilDescription
            {
                DepthEnable = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthFunc = ComparisonFunction.Always,
                StencilEnable = false,
                FrontFace = stencilOpDesc,
                BackFace = stencilOpDesc
            };

            var inputElements = new[]
            {
                new InputElementDescription( "POSITION", 0, Format.R32G32Float,   0, 0, InputClassification.PerVertexData, 0 ),
                new InputElementDescription( "TEXCOORD", 0, Format.R32G32Float,   8,  0, InputClassification.PerVertexData, 0 ),
                new InputElementDescription( "COLOR",    0, Format.R8G8B8A8UNorm, 16, 0, InputClassification.PerVertexData, 0 ),
            };

            pso = device.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    VertexShader = "internal/ui/vs.hlsl",
                    PixelShader = "internal/ui/ps.hlsl"
                },
                State = new()
                {
                    Blend = blendDesc,
                    Rasterizer = rasterDesc,
                    DepthStencil = depthDesc,
                    Topology = PrimitiveTopology.TriangleList,
                    InputElements = inputElements
                }
            });

            var constBufferDesc = new BufferDescription
            {
                ByteWidth = sizeof(Matrix4x4),
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
            };

            constantBuffer = device.CreateBuffer(constBufferDesc);
        }

        private void SetupRenderState(Viewport viewport, IGraphicsContext ctx)
        {
            ctx.SetViewport(viewport);

            uint stride = (uint)sizeof(RmGuiVertex);
            uint offset = 0;

            ctx.SetPipelineState(pso);
            ctx.SetVertexBuffer(0, vertexBuffer, stride, offset);
            ctx.SetIndexBuffer(indexBuffer, Format.R32UInt, 0);
            ctx.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            ctx.VSSetConstantBuffer(0, constantBuffer);
            ctx.PSSetSampler(0, fontSampler);
        }

        public void RenderDrawData(IGraphicsContext ctx, Viewport viewport, Matrix4x4 transform, UICommandList cmdList)
        {
            // Avoid rendering when minimized
            if (viewport.Width <= 0 || viewport.Height <= 0)
            {
                return;
            }

            if (cmdList.CmdBuffer.Size == 0)
            {
                return;
            }

            // Create and grow vertex/index buffers if needed
            if (vertexBuffer == null || vertexBufferSize < cmdList.TotalVtxCount)
            {
                vertexBuffer?.Dispose();
                vertexBufferSize = cmdList.TotalVtxCount + 5000;
                BufferDescription desc = new();
                desc.Usage = Usage.Dynamic;
                desc.ByteWidth = vertexBufferSize * sizeof(RmGuiVertex);
                desc.BindFlags = BindFlags.VertexBuffer;
                desc.CPUAccessFlags = CpuAccessFlags.Write;
                vertexBuffer = device.CreateBuffer(desc);
            }

            if (indexBuffer == null || indexBufferSize < cmdList.TotalIdxCount)
            {
                indexBuffer?.Dispose();
                indexBufferSize = cmdList.TotalIdxCount + 10000;
                BufferDescription desc = new();
                desc.Usage = Usage.Dynamic;
                desc.ByteWidth = indexBufferSize * sizeof(uint);
                desc.BindFlags = BindFlags.IndexBuffer;
                desc.CPUAccessFlags = CpuAccessFlags.Write;
                indexBuffer = device.CreateBuffer(desc);
            }

            var vertexResource = ctx.Map(vertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var indexResource = ctx.Map(indexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var vertexResourcePointer = (RmGuiVertex*)vertexResource.PData;
            var indexResourcePointer = (uint*)indexResource.PData;

            var vertBytes = cmdList.VtxBuffer.Size * sizeof(RmGuiVertex);
            Buffer.MemoryCopy(cmdList.VtxBuffer.Data, vertexResourcePointer, vertBytes, vertBytes);

            var idxBytes = cmdList.IdxBuffer.Size * sizeof(uint);
            Buffer.MemoryCopy(cmdList.IdxBuffer.Data, indexResourcePointer, idxBytes, idxBytes);

            ctx.Unmap(vertexBuffer, 0);
            ctx.Unmap(indexBuffer, 0);

            {
                var mappedResource = ctx.Map(constantBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
                Matrix4x4* constant_buffer = (Matrix4x4*)mappedResource.PData;

                *constant_buffer = Matrix4x4.Transpose(transform);

                ctx.Unmap(constantBuffer, 0);
            }

            SetupRenderState(viewport, ctx);

            uint global_idx_offset = 0;
            int global_vtx_offset = 0;

            for (int i = 0; i < cmdList.CmdBuffer.Size; i++)
            {
                var cmd = cmdList.CmdBuffer[i];

                //var srv = (void*)cmd.TextureId;
                //ctx.PSSetShaderResources(0, 1, &srv);

                ctx.DrawIndexedInstanced(cmd.IndexCount, 1, global_idx_offset, global_vtx_offset, 0);

                global_idx_offset += cmd.IndexCount;
                global_vtx_offset += (int)cmd.VertexCount;
            }

            ctx.SetPipelineState(null);
            ctx.SetViewport(default);
            ctx.SetVertexBuffer(0, null, 0, 0);
            ctx.SetIndexBuffer(null, default, 0);
            ctx.VSSetConstantBuffer(0, null);
            ctx.PSSetSampler(0, null);
            ctx.PSSetShaderResource(0, null);
        }

        public void Release()
        {
            pso.Dispose();
            fontSampler.Dispose();
            fontTextureView.Dispose();
            indexBuffer.Dispose();
            vertexBuffer.Dispose();
            constantBuffer.Dispose();
        }
    }
}