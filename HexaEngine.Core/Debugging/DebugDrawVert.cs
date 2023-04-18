#nullable disable

namespace HexaEngine.Editor
{
    using System.Numerics;

    public struct DebugDrawVert
    {
        public DebugDrawVert(Vector3 position, Vector2 uv, uint color)
        {
            Position = position;
            UV = uv;
            Color = color;
        }

        public Vector3 Position;
        public Vector2 UV;
        public uint Color;
    }
}