namespace HexaEngine.PostFx
{
    [Flags]
    public enum PostProcessingFlags
    {
        None = 0,
        HDR = 1,
        Debug = 2,
        ForceDynamic = 4,
    }
}