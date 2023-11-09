namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graph;
    using HexaEngine.Meshes;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using Silk.NET.Assimp;
    using System.Numerics;

    public class GBufferPass : RenderPass
    {
        private ResourceRef<GBuffer> gbuffer;
        private ResourceRef<Texture2D> lightBuffer;
        private ResourceRef<DepthStencil> depthStencil;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;

        public GBufferPass(Windows.RendererFlags flags) : base("GBuffer")
        {
            forceForward = (flags & Windows.RendererFlags.ForceForward) != 0;
            AddWriteDependency(new("#DepthStencil"));
            AddWriteDependency(new("GBuffer"));
        }

        private readonly bool forceForward = true;

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device, ICPUProfiler? profiler)
        {
            var viewport = creator.Viewport;
            gbuffer = creator.CreateGBuffer("GBuffer", new((int)viewport.Width, (int)viewport.Height, 4,
                Format.R16G16B16A16Float,   // BaseColor(RGB)   Material ID(A)
                Format.R8G8B8A8UNorm,       // Normal(XYZ)      Roughness(W)
                Format.R8G8B8A8UNorm,       // Metallic         Reflectance             AO      Material Data
                Format.R8G8B8A8UNorm        // Emission(XYZ)    Emission Strength(W)
                ));
            lightBuffer = creator.GetTexture2D("LightBuffer");
            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
        }

        public override unsafe void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var gbuffer = this.gbuffer.Value;
            var lightBuffer = this.lightBuffer.Value;
            context.ClearRenderTargetViews(gbuffer.Count, gbuffer.PRTVs, Vector4.Zero);
            context.ClearRenderTargetView(lightBuffer.RTV, Vector4.Zero);

            if (forceForward)
                return;

            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var renderers = current.RenderManager;

            var depthStencil = this.depthStencil.Value;

            context.SetRenderTargets(gbuffer.Count, gbuffer.PRTVs, depthStencil.DSV);

            context.SetViewport(gbuffer.Viewport);
            context.VSSetConstantBuffer(1, camera.Value);
            context.DSSetConstantBuffer(1, camera.Value);
            context.GSSetConstantBuffer(1, camera.Value);
            context.CSSetConstantBuffer(1, camera.Value);

            profiler?.Begin("LightDeferred.Background");
            var background = renderers.BackgroundQueue;
            for (int i = 0; i < background.Count; i++)
            {
                var renderer = background[i];
                profiler?.Begin($"LightDeferred.{renderer.DebugName}");
                renderer.Draw(context, RenderPath.Deferred);
                profiler?.End($"LightDeferred.{renderer.DebugName}");
            }
            profiler?.End("LightDeferred.Background");

            profiler?.Begin("LightDeferred.Geometry");
            var geometry = renderers.GeometryQueue;
            for (int i = 0; i < geometry.Count; i++)
            {
                var renderer = geometry[i];
                profiler?.Begin($"LightDeferred.{renderer.DebugName}");
                renderer.Draw(context, RenderPath.Deferred);
                profiler?.End($"LightDeferred.{renderer.DebugName}");
            }
            profiler?.End("LightDeferred.Geometry");

            context.ClearState();
        }
    }
}