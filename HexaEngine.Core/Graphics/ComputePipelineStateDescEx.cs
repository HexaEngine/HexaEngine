namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a combined description of a compute pipeline and its state.
    /// </summary>
    public struct ComputePipelineStateDescEx : IEquatable<ComputePipelineStateDescEx>
    {
        /// <summary>
        /// The description of the compute pipeline (compute shader path/entry and macros).
        /// </summary>
        public ComputePipelineDesc Pipeline;

        /// <summary>
        /// The description of the compute pipeline state (flags that affect pipeline behavior).
        /// </summary>
        public ComputePipelineStateDesc State;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputePipelineStateDescEx"/> struct.
        /// </summary>
        /// <param name="pipeline">The description of the compute pipeline.</param>
        /// <param name="state">The description of the compute pipeline state.</param>
        public ComputePipelineStateDescEx(ComputePipelineDesc pipeline, ComputePipelineStateDesc state)
        {
            Pipeline = pipeline;
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputePipelineStateDescEx"/> struct with default state description.
        /// </summary>
        /// <param name="pipeline">The description of the compute pipeline.</param>
        public ComputePipelineStateDescEx(ComputePipelineDesc pipeline)
        {
            Pipeline = pipeline;
            State = ComputePipelineStateDesc.Default;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is ComputePipelineStateDescEx ex && Equals(ex);
        }

        /// <inheritdoc/>
        public readonly bool Equals(ComputePipelineStateDescEx other)
        {
            return Pipeline.Equals(other.Pipeline) &&
                   State.Equals(other.State);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Pipeline, State);
        }

        /// <summary>
        /// Checks whether two <see cref="ComputePipelineStateDescEx"/> instances are equal.
        /// </summary>
        public static bool operator ==(ComputePipelineStateDescEx left, ComputePipelineStateDescEx right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks whether two <see cref="ComputePipelineStateDescEx"/> instances are not equal.
        /// </summary>
        public static bool operator !=(ComputePipelineStateDescEx left, ComputePipelineStateDescEx right)
        {
            return !(left == right);
        }
    }
}