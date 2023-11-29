namespace HexaEngine.Graphics.Renderers
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using System;
    using System.Numerics;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public unsafe class DebugDrawRenderer : IDisposable
    {
        private static IGraphicsDevice device;
        private static IGraphicsContext context;
        private static IGraphicsPipeline pipeline;
        private static IBuffer vertexBuffer;
        private static IBuffer indexBuffer;
        private static IBuffer constantBuffer;
        private static ISamplerState fontSampler;
        private static IShaderResourceView fontTextureView;

        private static int vertexBufferSize = 5000;
        private static int indexBufferSize = 10000;
        private bool disposedValue;

        public DebugDrawRenderer(IGraphicsDevice device)
        {
            DebugDrawRenderer.device = device;

            context = device.Context;

            CreateDeviceObjects();
        }

        private static void CreateDeviceObjects()
        {
            var desc = RasterizerDescription.CullNone;
            desc.AntialiasedLineEnable = true;
            desc.MultisampleEnable = false;

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "internal/debugdraw/vs.hlsl",
                PixelShader = "internal/debugdraw/ps.hlsl",
                State = new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Blend = BlendDescription.NonPremultiplied,
                    Rasterizer = desc,
                    BlendFactor = Vector4.One,
                    SampleMask = int.MaxValue,
                },
                InputElements =
                [
                    new("POSITION", 0, Format.R32G32B32Float, 0),
                    new("TEXCOORD", 0, Format.R32G32Float, 0),
                    new("COLOR", 0, Format.R8G8B8A8UNorm, 0),
                ]
            });

            vertexBuffer = device.CreateBuffer(new BufferDescription(vertexBufferSize * sizeof(DebugDrawVert), BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            indexBuffer = device.CreateBuffer(new BufferDescription(indexBufferSize * sizeof(uint), BindFlags.IndexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            constantBuffer = device.CreateBuffer(new BufferDescription(sizeof(Matrix4x4), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));

            CreateFontsTexture();
        }

        private static unsafe void CreateFontsTexture()
        {
            uint* pixels = stackalloc uint[] { 0xffffffff };
            int width = 1;
            int height = 1;

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

        public void BeginDraw()
        {
            DebugDraw.NewFrame();
        }

        public void EndDraw()
        {
            DebugDraw.Render();
            context.SetViewport(DebugDraw.GetViewport());
            Render(DebugDraw.GetImmediateCommandList(), DebugDraw.GetCamera());
        }

        private void Render(DebugDrawCommandList queue, Matrix4x4 camera)
        {
            if (!Application.InEditorMode)
            {
                return;
            }

#if DEBUG
            context.BeginEvent("DebugDrawRenderer");
#endif

            if (queue.VertexCount > vertexBufferSize)
            {
                vertexBuffer.Dispose();
                vertexBufferSize = (int)(queue.VertexCount * 1.5f);
                vertexBuffer = device.CreateBuffer(new BufferDescription(vertexBufferSize * sizeof(DebugDrawVert), BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            }

            if (queue.IndexCount > indexBufferSize)
            {
                indexBuffer.Dispose();
                indexBufferSize = (int)(queue.IndexCount * 1.5f);
                indexBuffer = device.CreateBuffer(new BufferDescription(indexBufferSize * sizeof(uint), BindFlags.IndexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            }

            var vertexResource = context.Map(vertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var indexResource = context.Map(indexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var vertexResourcePointer = (DebugDrawVert*)vertexResource.PData;
            var indexResourcePointer = (uint*)indexResource.PData;

            MemcpyT(queue.Vertices, vertexResourcePointer, queue.VertexCount);
            MemcpyT(queue.Indices, indexResourcePointer, queue.IndexCount);

            context.Unmap(vertexBuffer, 0);
            context.Unmap(indexBuffer, 0);

            context.Write(constantBuffer, Matrix4x4.Transpose(camera));

            {
                context.VSSetConstantBuffer(0, constantBuffer);
                context.SetVertexBuffer(vertexBuffer, (uint)sizeof(DebugDrawVert));
                context.SetIndexBuffer(indexBuffer, Format.R32UInt, 0);
                context.SetGraphicsPipeline(pipeline);
                context.VSSetConstantBuffer(0, constantBuffer);
                context.PSSetSampler(0, fontSampler);

                int voffset = 0;
                uint ioffset = 0;

                for (int i = 0; i < queue.Commands.Count; i++)
                {
                    var cmd = queue.Commands[i];

                    var texId = cmd.TextureId;
                    if (texId == 0)
                    {
                        texId = fontTextureView.NativePointer;
                    }
                    context.PSSetShaderResources(0, 1, (void**)&texId);
                    context.SetPrimitiveTopology(cmd.Topology);
                    context.DrawIndexedInstanced(cmd.IndexCount, 1, ioffset, voffset, 0);
                    voffset += (int)cmd.VertexCount;
                    ioffset += cmd.IndexCount;
                }
            }

            context.SetGraphicsPipeline(null);
            context.SetViewport(default);
            context.SetVertexBuffer(null, 0, 0);
            context.SetIndexBuffer(null, default, 0);
            context.SetPrimitiveTopology(PrimitiveTopology.Undefined);
            context.VSSetConstantBuffer(0, null);
            context.PSSetSampler(0, null);
            context.PSSetShaderResource(0, null);

#if DEBUG
            context.EndEvent();
#endif
        }

        private void SetupRenderState(DebugDrawData data)
        {
            context.SetViewport(data.Viewport);
            context.VSSetConstantBuffer(0, constantBuffer);
            context.SetVertexBuffer(vertexBuffer, (uint)sizeof(DebugDrawVert));
            context.SetIndexBuffer(indexBuffer, Format.R32UInt, 0);
            context.SetGraphicsPipeline(pipeline);
            context.VSSetConstantBuffer(0, constantBuffer);
            context.PSSetSampler(0, fontSampler);
        }

        private void Render(DebugDrawData data)
        {
            if (!Application.InEditorMode || data.Viewport.Width <= 0 || data.Viewport.Height <= 0)
            {
                return;
            }
#if DEBUG
            context.BeginEvent("DebugDrawRenderer");
#endif

            if (data.TotalVertices > vertexBufferSize)
            {
                vertexBuffer.Dispose();
                vertexBufferSize = (int)(data.TotalVertices * 1.5f);
                vertexBuffer = device.CreateBuffer(new BufferDescription(vertexBufferSize * sizeof(DebugDrawVert), BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            }

            if (data.TotalIndices > indexBufferSize)
            {
                indexBuffer.Dispose();
                indexBufferSize = (int)(data.TotalIndices * 1.5f);
                indexBuffer = device.CreateBuffer(new BufferDescription(indexBufferSize * sizeof(uint), BindFlags.IndexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            }

            var vertexResource = context.Map(vertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var indexResource = context.Map(indexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var vertexResourcePointer = (DebugDrawVert*)vertexResource.PData;
            var indexResourcePointer = (uint*)indexResource.PData;

            for (int i = 0; i < data.CmdLists.Count; i++)
            {
                var list = data.CmdLists[i];
                MemcpyT(list.Vertices, vertexResourcePointer, list.VertexCount);
                MemcpyT(list.Indices, indexResourcePointer, list.IndexCount);
                vertexResourcePointer += list.VertexCount;
                indexResourcePointer += list.IndexCount;
            }

            context.Unmap(vertexBuffer, 0);
            context.Unmap(indexBuffer, 0);

            context.Write(constantBuffer, Matrix4x4.Transpose(data.Camera));

            SetupRenderState(data);

            for (int i = 0; i < data.CmdLists.Count; i++)
            {
                int voffset = 0;
                uint ioffset = 0;
                var list = data.CmdLists[i];
                for (int j = 0; j < list.Commands.Count; j++)
                {
                    var cmd = list.Commands[j];

                    var texId = cmd.TextureId;
                    context.PSSetShaderResources(0, 1, (void**)&texId);
                    context.SetPrimitiveTopology(cmd.Topology);
                    context.DrawIndexedInstanced(cmd.IndexCount, 1, ioffset, voffset, 0);
                    voffset += (int)cmd.VertexCount;
                    ioffset += cmd.IndexCount;
                }
            }

            context.SetGraphicsPipeline(null);
            context.SetViewport(default);
            context.SetVertexBuffer(null, 0, 0);
            context.SetIndexBuffer(null, default, 0);
            context.SetPrimitiveTopology(PrimitiveTopology.Undefined);
            context.VSSetConstantBuffer(0, null);
            context.PSSetSampler(0, null);
            context.PSSetShaderResource(0, null);

#if DEBUG
            context.EndEvent();
#endif
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline.Dispose();
                constantBuffer.Dispose();
                vertexBuffer.Dispose();
                indexBuffer.Dispose();
                disposedValue = true;
            }
        }

        ~DebugDrawRenderer()
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
    }
}