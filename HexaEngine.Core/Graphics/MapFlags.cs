namespace HexaEngine.Core.Graphics
{
    [Flags]
    public enum MapFlags : int
    {
        DoNotWait = unchecked((int)1048576),
        None = unchecked((int)0)
    }
}