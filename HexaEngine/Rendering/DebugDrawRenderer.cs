using HexaEngine.Editor;

namespace HexaEngine.Rendering
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using System;
    using System.Numerics;
    using System.Security.Cryptography;

    public unsafe class DebugDrawRenderer : IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly ISwapChain swapChain;
        private readonly IGraphicsContext context;
        private readonly IGraphicsPipeline pipeline;
        private readonly IBuffer constantBuffer;
        private readonly ResourceRef<IDepthStencilView> dsv;
        private readonly ResourceRef<Texture> rtv;
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
            indexBuffer = device.CreateBuffer(new BufferDescription(indexBufferSize * sizeof(int), BindFlags.IndexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            constantBuffer = device.CreateBuffer(new BufferDescription(sizeof(Matrix4x4), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));

            dsv = ResourceManager2.Shared.GetDepthStencilView("GBuffer.DepthStencil");
            rtv = ResourceManager2.Shared.GetTexture("LightBuffer");
        }

        public void BeginDraw()
        {
            DebugDraw.NewFrame();
        }

        public void EndDraw()
        {
            DebugDraw.Render();
            if (rtv.Value == null)
            {
                return;
            }
            context.SetRenderTarget(swapChain.BackbufferRTV, null);
            context.SetViewport(DebugDraw.GetViewport());
            Render(DebugDraw.GetQueue(), DebugDraw.GetCamera());
        }

        private void Render(DebugDrawCommandQueue queue, Camera? camera)
        {
            if (!Application.InEditorMode)
            {
                return;
            }

            if (camera == null)
            {
                return;
            }

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
                indexBuffer = device.CreateBuffer(new BufferDescription(indexBufferSize * sizeof(int), BindFlags.IndexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            }

            var vertexResource = context.Map(vertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var indexResource = context.Map(indexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var vertexResourcePointer = (DebugDrawVert*)vertexResource.PData;
            var indexResourcePointer = (ushort*)indexResource.PData;

            for (int i = 0; i < queue.Commands.Count; i++)
            {
                var cmd = queue.Commands[i];
                MemoryCopy(cmd.Vertices, vertexResourcePointer, cmd.nVertices);
                MemoryCopy(cmd.Indices, indexResourcePointer, cmd.nIndices);
                vertexResourcePointer += cmd.nVertices;
                indexResourcePointer += cmd.nIndices;
            }

            context.Unmap(vertexBuffer, 0);
            context.Unmap(indexBuffer, 0);

            {
                context.Write(constantBuffer, Matrix4x4.Transpose(camera.Transform.View * camera.Transform.Projection));

                context.VSSetConstantBuffer(0, constantBuffer);
                context.SetVertexBuffer(vertexBuffer, (uint)sizeof(DebugDrawVert));
                context.SetIndexBuffer(indexBuffer, Format.R16UInt, 0);
                pipeline.BeginDraw(context);

                int voffset = 0;
                uint ioffset = 0;
                bool hadDepth = false;
                for (int i = 0; i < queue.Commands.Count; i++)
                {
                    var cmd = queue.Commands[i];

                    context.SetPrimitiveTopology(cmd.Topology);
                    context.DrawIndexedInstanced(cmd.nIndices, 1, ioffset, voffset, 0);
                    voffset += (int)cmd.nVertices;
                    ioffset += cmd.nIndices;
                }
            }
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