namespace HexaEngine.Core.Graphics
{
    public interface IComputePipeline : IDisposable
    {
        string DebugName { get; }

        void BeginDispatch(IGraphicsContext context);

        void Dispatch(IGraphicsContext context, uint x, uint y, uint z);

        void EndDispatch(IGraphicsContext context);

        void Recompile();

        ShaderMacro[]? Macros { get; set; }
        ComputePipelineDesc Desc { get; }
        bool IsInitialized { get; }
        bool IsValid { get; }
    }
}