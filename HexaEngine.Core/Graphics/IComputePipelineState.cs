namespace HexaEngine.Core.Graphics
{
    public interface IComputePipelineState : IPipelineState
    {
        /// <summary>
        /// Gets the pipeline of the compute pipeline state.
        /// </summary>
        public IComputePipeline Pipeline { get; }
    }
}