namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Describes the configuration for a compute pipeline state object, including any pipeline state flags that affect
    /// its behavior.
    /// </summary>
    /// <remarks>Use this structure to specify options when creating or comparing compute pipeline state
    /// objects. The equality members allow for value-based comparison of pipeline state descriptions.</remarks>
    public struct ComputePipelineStateDesc : IEquatable<ComputePipelineStateDesc>
    {
        /// <summary>
        /// Specifies the set of flags that describe the current state of the pipeline.
        /// </summary>
        public PipelineStateFlags Flags;

        public static readonly ComputePipelineStateDesc Default = new(PipelineStateFlags.None);

        /// <summary>
        /// Initializes a new instance of the ComputePipelineStateDesc struct with default values.
        /// </summary>
        public ComputePipelineStateDesc()
        {
            Flags = PipelineStateFlags.None;
        }

        /// <summary>
        /// Initializes a new instance of the ComputePipelineStateDesc struct with the specified pipeline state flags.
        /// </summary>
        /// <param name="flags">The flags that specify options for configuring the compute pipeline state.</param>
        public ComputePipelineStateDesc(PipelineStateFlags flags)
        {
            Flags = flags;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current ComputePipelineStateDesc instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current ComputePipelineStateDesc instance.</param>
        /// <returns>true if the specified object is a ComputePipelineStateDesc and is equal to the current instance; otherwise,
        /// false.</returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is ComputePipelineStateDesc desc && Equals(desc);
        }

        /// <summary>
        /// Indicates whether the current instance is equal to the specified <see cref="ComputePipelineStateDesc"/>.
        /// </summary>
        /// <param name="other">The <see cref="ComputePipelineStateDesc"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see
        /// langword="false"/>.</returns>
        public readonly bool Equals(ComputePipelineStateDesc other)
        {
            return Flags == other.Flags;
        }

        /// <summary>
        /// Serves as the default hash function for the current object.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code representing the current object.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Flags);
        }

        /// <summary>
        /// Determines whether two specified ComputePipelineStateDesc instances are equal.
        /// </summary>
        /// <param name="left">The first ComputePipelineStateDesc to compare.</param>
        /// <param name="right">The second ComputePipelineStateDesc to compare.</param>
        /// <returns>true if the two ComputePipelineStateDesc instances are equal; otherwise, false.</returns>
        public static bool operator ==(ComputePipelineStateDesc left, ComputePipelineStateDesc right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified ComputePipelineStateDesc instances are not equal.
        /// </summary>
        /// <param name="left">The first ComputePipelineStateDesc instance to compare.</param>
        /// <param name="right">The second ComputePipelineStateDesc instance to compare.</param>
        /// <returns>true if the instances are not equal; otherwise, false.</returns>
        public static bool operator !=(ComputePipelineStateDesc left, ComputePipelineStateDesc right)
        {
            return !(left == right);
        }
    }
}