namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies how triangles are culled during rasterization.
    /// </summary>
    public enum CullMode : int
    {
        /// <summary>
        /// No triangles are culled.
        /// </summary>
        None = unchecked(1),

        /// <summary>
        /// Front-facing triangles are culled.
        /// </summary>
        Front = unchecked(2),

        /// <summary>
        /// Back-facing triangles are culled.
        /// </summary>
        Back = unchecked(3)
    }
}