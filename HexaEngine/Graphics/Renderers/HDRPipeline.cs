#nullable disable

namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Graphics.Passes;

    public class HDRPipeline : RenderGraph
    {
        public HDRPipeline(Windows.RendererFlags flags) : base("HDRPipeline")
        {
            Add<BRDFLUTPass>();
            Add<DepthPrePass>();
            Add<PostProcessPrePass>();
            Add<HizDepthPass>();

            Add<ObjectCullPass>();
            Add<LightCullPass>();
            Add<ShadowMapPass>();
            Add(new GBufferPass(flags));
            Add(new LightDeferredPass(flags));
            Add(new LightForwardPass(flags));
            Add<PostProcessPass>();
            Add<OverlayPass>();
        }
    }
}