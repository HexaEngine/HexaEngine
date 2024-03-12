namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.UI;
    using HexaEngine.UI.Graphics;
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public unsafe class UIRenderer
    {
        private static int vertexBufferSize = 5000, indexBufferSize = 10000;
        private readonly IGraphicsDevice device;

        private IGraphicsPipelineState primPso;
        private IGraphicsPipelineState texPso;
        private IGraphicsPipelineState bezierPso;
        private IBuffer vertexBuffer;
        private IBuffer indexBuffer;
        private IBuffer constantBuffer;
        private ISamplerState fontSampler;

        public UIRenderer(IGraphicsDevice device)
        {
            CreateDeviceObjects(device);
            this.device = device;
        }

        private void CreateFontsTexture(IGraphicsDevice device)
        {
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
                ScissorEnable = true,
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

            GraphicsPipelineStateDesc stateDesc = new()
            {
                Blend = blendDesc,
                Rasterizer = rasterDesc,
                DepthStencil = depthDesc,
                StencilRef = 0,
                BlendFactor = Vector4.One,
                InputElements = inputElements,
                Topology = PrimitiveTopology.TriangleList,
            };

            primPso = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "internal/ui/prim/vs.hlsl",
                PixelShader = "internal/ui/prim/ps.hlsl"
            }, stateDesc);

            texPso = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "internal/ui/tex/vs.hlsl",
                PixelShader = "internal/ui/tex/ps.hlsl"
            }, stateDesc);

            stateDesc.InputElements = null;

            bezierPso = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "internal/ui/vec/vs.hlsl",
                PixelShader = "internal/ui/vec/ps.hlsl"
            }, stateDesc);

            var constBufferDesc = new BufferDescription
            {
                ByteWidth = sizeof(Matrix4x4),
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
            };

            constantBuffer = device.CreateBuffer(constBufferDesc);

            CreateFontsTexture(device);
        }

        private void SetupRenderState(Viewport viewport, IGraphicsContext ctx)
        {
            ctx.SetViewport(viewport);

            uint stride = (uint)sizeof(UIVertex);
            uint offset = 0;

            ctx.SetVertexBuffer(0, vertexBuffer, stride, offset);
            ctx.SetIndexBuffer(indexBuffer, Format.R32UInt, 0);
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
                BufferDescription desc = new()
                {
                    Usage = Usage.Dynamic,
                    ByteWidth = vertexBufferSize * sizeof(UIVertex),
                    BindFlags = BindFlags.VertexBuffer,
                    CPUAccessFlags = CpuAccessFlags.Write
                };
                vertexBuffer = device.CreateBuffer(desc);
            }

            if (indexBuffer == null || indexBufferSize < cmdList.TotalIdxCount)
            {
                indexBuffer?.Dispose();
                indexBufferSize = cmdList.TotalIdxCount + 10000;
                BufferDescription desc = new()
                {
                    Usage = Usage.Dynamic,
                    ByteWidth = indexBufferSize * sizeof(uint),
                    BindFlags = BindFlags.IndexBuffer,
                    CPUAccessFlags = CpuAccessFlags.Write
                };
                indexBuffer = device.CreateBuffer(desc);
            }

            var vertexResource = ctx.Map(vertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var indexResource = ctx.Map(indexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var vertexResourcePointer = (UIVertex*)vertexResource.PData;
            var indexResourcePointer = (uint*)indexResource.PData;

            var vertBytes = cmdList.VtxBuffer.Size * sizeof(UIVertex);
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

            nint* srvs = stackalloc nint[2] { 0, 0 };

            for (int i = 0; i < cmdList.CmdBuffer.Size; i++)
            {
                var cmd = cmdList.CmdBuffer[i];
                var clip = cmd.ClipRect;

                switch (cmd.Type)
                {
                    case UICommandType.None:
                        break;

                    case UICommandType.DrawPrimitive:
                        ctx.SetPipelineState(primPso);
                        break;

                    case UICommandType.DrawTexture:
                        ctx.SetPipelineState(texPso);
                        break;

                    case UICommandType.DrawTextVector:
                        ctx.SetPipelineState(bezierPso);
                        break;
                }

                srvs[0] = cmd.TextureId0;
                srvs[1] = cmd.TextureId1;

                ctx.PSSetShaderResources(0, 2, (void**)srvs);

                ctx.SetScissorRect(clip.Left, clip.Top, clip.Right, clip.Bottom);
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
            ctx.PSSetShaderResource(1, null);
            ctx.SetScissorRect(default, default, default, default);
        }

        public void Release()
        {
            primPso.Dispose();
            texPso.Dispose();
            bezierPso.Dispose();
            fontSampler.Dispose();
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();
            constantBuffer.Dispose();
        }
    }
}