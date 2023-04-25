namespace HexaEngine.Core.IO.Materials
{
    [Flags]
    public enum MaterialFlags
    {
        None = 0,
        Depth = 1,
        Tessellation = 2,
        Geometry = 4,
        Custom = 8,
        Transparent = 16,
    }
}