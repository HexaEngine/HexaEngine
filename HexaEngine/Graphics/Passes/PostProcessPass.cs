namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Effects.BuildIn;
    using HexaEngine.Graph;
    using Hexa.NET.ImGui;
    using HexaEngine.PostFx;
    using HexaEngine.PostFx.BuildIn;
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
            postProcessingManager = new(device, creator, (int)viewport.Width, (int)viewport.Height, 4, false);
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
            postProcessingManager.Add(new ColorGrading());
            postProcessingManager.Add(new TAA());
            postProcessingManager.Add(new FXAA());
            postProcessingManager.Add(new ChromaticAberration());
            postProcessingManager.Add(new UserLUT());
            postProcessingManager.Add(new Grain());
            postProcessingManager.Add(new Vignette());
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
            var tex = postProcessingManager.DebugTextures;

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
            if (postProcessingManager.Debug)
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    var size = ImGui.GetWindowContentRegionMax();

                    var texture = tex[i];

                    if (texture.SRV != null)
                    {
                        float aspect = texture.Viewport.Height / texture.Viewport.Width;
                        size.X = MathF.Min(texture.Viewport.Width, size.X);
                        size.Y = texture.Viewport.Height;
                        var dx = texture.Viewport.Width - size.X;
                        if (dx > 0)
                        {
                            size.Y = size.X * aspect;
                        }

                        if (ImGui.CollapsingHeader(effects[i].Name))
                        {
                            ImGui.Image(texture.SRV.NativePointer, size);
                        }
                    }
                }
            }
            ImGui.End();
        }

        public override void Release()
        {
            postProcessingManager.Dispose();
        }
    }
}