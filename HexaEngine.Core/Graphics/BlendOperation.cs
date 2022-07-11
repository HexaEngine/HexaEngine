namespace HexaEngine.Core.Graphics
{
    public enum BlendOperation : int
    {
        Add = unchecked((int)1),
        Subtract = unchecked((int)2),
        ReverseSubtract = unchecked((int)3),
        Min = unchecked((int)4),
        Max = unchecked((int)5)
    }
}