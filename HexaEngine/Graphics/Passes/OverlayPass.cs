namespace HexaEngine.Graphics.Passes
{
    using Hexa.NET.DebugDraw;
    using Hexa.NET.FreeType;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Profiling;
    using HexaEngine.Scenes;
    using HexaEngine.UI;
    using HexaEngine.UI.Graphics;
    using System.Numerics;

    public class OverlayPass : RenderPass
    {
        private ResourceRef<IGraphicsPipelineState> copyPipeline = null!;
        private ResourceRef<ISamplerState> samplerState = null!;

        private ResourceRef<DepthStencil> depthStencil = null!;
        private ResourceRef<Texture2D> postFxBuffer = null!;

        private UIRenderer uirenderer = null!;

        public OverlayPass() : base("OverlayPass")
        {
            AddReadDependency(new("#DepthStencil"));
            AddReadDependency(new("PostFxBuffer"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            copyPipeline = creator.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    VertexShader = "quad.hlsl",
                    PixelShader = "effects/copy/ps.hlsl",
                    Macros = [new ShaderMacro("SAMPLED", 1)]
                },
                State = GraphicsPipelineStateDesc.DefaultFullscreen,
            });
            samplerState = creator.CreateSamplerState("LinearClamp", SamplerStateDescription.LinearClamp);

            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
            postFxBuffer = creator.GetTexture2D("PostFxBuffer");

            uirenderer = new(creator.Device);
        }

        public override void Prepare(GraphResourceBuilder creator)
        {
            var bindings = copyPipeline.Value!.Bindings;
            bindings.SetSRV("sourceTex", postFxBuffer.Value);
            bindings.SetSampler("samplerState", samplerState.Value);
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var enabled = (SceneRenderer.Current.DrawFlags & SceneDrawFlags.NoOverlay) == 0;

            if (Application.InEditorMode && enabled)
            {
                profiler?.Begin("DebugDraw");
                context.SetRenderTarget(postFxBuffer.Value, depthStencil.Value);
                DebugDraw.SetViewport(creator.Viewport.Offset, creator.Viewport.Size);
                profiler?.Begin("DebugDraw.EndDraw");
                DebugDrawRenderer.EndDraw();
                profiler?.End("DebugDraw.EndDraw");
                context.SetViewport(creator.Viewport);
                profiler?.Begin("DebugDraw.BeginDraw");
                DebugDrawRenderer.BeginDraw();
                profiler?.End("DebugDraw.BeginDraw");
                profiler?.End("DebugDraw");
            }

            var ui = UISystem.Current;
            if (ui != null)
            {
                var commandList = ui.CommandList;
                float L = 0;
                float R = creator.Viewport.Width;
                float T = 0;
                float B = creator.Viewport.Height;
                Matrix4x4 mvp = new
                    (
                     2.0f / (R - L), 0.0f, 0.0f, 0.0f,
                     0.0f, 2.0f / (T - B), 0.0f, 0.0f,
                     0.0f, 0.0f, 0.5f, 0.0f,
                     (R + L) / (L - R), (T + B) / (B - T), 0.5f, 1.0f
                );

                context.SetRenderTarget(postFxBuffer.Value, depthStencil.Value);
                // End the ImGui frame rendering.
                uirenderer?.RenderDrawData(context, creator.Viewport, mvp, commandList);
            }

            context.SetRenderTarget(creator.Output, null);
            context.SetViewport(creator.OutputViewport);
            context.SetGraphicsPipelineState(copyPipeline.Value);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        public override void Release()
        {
            uirenderer.Release();
        }
    }
}