namespace HexaEngine.Editor.Painting.Tools
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using ImGuiNET;
    using System.Numerics;

    public class Pencil : Tool
    {
        private Vector2 brushSize = Vector2.One;
        private Quad quad;
        private IGraphicsPipeline brushPipeline;

        public override string Icon => "\xED63##PencilTool";

        public override string Name => "Pencil";

        public override void Init(IGraphicsDevice device)
        {
            quad = new(device);
            brushPipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/brush/vs.hlsl",
                PixelShader = "effects/brush/ps.hlsl",
            }, new GraphicsPipelineState()
            {
                Blend = BlendDescription.Additive,
                BlendFactor = Vector4.Zero,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList,
                SampleMask = int.MaxValue,
                StencilRef = 0,
            });
        }

        public override void DrawSettings()
        {
            ImGui.InputFloat2("Size", ref brushSize);
        }

        public override void Draw(Vector2 position, IGraphicsContext context)
        {
            context.SetViewport(new(position - brushSize / 2f, brushSize));
            quad.DrawAuto(context, brushPipeline);
        }

        public override void DrawPreview(Vector2 position, IGraphicsContext context)
        {
            context.SetViewport(new(position - brushSize / 2f, brushSize));
            quad.DrawAuto(context, brushPipeline);
        }

        public override void Dispose()
        {
            brushPipeline.Dispose();
            quad.Dispose();
        }
    }
}