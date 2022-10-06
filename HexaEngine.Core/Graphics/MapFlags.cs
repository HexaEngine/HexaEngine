namespace HexaEngine.Core.Graphics
{
    [Flags]
    public enum MapFlags : int
    {
        DoNotWait = unchecked(1048576),
        None = unchecked(0)
    }
}