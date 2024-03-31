namespace HexaEngine.Dummy
{
    using HexaEngine.Core.Graphics;

    public class DummyComputePipeline : DisposableBase, IComputePipeline
    {
        public DummyComputePipeline(ComputePipelineDesc desc)
        {
            Desc = desc;
        }

        public string DebugName { get; }

        public ShaderMacro[]? Macros { get; set; }

        public ComputePipelineDesc Desc { get; }

        public bool IsInitialized { get; }

        public bool IsValid { get; }

        public void BeginDispatch(IGraphicsContext context)
        {
        }

        public void Dispatch(IGraphicsContext context, uint x, uint y, uint z)
        {
        }

        public void EndDispatch(IGraphicsContext context)
        {
        }

        public void Recompile()
        {
        }

        protected override void DisposeCore()
        {
        }
    }
}