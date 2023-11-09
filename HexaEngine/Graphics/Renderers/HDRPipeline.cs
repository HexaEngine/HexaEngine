#nullable disable

namespace HexaEngine.Rendering.Renderers
{
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Rendering.Passes;

    public class HDRPipeline : RenderGraph
    {
        public RenderPass[] Passes;

        public HDRPipeline(Windows.RendererFlags flags) : base("HDRPipeline")
        {
            BRDFLUTPass brdfLutPass = new();
            DepthPrePass depthPrePass = new();
            PostProcessPrePass postProcessPrePass = new();
            HizDepthPass hizDepthPass = new();

            ObjectCullPass objectCullPass = new();
            LightCullPass lightCullPass = new();
            ShadowMapPass shadowMapPass = new();
            GBufferPass gBufferPass = new(flags);
            LightDeferredPass lightDeferredPass = new(flags);
            LightForwardPass lightForwardPass = new(flags);
            PostProcessPass postProcessPass = new();

            brdfLutPass.Build(this);
            depthPrePass.Build(this);
            postProcessPrePass.Build(this);
            hizDepthPass.Build(this);
            objectCullPass.Build(this);
            lightCullPass.Build(this);
            shadowMapPass.Build(this);
            gBufferPass.Build(this);
            lightDeferredPass.Build(this);
            lightForwardPass.Build(this);
            postProcessPass.Build(this);

            Passes =
            [
                brdfLutPass,
                depthPrePass,
                postProcessPrePass,
                hizDepthPass,
                objectCullPass,
                lightCullPass,
                shadowMapPass,
                gBufferPass,
                lightDeferredPass,
                lightForwardPass,
                postProcessPass
            ];
        }
    }
}