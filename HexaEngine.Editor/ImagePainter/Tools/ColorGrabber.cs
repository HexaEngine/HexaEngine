namespace HexaEngine.Editor.ImagePainter.Tools
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System.Numerics;

    public class ColorGrabber : Tool
    {
        private IComputePipelineState computePipeline;
        private ConstantBuffer<Vector4> mousePosBuffer;
        private StructuredUavBuffer<Vector4> resultBuffer;

        public override string Icon => UwU.EyeDropper + "##ColorGrabber";

        public override string Name => "ColorGrabber";

        public override ToolFlags Flags { get; } = ToolFlags.NoEdit;

        public override IResourceBindingList Bindings => computePipeline.Bindings;

        public override void Init(IGraphicsDevice device)
        {
            computePipeline = device.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Path = "tools/image/grabber/cs.hlsl"
            });
            mousePosBuffer = new(CpuAccessFlags.Write);
            resultBuffer = new(1, CpuAccessFlags.Read);
            computePipeline.Bindings.SetCBV("PositionBuffer", mousePosBuffer);
            computePipeline.Bindings.SetUAV("output", resultBuffer.UAV);
        }

        public override void DrawSettings()
        {
        }

        public override unsafe void Draw(IGraphicsContext context, ToolContext toolContext)
        {
            var pixel = *resultBuffer.Local;
            toolContext.BrushColor = pixel;
        }

        public override unsafe void DrawPreview(IGraphicsContext context, ToolContext toolContext)
        {
            if (ImGui.BeginTooltip())
            {
                mousePosBuffer.Update(context, new(toolContext.Position, 0, 0));
                computePipeline.Bindings.SetSRV("tex", toolContext.ImageSource.SRV);
                context.SetComputePipelineState(computePipeline);
                context.Dispatch(1, 1, 1);
                context.SetComputePipelineState(null);
                resultBuffer.Read(context);
                var pixel = *resultBuffer.Local;

                ImGui.ColorEdit4("##ColorGrabber", ref pixel, ImGuiColorEditFlags.NoLabel);

                ImGui.EndTooltip();
            }
        }

        public override void Dispose()
        {
            computePipeline.Dispose();
            mousePosBuffer.Dispose();
            resultBuffer.Dispose();
        }
    }
}