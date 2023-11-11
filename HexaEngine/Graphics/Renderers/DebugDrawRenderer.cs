using HexaEngine.Editor;

namespace HexaEngine.Rendering.Renderers
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using System;
    using System.Numerics;

    public unsafe class DebugDrawRenderer : IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly ISwapChain swapChain;
        private readonly IGraphicsContext context;
        private readonly IGraphicsPipeline pipeline;
        private readonly IBuffer constantBuffer;

        private IBuffer vertexBuffer;
        private IBuffer indexBuffer;

        private int vertexBufferSize = 5000;
        private int indexBufferSize = 10000;
        private bool disposedValue;

        public DebugDrawRenderer(IGraphicsDevice device, ISwapChain swapChain)
        {
            this.device = device;
            this.swapChain = swapChain;

            context = device.Context;
            var desc = RasterizerDescription.CullNone;
            desc.AntialiasedLineEnable = true;
            desc.MultisampleEnable = false;

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "internal/debugdraw/vs.hlsl",
                PixelShader = "internal/debugdraw/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Blend = BlendDescription.NonPremultiplied,
                Rasterizer = desc,
                BlendFactor = Vector4.One,
                SampleMask = int.MaxValue,
            },
            new InputElementDescription[]
            {
                new("POSITION", 0, Format.R32G32B32Float, 0),
                new("TEXCOORD",0, Format.R32G32Float, 0),
                new("COLOR", 0, Format.R8G8B8A8UNorm, 0),
            });

            vertexBuffer = device.CreateBuffer(new BufferDescription(vertexBufferSize * sizeof(DebugDrawVert), BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            indexBuffer = device.CreateBuffer(new BufferDescription(indexBufferSize * sizeof(uint), BindFlags.IndexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            constantBuffer = device.CreateBuffer(new BufferDescription(sizeof(Matrix4x4), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
        }

        public void BeginDraw()
        {
            DebugDraw.NewFrame();
        }

        public void EndDraw()
        {
            DebugDraw.Render();
            context.SetRenderTarget(swapChain.BackbufferRTV, null);
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

            {
                context.Write(constantBuffer, Matrix4x4.Transpose(camera));

                context.VSSetConstantBuffer(0, constantBuffer);
                context.SetVertexBuffer(vertexBuffer, (uint)sizeof(DebugDrawVert));
                context.SetIndexBuffer(indexBuffer, Format.R32UInt, 0);
                pipeline.BeginDraw(context);

                int voffset = 0;
                uint ioffset = 0;

                for (int i = 0; i < queue.Commands.Count; i++)
                {
                    var cmd = queue.Commands[i];

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

        private void SetupRenderState(DebugDrawData data)
        {
            context.SetViewport(data.Viewport);
            context.VSSetConstantBuffer(0, constantBuffer);
            context.SetVertexBuffer(vertexBuffer, (uint)sizeof(DebugDrawVert));
            context.SetIndexBuffer(indexBuffer, Format.R32UInt, 0);
            context.SetGraphicsPipeline(pipeline);
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