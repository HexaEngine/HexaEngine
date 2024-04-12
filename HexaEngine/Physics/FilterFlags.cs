namespace HexaEngine.Physics
{
    [Flags]
    public enum FilterFlags : ushort
    {
        Kill = 1,
        Suppress = 2,
        Callback = 4,
        Notify = 4
    }
}