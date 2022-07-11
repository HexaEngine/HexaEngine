namespace HexaEngine.Core.Graphics
{
    public enum UnorderedAccessViewDimension : int
    {
        Unknown = unchecked((int)0),
        Buffer = unchecked((int)1),
        Texture1D = unchecked((int)2),
        Texture1DArray = unchecked((int)3),
        Texture2D = unchecked((int)4),
        Texture2DArray = unchecked((int)5),
        Texture3D = unchecked((int)8)
    }
}