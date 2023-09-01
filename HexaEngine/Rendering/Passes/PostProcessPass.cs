namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Effects.BuildIn;
    using HexaEngine.Graph;
    using HexaEngine.ImGuiNET;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;

    public class PostProcessPass : DrawPass
    {
        private PostProcessingManager postProcessingManager;
        private ResourceRef<Texture2D> lightBuffer;

        public PostProcessPass() : base("PostProcess")
        {
            AddReadDependency(new("GBuffer"));
            AddReadDependency(new("LightBuffer"));
            AddReadDependency(new("#DepthStencil"));
        }

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device, ICPUProfiler? profiler)
        {
            lightBuffer = creator.GetTexture2D("LightBuffer");
            var viewport = creator.Viewport;
            postProcessingManager = new(device, creator, (int)viewport.Width, (int)viewport.Height);
            postProcessingManager.Add(new VelocityBuffer());
            postProcessingManager.Add(new HBAO());
            postProcessingManager.Add(new VolumetricClouds());
            postProcessingManager.Add(new SSR());
            postProcessingManager.Add(new SSGI());
            postProcessingManager.Add(new MotionBlur());
            postProcessingManager.Add(new DepthOfField());
            postProcessingManager.Add(new AutoExposure());
            postProcessingManager.Add(new Bloom());
            postProcessingManager.Add(new LensFlare());
            postProcessingManager.Add(new GodRays());
            postProcessingManager.Add(new Compose());
            postProcessingManager.Add(new TAA());
            postProcessingManager.Add(new FXAA());
            postProcessingManager.Initialize((int)viewport.Width, (int)viewport.Height, profiler);
            postProcessingManager.Enabled = true;
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            postProcessingManager.Input = lightBuffer.Value;
            postProcessingManager.Output = creator.Output;
            postProcessingManager.OutputTex = creator.OutputTex;
            postProcessingManager.Viewport = creator.OutputViewport;
            postProcessingManager.Draw(context, creator, profiler);

            var effects = postProcessingManager.Effects;

            if (ImGui.Begin("PostFx"))
            {
                if (ImGui.Button("Invalidate"))
                    postProcessingManager.Invalidate();
                ImGui.BeginListBox("Effects");
                for (int i = 0; i < effects.Count; i++)
                {
                    var effect = effects[i];

                    ImGui.Text($"{effect.Name},{i}");
                }
                ImGui.EndListBox();
            }
            ImGui.End();
        }
    }
}