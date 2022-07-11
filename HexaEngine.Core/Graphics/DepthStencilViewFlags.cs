namespace HexaEngine.Core.Graphics
{
    [Flags]
    public enum DepthStencilViewFlags : int
    {
        ReadOnlyDepth = unchecked((int)1),
        ReadOnlyStencil = unchecked((int)2),
        None = unchecked((int)0)
    }
}