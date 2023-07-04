using HexaEngine.Scenes.Components.Renderer;

namespace HexaEngine.Rendering
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Terrains;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Resources;
    using System.Numerics;

    public class TerrainRenderer : IDisposable
    {
        private readonly ConstantBuffer<Matrix4x4> worldBuffer;
        private readonly ResourceRef<IBuffer> camera;
        private readonly IGraphicsPipeline geometry;
        private readonly IGraphicsPipeline depth;
        private readonly IGraphicsPipeline psm;
        private readonly IGraphicsPipeline osm;
        private readonly IGraphicsPipeline csm;

        private TerrainGrid? grid;

        private bool initialized;
        private bool disposedValue;

        public TerrainRenderer(IGraphicsDevice device)
        {
            worldBuffer = new(device, CpuAccessFlags.Write);
            camera = ResourceManager2.Shared.GetBuffer("CBCamera");

            GraphicsPipelineDesc pipelineDesc = new()
            {
                VertexShader = "deferred/terrain/vs.hlsl",
                PixelShader = "deferred/terrain/ps.hlsl"
            };

            GraphicsPipelineState pipelineState = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            geometry = device.CreateGraphicsPipeline(pipelineDesc, pipelineState, Terrain.InputElements);

            pipelineDesc.PixelShader = null;
            depth = device.CreateGraphicsPipeline(pipelineDesc, pipelineState, Terrain.InputElements);

            var csmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "forward/terrain/csm/vs.hlsl",
                GeometryShader = "forward/terrain/csm/gs.hlsl",
            };
            var csmPipelineState = new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            var osmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "forward/terrain/osm/vs.hlsl",
                GeometryShader = "forward/terrain/osm/gs.hlsl",
                PixelShader = "forward/terrain/osm/ps.hlsl",
            };
            var osmPipelineState = new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            var psmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "forward/terrain/psm/vs.hlsl",
                PixelShader = "forward/terrain/psm/ps.hlsl",
            };
            var psmPipelineState = new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullFront,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            csm = device.CreateGraphicsPipeline(csmPipelineDesc, csmPipelineState, Terrain.InputElements);
            osm = device.CreateGraphicsPipeline(osmPipelineDesc, osmPipelineState, Terrain.InputElements);
            psm = device.CreateGraphicsPipeline(psmPipelineDesc, psmPipelineState, Terrain.InputElements);
        }

        public void Initialize(TerrainGrid grid)
        {
            this.grid = grid;

            initialized = true;
        }

        public void Uninitialize()
        {
            initialized = false;

            grid = null;
        }

        public unsafe void Update(Matrix4x4 transform)
        {
            if (!initialized)
                return;
            for (int i = 0; i < grid.Count; i++)
            {
                grid[i].Transform = transform * Matrix4x4.CreateTranslation(grid[i].Offset);
            }
        }

        public void Draw(IGraphicsContext context)
        {
            if (!initialized)
                return;

            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                unsafe
                {
                    *worldBuffer.Local = Matrix4x4.Transpose(cell.Transform);
                    worldBuffer.Update(context);
                }

                context.SetVertexBuffer(cell.VertexBuffer, cell.Stride);
                context.SetIndexBuffer(cell.IndexBuffer, Format.R32UInt, 0);
                context.VSSetConstantBuffer(0, worldBuffer);
                context.VSSetConstantBuffer(1, camera.Value);
                context.SetGraphicsPipeline(geometry);
                context.DrawIndexedInstanced(cell.Terrain.IndicesCount, 1, 0, 0, 0);
            }
        }

        public void DrawDepth(IGraphicsContext context)
        {
            if (!initialized)
                return;
            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                context.SetVertexBuffer(cell.VertexBuffer, cell.Stride);
                context.SetIndexBuffer(cell.IndexBuffer, Format.R32UInt, 0);
                context.VSSetConstantBuffer(0, worldBuffer);
                context.VSSetConstantBuffer(1, camera.Value);
                context.SetGraphicsPipeline(depth);
                context.DrawIndexedInstanced(cell.Terrain.IndicesCount, 1, 0, 0, 0);
            }
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!initialized)
                return;
            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                unsafe
                {
                    *worldBuffer.Local = Matrix4x4.Transpose(cell.Transform);
                    worldBuffer.Update(context);
                }

                context.SetVertexBuffer(cell.VertexBuffer, cell.Stride);
                context.SetIndexBuffer(cell.IndexBuffer, Format.R32UInt, 0);
                context.VSSetConstantBuffer(0, worldBuffer);
                context.DSSetConstantBuffer(1, light);
                context.VSSetConstantBuffer(1, light);
                context.GSSetConstantBuffer(1, light);
                switch (type)
                {
                    case ShadowType.Perspective:
                        if (!psm.IsValid)
                        {
                            return;
                        }
                        context.SetGraphicsPipeline(psm);
                        break;

                    case ShadowType.Cascaded:
                        if (!csm.IsValid)
                        {
                            return;
                        }
                        context.SetGraphicsPipeline(csm);
                        break;

                    case ShadowType.Omni:
                        if (!osm.IsValid)
                        {
                            return;
                        }
                        context.SetGraphicsPipeline(osm);
                        break;
                }
                context.DrawIndexedInstanced(cell.Terrain.IndicesCount, 1, 0, 0, 0);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                worldBuffer?.Dispose();
                geometry?.Dispose();
                depth?.Dispose();
                csm?.Dispose();
                osm?.Dispose();
                psm?.Dispose();
                disposedValue = true;
            }
        }

        ~TerrainRenderer()
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