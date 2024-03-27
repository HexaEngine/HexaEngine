namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.PostFx;
    using HexaEngine.PostFx.BuildIn;
    using HexaEngine.Scenes;

    public class PostProcessPass : DrawPass
    {
        private PostProcessingManager postProcessingManager;
        private ResourceRef<Texture2D> lightBuffer;
        private ResourceRef<Texture2D> postFxBuffer;

        public PostProcessPass() : base("PostProcess")
        {
            AddReadDependency(new("GBuffer"));
            AddReadDependency(new("LightBuffer"));
            AddReadDependency(new("#DepthStencil"));
            AddWriteDependency(new("PostFxBuffer"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            lightBuffer = creator.GetTexture2D("LightBuffer");
            var viewport = creator.Viewport;
            postProcessingManager = new(creator.Device, creator, (int)viewport.Width, (int)viewport.Height, 4, PostProcessingFlags.HDR);
            postProcessingManager.Add<VelocityBuffer>();
            postProcessingManager.Add<SSAO>();
            postProcessingManager.Add<HBAO>();
            postProcessingManager.Add<VolumetricClouds>();
            postProcessingManager.Add<SSR>();
            postProcessingManager.Add<SSGI>();
            postProcessingManager.Add<Fog>();
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

            postFxBuffer = creator.CreateTexture2D("PostFxBuffer", new(Format.R16G16B16A16Float, (int)viewport.Width, (int)viewport.Height, 1, 1, BindFlags.RenderTarget | BindFlags.ShaderResource), ResourceCreationFlags.LazyInit);
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            context.ClearRenderTargetView(postFxBuffer.Value.RTV, default);
            postProcessingManager.Enabled = (SceneRenderer.Current.DrawFlags & SceneDrawFlags.NoPostProcessing) == 0;
            postProcessingManager.Input = lightBuffer.Value;
            postProcessingManager.Output = postFxBuffer.Value;
            postProcessingManager.OutputTex = postFxBuffer.Value;
            postProcessingManager.Viewport = creator.Viewport;
            postProcessingManager.Draw(context, creator, profiler);
        }

        public override void Release()
        {
            postProcessingManager.Dispose();
        }
    }
}