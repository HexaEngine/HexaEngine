namespace HexaEngine.Core.Graphics
{
    [Flags]
    public enum CpuAccessFlags : int
    {
        Write = unchecked(65536),
        Read = unchecked(131072),
        None = unchecked(0)
    }
}