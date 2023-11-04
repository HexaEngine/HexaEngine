namespace HexaEngine.Core.Graphics
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Represents a description of stencil operations and functions for depth-stencil testing.
    /// </summary>
    public struct DepthStencilOperationDescription : IEquatable<DepthStencilOperationDescription>
    {
        /// <summary>
        /// Gets or sets the stencil operation to perform when stencil testing fails.
        /// </summary>
        [DefaultValue(StencilOperation.Keep)]
        public StencilOperation StencilFailOp;

        /// <summary>
        /// Gets or sets the stencil operation to perform when stencil testing passes and depth testing fails.
        /// </summary>
        [DefaultValue(StencilOperation.Keep)]
        public StencilOperation StencilDepthFailOp;

        /// <summary>
        /// Gets or sets the stencil operation to perform when both stencil testing and depth testing pass.
        /// </summary>
        [DefaultValue(StencilOperation.Keep)]
        public StencilOperation StencilPassOp;

        /// <summary>
        /// Gets or sets the function that compares stencil data against existing stencil data.
        /// </summary>
        [DefaultValue(ComparisonFunction.Always)]
        public ComparisonFunction StencilFunc;

        /// <summary>
        /// A built-in description with default values.
        /// </summary>
        public static readonly DepthStencilOperationDescription Default = new(StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, ComparisonFunction.Always);

        /// <summary>
        /// A built-in description with default values.
        /// </summary>
        public static readonly DepthStencilOperationDescription DefaultFront = new(StencilOperation.Keep, StencilOperation.Increment, StencilOperation.Keep, ComparisonFunction.Always);

        /// <summary>
        /// A built-in description with default values.
        /// </summary>
        public static readonly DepthStencilOperationDescription DefaultBack = new(StencilOperation.Keep, StencilOperation.Decrement, StencilOperation.Keep, ComparisonFunction.Always);

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilOperationDescription"/> struct.
        /// </summary>
        /// <param name="stencilFailOp">A <see cref="StencilOperation"/> _value that identifies the stencil operation to perform when stencil testing fails.</param>
        /// <param name="stencilDepthFailOp">A <see cref="StencilOperation"/> _value that identifies the stencil operation to perform when stencil testing passes and depth testing fails.</param>
        /// <param name="stencilPassOp">A <see cref="StencilOperation"/> _value that identifies the stencil operation to perform when stencil testing and depth testing both pass.</param>
        /// <param name="stencilFunc">A <see cref="ComparisonFunction"/> _value that identifies the function that compares stencil data against existing stencil data.</param>
        public DepthStencilOperationDescription(StencilOperation stencilFailOp, StencilOperation stencilDepthFailOp, StencilOperation stencilPassOp, ComparisonFunction stencilFunc)
        {
            StencilFailOp = stencilFailOp;
            StencilDepthFailOp = stencilDepthFailOp;
            StencilPassOp = stencilPassOp;
            StencilFunc = stencilFunc;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is DepthStencilOperationDescription description && Equals(description);
        }

        /// <inheritdoc/>
        public readonly bool Equals(DepthStencilOperationDescription other)
        {
            return StencilFailOp == other.StencilFailOp &&
                   StencilDepthFailOp == other.StencilDepthFailOp &&
                   StencilPassOp == other.StencilPassOp &&
                   StencilFunc == other.StencilFunc;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(StencilFailOp, StencilDepthFailOp, StencilPassOp, StencilFunc);
        }

        /// <summary>
        /// Determines whether two <see cref="DepthStencilOperationDescription"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="DepthStencilOperationDescription"/> to compare.</param>
        /// <param name="right">The second <see cref="DepthStencilOperationDescription"/> to compare.</param>
        /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(DepthStencilOperationDescription left, DepthStencilOperationDescription right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="DepthStencilOperationDescription"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="DepthStencilOperationDescription"/> to compare.</param>
        /// <param name="right">The second <see cref="DepthStencilOperationDescription"/> to compare.</param>
        /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(DepthStencilOperationDescription left, DepthStencilOperationDescription right)
        {
            return !(left == right);
        }
    }
}