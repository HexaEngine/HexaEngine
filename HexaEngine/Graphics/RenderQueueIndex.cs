namespace HexaEngine.Graphics
{
    public enum RenderQueueIndex
    {
        Background = 0,
        Geometry = 1000,
        AlphaTest = 2000,
        GeometryLast = 3000,
        Transparency = 4000,
        Overlay = 5000,
    }

    [Flags]
    public enum RenderQueueIndexFlags
    {
        None = 0,
        Background = 1,
        Geometry = 2,
        AlphaTest = 4,
        GeometryLast = 8,
        Transparency = 16,
        Overlay = 32,
    }
}