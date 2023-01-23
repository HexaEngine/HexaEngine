namespace HexaEngine.Core.Graphics
{
    public struct DepthStencilOperationDescription
    {
        public StencilOperation StencilFailOp;
        public StencilOperation StencilDepthFailOp;
        public StencilOperation StencilPassOp;
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
        /// <param name="stencilFailOp">A <see cref="StencilOperation"/> value that identifies the stencil operation to perform when stencil testing fails.</param>
        /// <param name="stencilDepthFailOp">A <see cref="StencilOperation"/> value that identifies the stencil operation to perform when stencil testing passes and depth testing fails.</param>
        /// <param name="stencilPassOp">A <see cref="StencilOperation"/> value that identifies the stencil operation to perform when stencil testing and depth testing both pass.</param>
        /// <param name="stencilFunc">A <see cref="ComparisonFunction"/> value that identifies the function that compares stencil data against existing stencil data.</param>
        public DepthStencilOperationDescription(StencilOperation stencilFailOp, StencilOperation stencilDepthFailOp, StencilOperation stencilPassOp, ComparisonFunction stencilFunc)
        {
            StencilFailOp = stencilFailOp;
            StencilDepthFailOp = stencilDepthFailOp;
            StencilPassOp = stencilPassOp;
            StencilFunc = stencilFunc;
        }
    }
}