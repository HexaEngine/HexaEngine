namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a combined description of a graphics pipeline and its state.
    /// </summary>
    public struct GraphicsPipelineStateDescEx : IEquatable<GraphicsPipelineStateDescEx>
    {
        /// <summary>
        /// The description of the graphics pipeline.
        /// </summary>
        public GraphicsPipelineDesc Pipeline;

        /// <summary>
        /// The description of the graphics pipeline state.
        /// </summary>
        public GraphicsPipelineStateDesc State;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPipelineStateDescEx"/> struct.
        /// </summary>
        /// <param name="pipeline">The description of the graphics pipeline.</param>
        /// <param name="state">The description of the graphics pipeline state.</param>
        public GraphicsPipelineStateDescEx(GraphicsPipelineDesc pipeline, GraphicsPipelineStateDesc state)
        {
            Pipeline = pipeline;
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPipelineStateDescEx"/> struct with default state description.
        /// </summary>
        /// <param name="pipeline">The description of the graphics pipeline.</param>
        public GraphicsPipelineStateDescEx(GraphicsPipelineDesc pipeline)
        {
            Pipeline = pipeline;
            State = GraphicsPipelineStateDesc.Default;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is GraphicsPipelineStateDescEx ex && Equals(ex);
        }

        /// <inheritdoc/>
        public readonly bool Equals(GraphicsPipelineStateDescEx other)
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
        /// Checks whether two <see cref="GraphicsPipelineStateDescEx"/> instances are equal.
        /// </summary>
        public static bool operator ==(GraphicsPipelineStateDescEx left, GraphicsPipelineStateDescEx right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks whether two <see cref="GraphicsPipelineStateDescEx"/> instances are not equal.
        /// </summary>
        public static bool operator !=(GraphicsPipelineStateDescEx left, GraphicsPipelineStateDescEx right)
        {
            return !(left == right);
        }
    }
}