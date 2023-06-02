namespace HexaEngine.Rendering
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Scenes.Managers;
    using System;

    public class TerrainRenderer
    {
        private IGraphicsPipeline pipeline;

        public TerrainRenderer()
        {
        }

        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Geometry;

        public void Initialize(IGraphicsDevice device)
        {
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "deferred/terrain/vs.hlsl",
                HullShader = "deferred/terrain/hs.hlsl",
                DomainShader = "deferred/terrain/ds.hlsl",
                PixelShader = "deferred/terrain/ps.hlsl"
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            });
        }

        public void Draw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void DrawDepth(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void DrawIndirect(IGraphicsContext context, IBuffer drawArgs, uint offset)
        {
            throw new NotImplementedException();
        }

        public void Uninitialize()
        {
            throw new NotImplementedException();
        }

        public void VisibilityTest(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }
}