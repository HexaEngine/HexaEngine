﻿namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;

    public unsafe class OSMPipeline : Pipeline
    {
        public IBuffer? View;
        public IBuffer? Light;

        public OSMPipeline(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "forward/osm/vs.hlsl",
            HullShader = "forward/osm/hs.hlsl",
            DomainShader = "forward/osm/ds.hlsl",
            GeometryShader = "forward/osm/gs.hlsl",
            PixelShader = "forward/osm/ps.hlsl",
        },
        new PipelineState()
        {
            DepthStencil = DepthStencilDescription.Default,
            Rasterizer = RasterizerDescription.CullFront,
            Blend = BlendDescription.Opaque,
            Topology = PrimitiveTopology.PatchListWith3ControlPoints,
        },
        new InputElementDescription[]
        {
                new("POSITION", 0, Format.RGB32Float, 0),
                new("TEXCOORD", 0, Format.RG32Float, 0),
                new("NORMAL", 0, Format.RGB32Float, 0),
                new("TANGENT", 0, Format.RGB32Float, 0),
                new("INSTANCED_MATS", 0, Format.RGBA32Float, 0, 1, InputClassification.PerInstanceData, 1),
                new("INSTANCED_MATS", 1, Format.RGBA32Float, 16, 1, InputClassification.PerInstanceData, 1),
                new("INSTANCED_MATS", 2, Format.RGBA32Float, 32, 1, InputClassification.PerInstanceData, 1),
                new("INSTANCED_MATS", 3, Format.RGBA32Float, 48, 1, InputClassification.PerInstanceData, 1),
        },
        new ShaderMacro[]
        {
            new("INSTANCED", 1)
        })
        {
        }

        protected override void BeginDraw(IGraphicsContext context, Viewport viewport)
        {
            base.BeginDraw(context, viewport);
            context.GSSetConstantBuffer(View, 0);
            context.PSSetConstantBuffer(Light, 0);
        }
    }
}