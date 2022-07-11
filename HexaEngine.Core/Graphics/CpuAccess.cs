namespace HexaEngine.Core.Graphics
{
    [Flags]
    public enum CpuAccessFlags : int
    {
        Write = unchecked((int)65536),
        Read = unchecked((int)131072),
        None = unchecked((int)0)
    }
}