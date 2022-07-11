namespace HexaEngine.Core.Graphics
{
    [Flags]
    public enum BindFlags : int
    {
        VertexBuffer = unchecked((int)1),
        IndexBuffer = unchecked((int)2),
        ConstantBuffer = unchecked((int)4),
        ShaderResource = unchecked((int)8),
        StreamOutput = unchecked((int)16),
        RenderTarget = unchecked((int)32),
        DepthStencil = unchecked((int)64),
        UnorderedAccess = unchecked((int)128),
        Decoder = unchecked((int)512),
        VideoEncoder = unchecked((int)1024),
        None = unchecked((int)0)
    }
}