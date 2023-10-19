namespace HexaEngine.Editor.ImagePainter.Tools
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.ImGuiNET;
    using System.Numerics;

    public class ColorGrabber : Tool
    {
        private IComputePipeline computePipeline;
        private ConstantBuffer<Vector4> mousePosBuffer;
        private StructuredUavBuffer<Vector4> resultBuffer;

        public override string Icon => "\xEF3C##ColorGrabber";

        public override string Name => "ColorGrabber";

        public override ToolFlags Flags { get; } = ToolFlags.NoEdit;

        public override void Init(IGraphicsDevice device)
        {
            computePipeline = device.CreateComputePipeline(new()
            {
                Path = "tools/image/grabber/cs.hlsl"
            });
            mousePosBuffer = new(device, CpuAccessFlags.Write);
            resultBuffer = new(device, 1, CpuAccessFlags.Read);
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
                context.SetComputePipeline(computePipeline);
                context.CSSetShaderResource(0, toolContext.ImageSource.SRV);
                context.CSSetUnorderedAccessView(0, (void*)resultBuffer.UAV.NativePointer);
                context.CSSetConstantBuffer(0, mousePosBuffer);
                context.Dispatch(1, 1, 1);
                context.SetComputePipeline(null);
                context.CSSetShaderResource(0, null);
                context.CSSetUnorderedAccessView(0, null);
                context.CSSetConstantBuffer(0, null);
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