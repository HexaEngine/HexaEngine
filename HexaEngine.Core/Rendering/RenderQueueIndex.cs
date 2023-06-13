namespace HexaEngine.Core.Renderers
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
}