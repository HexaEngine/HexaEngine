namespace HexaEngine.Core.Graphics
{
    [Flags]
    public enum ColorWriteEnable : byte
    {
        Red = unchecked((byte)1),
        Green = unchecked((byte)2),
        Blue = unchecked((byte)4),
        Alpha = unchecked((byte)8),
        All = unchecked((byte)15),
        None = unchecked((byte)0)
    }
}