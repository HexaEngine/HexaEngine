namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class SelectionPipeline : GraphicsPipeline
    {
        public SelectionPipeline(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "forward/selection/vs.hlsl",
            PixelShader = "forward/selection/ps.hlsl"
        },
        new InputElementDescription[]
        {
            new InputElementDescription("POSITION",0, Format.RGB32Float,0),
            new InputElementDescription("TEXCOORD",0, Format.RG32Float,0),
            new InputElementDescription("NORMAL",0, Format.RGB32Float,0),
            new InputElementDescription("TANGENT",0, Format.RGB32Float,0),
        })
        {
            State = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };
        }
    }
}