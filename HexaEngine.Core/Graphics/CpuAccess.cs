namespace HexaEngine.Core.Graphics
{
    [Flags]
    public enum GpuAccessFlags : int
    {
        Write = unchecked(65536),
        Read = unchecked(131072),
        UA = unchecked(262144),
        None = unchecked(0),
        RW = Read | Write,
        All = Read | Write | UA,
    }

    [Flags]
    public enum CpuAccessFlags : int
    {
        Write = unchecked(65536),
        Read = unchecked(131072),
        None = unchecked(0),
        RW = Read | Write
    }
}