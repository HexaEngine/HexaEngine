namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies how filled polygons are rendered.
    /// </summary>
    public enum FillMode : int
    {
        /// <summary>
        /// Render polygons as wireframes, showing only the edges.
        /// </summary>
        Wireframe = unchecked(2),

        /// <summary>
        /// Render polygons as solid, filled shapes.
        /// </summary>
        Solid = unchecked(3)
    }
}