#nullable disable

namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Graphics.Passes;

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
            OverlayPass overlayPass = new();

            RadiosityBakePass radiosityBakePass = new();

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
            overlayPass.Build(this);

            radiosityBakePass.Build(this);

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
                postProcessPass,
                overlayPass,
                radiosityBakePass
            ];
        }
    }
}