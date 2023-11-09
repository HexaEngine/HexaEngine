namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies the classification of input data in a vertex buffer.
    /// </summary>
    public enum InputClassification : int
    {
        /// <summary>
        /// Specifies that the input data is per-vertex.
        /// </summary>
        PerVertexData = unchecked(0),

        /// <summary>
        /// Specifies that the input data is per-instance.
        /// </summary>
        PerInstanceData = unchecked(1)
    }
}