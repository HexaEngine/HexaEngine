namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Specifies flags that control pipeline state behavior.
    /// </summary>
    /// <remarks>This enumeration supports bitwise combination of its values. Use these flags to modify how
    /// the pipeline is configured or executed.</remarks>
    [Flags]
    public enum PipelineStateFlags : uint
    {
        /// <summary>
        /// Represents the absence of a value or a default state.
        /// </summary>
        None,

        /// <summary>
        /// Enables SetVariable functions and automatic constant buffer handling.
        /// </summary>
        ReflectVariables = 1 << 0,
    }
}