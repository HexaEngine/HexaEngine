namespace HexaEngine.Editor.ImagePainter.Tools
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.ImagePainter;
    using System.Numerics;

    public class Eraser : Tool
    {
        private Vector2 brushSize = Vector2.One;
        private float opacity = 1;
        private IGraphicsPipelineState brushPipeline;
        private ConstantBuffer<Vector4> opacityBuffer;

        public override string Icon => UwU.Eraser + "##EraserTool";

        public override string Name => "Eraser";

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
            BlendDescription blendDescription = new(Blend.SourceAlpha, Blend.InverseSourceAlpha, Blend.SourceAlpha, Blend.InverseSourceAlpha);
            brushPipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "tools/image/eraser/ps.hlsl",
            }, new GraphicsPipelineStateDesc()
            {
                Blend = blendDescription,
                BlendFactor = Vector4.Zero,
                DepthStencil = depthStencil,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleStrip,
            });
            opacityBuffer = new(CpuAccessFlags.Write);
        }

        public override void DrawSettings()
        {
            ImGui.SliderFloat("Opacity", ref opacity, 0, 1);
            ImGui.InputFloat2("Size", ref brushSize);
        }

        public override void Draw(IGraphicsContext context, ToolContext toolContext)
        {
            opacityBuffer.Update(context, new(opacity));

            context.PSSetConstantBuffer(1, opacityBuffer);
            context.SetViewport(toolContext.ComputeViewport(brushSize));
            context.SetPipelineState(brushPipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);
            context.PSSetConstantBuffer(1, null);
        }

        public override void DrawPreview(IGraphicsContext context, ToolContext toolContext)
        {
            opacityBuffer.Update(context, new(opacity));

            context.PSSetConstantBuffer(1, opacityBuffer);
            context.SetViewport(toolContext.ComputeViewport(brushSize));
            context.SetPipelineState(brushPipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);
            context.PSSetConstantBuffer(1, null);
        }

        public override void Dispose()
        {
            brushPipeline.Dispose();
            opacityBuffer.Dispose();
        }
    }
}