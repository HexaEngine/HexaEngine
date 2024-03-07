namespace HexaEngine.UI
{
    using System.Numerics;

    public struct RmGuiVertex
    {
        public Vector2 Position;
        public Vector2 UV;
        public uint Color;

        public RmGuiVertex(Vector2 position, Vector2 uv, uint color)
        {
            Position = position;
            UV = uv;
            Color = color;
        }
    }
}