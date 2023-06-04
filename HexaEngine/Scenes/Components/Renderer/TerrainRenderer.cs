namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Terrains;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using System.Numerics;

    [EditorComponent<TerrainRenderer>("Terrain", false, true)]
    public class TerrainRenderer : IRendererComponent
    {
        private GameObject gameObject;
        private Terrain terrain;
        private HeightMap heightMap;
        private IBuffer VertexBuffer;
        private IBuffer IndexBuffer;
        private uint Stride;
        private ConstantBuffer<Matrix4x4> WorldBuffer;
        private ResourceRef<IBuffer> camera;
        private IGraphicsPipeline geometry;
        private IGraphicsPipeline depth;
        private IGraphicsPipeline psm;
        private IGraphicsPipeline osm;
        private IGraphicsPipeline csm;

        private bool drawable;

        public TerrainRenderer()
        {
        }

        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Geometry;

        public RendererFlags Flags { get; } = RendererFlags.All;

        [EditorProperty("HeightMap", null)]
        public string HeightMap { get; } = "";

        public async void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;
            heightMap = new(32, 32);
            heightMap.GenerateEmpty();
            terrain = new(heightMap);
            VertexBuffer = terrain.CreateVertexBuffer(device);
            IndexBuffer = terrain.CreateIndexBuffer(device);
            Stride = terrain.GetStride();
            WorldBuffer = new(device, CpuAccessFlags.Write);
            camera = ResourceManager2.Shared.GetBuffer("CBCamera");

            var inputElements = terrain.GetInputElements();
            var macros = terrain.GetShaderMacros();

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

            geometry = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, inputElements, macros);

            pipelineDesc.PixelShader = null;
            depth = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, inputElements, macros);

            var csmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "deferred/terrain/csm/vs.hlsl",
                GeometryShader = "deferred/terrain/csm/gs.hlsl",
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
                VertexShader = "deferred/terrain/osm/vs.hlsl",
                GeometryShader = "deferred/terrain/osm/gs.hlsl",
                PixelShader = "deferred/terrain/osm/ps.hlsl",
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
                VertexShader = "deferred/terrain/psm/vs.hlsl",
                PixelShader = "deferred/terrain/psm/ps.hlsl",
            };
            var psmPipelineState = new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullFront,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            csm = await device.CreateGraphicsPipelineAsync(csmPipelineDesc, csmPipelineState, inputElements, macros);
            osm = await device.CreateGraphicsPipelineAsync(osmPipelineDesc, osmPipelineState, inputElements, macros);
            psm = await device.CreateGraphicsPipelineAsync(psmPipelineDesc, psmPipelineState, inputElements, macros);

            Volatile.Write(ref drawable, true);
        }

        public void Destory()
        {
            Volatile.Write(ref drawable, false);
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
            WorldBuffer?.Dispose();
            geometry?.Dispose();
            depth?.Dispose();
            csm?.Dispose();
            osm?.Dispose();
            psm?.Dispose();
        }

        public unsafe void Update(IGraphicsContext context)
        {
            if (!drawable)
                return;
            *WorldBuffer.Local = Matrix4x4.Transpose(gameObject.Transform.Global);
            WorldBuffer.Update(context);
        }

        public void DrawDepth(IGraphicsContext context, IBuffer camera)
        {
            if (!drawable)
                return;
            context.SetVertexBuffer(VertexBuffer, Stride);
            context.SetIndexBuffer(IndexBuffer, Format.R32UInt, 0);
            context.VSSetConstantBuffer(WorldBuffer, 0);
            context.VSSetConstantBuffer(camera, 1);
            context.SetGraphicsPipeline(depth);
            context.DrawIndexedInstanced(terrain.IndicesCount, 1, 0, 0, 0);
        }

        public void DrawShadows(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!drawable)
                return;
            context.SetVertexBuffer(VertexBuffer, Stride);
            context.SetIndexBuffer(IndexBuffer, Format.R32UInt, 0);
            context.VSSetConstantBuffer(WorldBuffer, 0);
            context.DSSetConstantBuffer(light, 1);
            context.VSSetConstantBuffer(light, 1);
            context.GSSetConstantBuffer(light, 1);
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
            context.DrawIndexedInstanced(terrain.IndicesCount, 1, 0, 0, 0);
        }

        public void Draw(IGraphicsContext context)
        {
            if (!drawable)
                return;
            context.SetVertexBuffer(VertexBuffer, Stride);
            context.SetIndexBuffer(IndexBuffer, Format.R32UInt, 0);
            context.VSSetConstantBuffer(WorldBuffer, 0);
            context.VSSetConstantBuffer(camera.Value, 1);
            context.SetGraphicsPipeline(geometry);
            context.DrawIndexedInstanced(terrain.IndicesCount, 1, 0, 0, 0);
        }

        public void VisibilityTest(IGraphicsContext context)
        {
        }
    }
}