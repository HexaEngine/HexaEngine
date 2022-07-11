namespace HexaEngine.Core.Graphics
{
    [Flags]
    public enum BufferUnorderedAccessViewFlags : int
    {
        Raw = unchecked((int)1),
        Append = unchecked((int)2),
        Counter = unchecked((int)4),
        None = unchecked((int)0)
    }
}