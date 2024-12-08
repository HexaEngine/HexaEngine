namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using System;

    /// <summary>
    /// Represents a graphics primitive that can be drawn automatically using a specified graphics pipeline.
    /// </summary>
    public interface IPrimitive : IDisposable
    {
        /// <summary>
        /// Draws the primitive automatically using the specified graphics context and graphics pipeline.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="pipeline">The graphics pipeline used for drawing.</param>
        void DrawAuto(IGraphicsContext context, IGraphicsPipelineState pipeline);

        /// <summary>
        /// Draws the primitive automatically using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        void DrawAuto(IGraphicsContext context);

        /// <summary>
        /// Binds the primitive to the graphics context, providing information about vertex count,
        /// index count, and instance count.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="vertexCount">Output parameter for the vertex count.</param>
        /// <param name="indexCount">Output parameter for the index count.</param>
        /// <param name="instanceCount">Output parameter for the instance count.</param>
        void BeginDraw(IGraphicsContext context, out uint vertexCount, out uint indexCount, out int instanceCount);

        /// <summary>
        /// Unbinds the primitive from the graphics context.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        void EndDraw(IGraphicsContext context);
    }
}