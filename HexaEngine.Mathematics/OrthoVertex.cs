namespace HexaEngine.Mathematics
{
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct OrthoVertex
    {
        public Vector3 Position;
        public Vector2 Texture;

        public OrthoVertex(Vector3 position, Vector2 texture)
        {
            Position = position;
            Texture = texture;
        }
    }
}