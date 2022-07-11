namespace HexaEngine.Mathematics
{
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct LineVertex
    {
        public LineVertex(Vector3 position, Vector4 color)
        {
            Position = position;
            Color = color;
        }

        public Vector3 Position;
        public Vector4 Color;
    }
}