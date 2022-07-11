namespace HexaEngine.Core.Graphics
{
    public enum DepthStencilViewDimension : int
    {
        Unknown = unchecked((int)0),
        Texture1D = unchecked((int)1),
        Texture1DArray = unchecked((int)2),
        Texture2D = unchecked((int)3),
        Texture2DArray = unchecked((int)4),
        Texture2DMultisampled = unchecked((int)5),
        Texture2DMultisampledArray = unchecked((int)6)
    }
}