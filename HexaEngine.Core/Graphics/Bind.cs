namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Flags used to specify the bind options for a resource, such as a buffer or texture.
    /// </summary>
    [Flags]
    public enum BindFlags : int
    {
        /// <summary>
        /// Specifies that the resource will be bound as a vertex buffer.
        /// </summary>
        VertexBuffer = unchecked(1),

        /// <summary>
        /// Specifies that the resource will be bound as an index buffer.
        /// </summary>
        IndexBuffer = unchecked(2),

        /// <summary>
        /// Specifies that the resource will be bound as a constant buffer.
        /// </summary>
        ConstantBuffer = unchecked(4),

        /// <summary>
        /// Specifies that the resource will be bound as a shader resource.
        /// </summary>
        ShaderResource = unchecked(8),

        /// <summary>
        /// Specifies that the resource will be used for stream output.
        /// </summary>
        StreamOutput = unchecked(16),

        /// <summary>
        /// Specifies that the resource will be bound as a render target.
        /// </summary>
        RenderTarget = unchecked(32),

        /// <summary>
        /// Specifies that the resource will be bound as a depth stencil target.
        /// </summary>
        DepthStencil = unchecked(64),

        /// <summary>
        /// Specifies that the resource will be bound for unordered access.
        /// </summary>
        UnorderedAccess = unchecked(128),

        /// <summary>
        /// Specifies that the resource is for a decoder.
        /// </summary>
        Decoder = unchecked(512),

        /// <summary>
        /// Specifies that the resource is for a video encoder.
        /// </summary>
        VideoEncoder = unchecked(1024),

        /// <summary>
        /// Specifies that the resource is not bound for any purpose.
        /// </summary>
        None = unchecked(0)
    }
}