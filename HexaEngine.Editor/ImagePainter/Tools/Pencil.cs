namespace HexaEngine.Editor.ImagePainter.Tools
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.ImagePainter;
    using System.Numerics;

    public class Pencil : Tool
    {
        private Vector2 brushSize = Vector2.One;
        private IGraphicsPipelineState brushPipeline;

        public override string Icon => UwU.Pen + "##PencilTool";

        public override string Name => "Pencil";

        public override IResourceBindingList Bindings => brushPipeline.Bindings;

        public override void Init(IGraphicsDevice device)
        {
            DepthStencilDescription depthStencil = new()
            {
                DepthEnable = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthFunc = ComparisonFunction.Less,
                StencilEnable = false,
                StencilReadMask = 255,
                StencilWriteMask = 255,
                FrontFace = DepthStencilOperationDescription.DefaultFront,
                BackFace = DepthStencilOperationDescription.DefaultBack
            };
            brushPipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "tools/image/brush/ps.hlsl",
            }, new()
            {
                Blend = BlendDescription.AlphaBlend,
                BlendFactor = Vector4.Zero,
                DepthStencil = depthStencil,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleStrip,
            });
        }

        public override void DrawSettings()
        {
            ImGui.InputFloat2("Size", ref brushSize);
        }

        public override void Draw(IGraphicsContext context, ToolContext toolContext)
        {
            context.SetViewport(toolContext.ComputeViewport(brushSize));
            context.SetGraphicsPipelineState(brushPipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
        }

        public override void DrawPreview(IGraphicsContext context, ToolContext toolContext)
        {
            context.SetViewport(toolContext.ComputeViewport(brushSize));
            context.SetGraphicsPipelineState(brushPipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
        }

        public override void Dispose()
        {
            brushPipeline.Dispose();
        }
    }
}