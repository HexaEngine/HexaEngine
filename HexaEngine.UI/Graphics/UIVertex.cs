namespace HexaEngine.UI.Graphics
{
    using System.Numerics;

    public struct UIVertex
    {
        public Vector2 Position;
        public Vector2 UV;
        public uint Color;

        public UIVertex(Vector2 position, Vector2 uv, uint color)
        {
            Position = position;
            UV = uv;
            Color = color;
        }
    }
}