namespace HexaEngine.Mathematics
{
    using System.Collections.Generic;
    using System.Numerics;

    public class VertexRef
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;
        public List<Face> Faces = new();

        public VertexRef(Vector3 position, Vector2 texture, Vector3 normal)
        {
            Position = position;
            Texture = texture;
            Normal = normal;
        }

        public Vector3 GetTangent()
        {
            Vector3 comulative = Vector3.Zero;
            for (int i = 0; i < Faces.Count; i++)
            {
                comulative += Faces[i].Tangent;
            }
            return Vector3.Normalize(comulative / Faces.Count);
        }

        public static implicit operator Vertex(VertexRef vertex)
        {
            return new Vertex(vertex.Position, vertex.Texture, vertex.Normal, vertex.GetTangent());
        }
    }

    public class Face
    {
        public Face(VertexRef vertex1, VertexRef vertex2, VertexRef vertex3)
        {
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            Vertex3 = vertex3;
            vertex1.Faces.Add(this);
            vertex2.Faces.Add(this);
            vertex3.Faces.Add(this);
        }

        public VertexRef Vertex1;
        public VertexRef Vertex2;
        public VertexRef Vertex3;
        public Vector3 Tangent;

        public void ComputeTangent()
        {
            Vector3 deltaPos1 = Vertex2.Position - Vertex1.Position;
            Vector3 deltaPos2 = Vertex3.Position - Vertex1.Position;

            Vector2 deltaTexture1 = Vertex2.Texture - Vertex1.Texture;
            Vector2 deltaTexture2 = Vertex3.Texture - Vertex1.Texture;

            float den = 1.0f / (deltaTexture1.X * deltaTexture2.Y - deltaTexture1.Y * deltaTexture2.X);
            Tangent += (deltaPos1 * deltaTexture2.Y - deltaPos2 * deltaTexture1.Y) * den;
        }

        public static void ComputeTangent(Vertex vertex1, Vertex vertex2, Vertex vertex3, out Vector3 tangent)
        {
            // Calculate the two vectors for the this face.
            Vector3 vecPd1 = new(vertex2.Position.X - vertex1.Position.X, vertex2.Position.Y - vertex1.Position.Y, vertex2.Position.Z - vertex1.Position.Z);
            Vector3 vecPd2 = new(vertex3.Position.X - vertex1.Position.X, vertex3.Position.Y - vertex1.Position.Y, vertex3.Position.Z - vertex1.Position.Z);

            // Calculate the tu and tv texture space vectors.
            Vector2 vecTd1 = new(vertex2.Texture.X - vertex1.Texture.X, vertex3.Texture.X - vertex1.Texture.X);
            Vector2 vecTd2 = new(vertex2.Texture.Y - vertex1.Texture.Y, vertex3.Texture.Y - vertex1.Texture.Y);

            // Calculate the denominator of the tangent / binormal equation.
            float den = 1.0f / (vecTd1.X * vecTd2.Y - vecTd1.Y * vecTd2.X);

            // Calculate the cross products and multiply by the coefficient to get the tangent and binormal.
            tangent.X = (vecTd2.Y * vecPd1.X - vecTd2.X * vecPd2.X) * den;
            tangent.Y = (vecTd2.Y * vecPd1.Y - vecTd2.X * vecPd2.Y) * den;
            tangent.Z = (vecTd2.Y * vecPd1.Z - vecTd2.X * vecPd2.Z) * den;

            // Normalize the normal and the store it.
            tangent = Vector3.Normalize(tangent);
        }

        public static void ComputeTangent(VertexRef vertex1, VertexRef vertex2, VertexRef vertex3, out Vector3 tangent)
        {
            // Calculate the two vectors for the this face.
            Vector3 vecPd1 = new(vertex2.Position.X - vertex1.Position.X, vertex2.Position.Y - vertex1.Position.Y, vertex2.Position.Z - vertex1.Position.Z);
            Vector3 vecPd2 = new(vertex3.Position.X - vertex1.Position.X, vertex3.Position.Y - vertex1.Position.Y, vertex3.Position.Z - vertex1.Position.Z);

            // Calculate the tu and tv texture space vectors.
            Vector2 vecTd1 = new(vertex2.Texture.X - vertex1.Texture.X, vertex3.Texture.X - vertex1.Texture.X);
            Vector2 vecTd2 = new(vertex2.Texture.Y - vertex1.Texture.Y, vertex3.Texture.Y - vertex1.Texture.Y);

            // Calculate the denominator of the tangent / binormal equation.
            float den = 1.0f / (vecTd1.X * vecTd2.Y - vecTd1.Y * vecTd2.X);

            // Calculate the cross products and multiply by the coefficient to get the tangent and binormal.
            tangent.X = (vecTd2.Y * vecPd1.X - vecTd2.X * vecPd2.X) * den;
            tangent.Y = (vecTd2.Y * vecPd1.Y - vecTd2.X * vecPd2.Y) * den;
            tangent.Z = (vecTd2.Y * vecPd1.Z - vecTd2.X * vecPd2.Z) * den;

            // Normalize the normal and the store it.
            tangent = Vector3.Normalize(tangent);
        }
    }
}