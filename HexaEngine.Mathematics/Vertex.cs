namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Texture;
        public Vector3 Normal;
        public Vector3 Tangent;

        public void InvertTexture()
        {
            Texture.X = MathF.Abs(Texture.X);
            Texture.Y = MathF.Abs(Texture.Y - 1);
        }

        public Vertex(Vector3 position, Vector2 texture, Vector3 normal)
        {
            Position = position;
            Texture = new Vector3(texture, 0);
            Normal = normal;
            Tangent = Vector3.Zero;
        }

        public Vertex(Vector3 position, Vector2 texture, Vector3 normal, Vector3 tangent)
        {
            Position = position;
            Texture = new Vector3(texture, 0);
            Normal = normal;
            Tangent = tangent;
        }

        public Vertex(Vector3 position, Vector3 texture, Vector3 normal, Vector3 tangent)
        {
            Position = position;
            Texture = texture;
            Normal = normal;
            Tangent = tangent;
        }

        public Vertex(Vertex vertex, Vector3 normal, Vector3 tangent)
        {
            Position = vertex.Position;
            Texture = vertex.Texture;
            Normal = normal;
            Tangent = tangent;
        }
    }
}