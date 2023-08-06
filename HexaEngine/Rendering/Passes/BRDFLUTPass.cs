namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Rendering.Graph;

    public class BRDFLUTPass : RenderPass
    {
        public BRDFLUTPass() : base("BRDFLUT")
        {
        }

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device, ICPUProfiler? profiler)
        {
            creator.CreateTexture2D("BRDFLUT", new(Format.R16G16B16A16Float, 128, 128, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));
        }
    }
}