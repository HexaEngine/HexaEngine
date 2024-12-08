namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a compute pipeline used for running compute shaders on a graphics device.
    /// </summary>
    public interface IComputePipeline : IPipeline
    {
        /// <summary>
        /// Gets the debug name of the compute pipeline.
        /// </summary>
        string DebugName { get; }

        /// <summary>
        /// Recompiles the compute pipeline.
        /// </summary>
        void Recompile();

        /// <summary>
        /// Gets or sets an array of shader macros used for shader compilation.
        /// </summary>
        ShaderMacro[]? Macros { get; set; }

        /// <summary>
        /// Gets the description of the compute pipeline.
        /// </summary>
        ComputePipelineDescEx Desc { get; }

        /// <summary>
        /// Gets a value indicating whether the compute pipeline has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets a value indicating whether the compute pipeline is valid and can be used.
        /// </summary>
        bool IsValid { get; }
    }
}