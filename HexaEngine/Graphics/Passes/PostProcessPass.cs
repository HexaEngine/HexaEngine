namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Effects.BuildIn;
    using HexaEngine.Graph;
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
            postProcessingManager.Add<VelocityBuffer>();
            postProcessingManager.Add<TemporalNoise>();
            postProcessingManager.Add<HBAO>();
            postProcessingManager.Add<VolumetricClouds>();
            postProcessingManager.Add<SSR>();
            postProcessingManager.Add<SSGI>();
            postProcessingManager.Add<VolumetricLighting>();
            postProcessingManager.Add<MotionBlur>();
            postProcessingManager.Add<DepthOfField>();
            postProcessingManager.Add<AutoExposure>();
            postProcessingManager.Add<Bloom>();
            postProcessingManager.Add<LensFlare>();
            postProcessingManager.Add<VolumetricScattering>();
            postProcessingManager.Add<ColorGrading>();
            postProcessingManager.Add<TAA>();
            postProcessingManager.Add<FXAA>();
            postProcessingManager.Add<ChromaticAberration>();
            postProcessingManager.Add<UserLUT>();
            postProcessingManager.Add<Grain>();
            postProcessingManager.Add<Vignette>();
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
        }

        public override void Release()
        {
            postProcessingManager.Dispose();
        }
    }
}