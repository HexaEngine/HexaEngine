namespace HexaEngine.Core.Graphics.Shaders.Reflection
{
    public enum Dimension
    {
        Texture1D = 0,
        Texture2D = 1,
        Texture3D = 2,
        TextureCube = 3,
        Rect = 4,
        Buffer = 5,
        SubpassData = 6,
        TileImageDataExt = 4173,
        Max = int.MaxValue
    }
}