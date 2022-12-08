namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Rendering;
    using ImGuiNET;

    public class DFGLUTWidget : ImGuiWindow
    {
        private Quad quad;
        private Pipeline pipeline;
        private ITexture2D tex;
        private IRenderTargetView rtv;
        private IShaderResourceView srv;
        private bool renderContinues;

        protected override string Name => "DFG LUT";

        public override void Init(IGraphicsDevice device)
        {
            quad = new(device);
            pipeline = new(device, new()
            {
                VertexShader = "effects/brdf/vs.hlsl",
                PixelShader = "effects/brdf/ps.hlsl"
            });
            tex = device.CreateTexture2D(Format.RGBA32Float, 512, 512, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            rtv = device.CreateRenderTargetView(tex, new(512, 512));
            srv = device.CreateShaderResourceView(tex);
            base.Init(device);
        }

        public override void DrawContent(IGraphicsContext context)
        {
            if (ImGui.Button("Render"))
            {
                context.SetRenderTarget(rtv, null);
                quad.DrawAuto(context, pipeline, rtv.Viewport);
                context.ClearState();
            }

            ImGui.Checkbox("Render Continues", ref renderContinues);

            if (renderContinues)
            {
                context.SetRenderTarget(rtv, null);
                quad.DrawAuto(context, pipeline, rtv.Viewport);
                context.ClearState();
            }

            ImGui.Image(srv.NativePointer, new(512, 512));
        }

        public override void Dispose()
        {
            tex.Dispose();
            srv.Dispose();
            rtv.Dispose();
            pipeline.Dispose();
            base.Dispose();
        }
    }
}