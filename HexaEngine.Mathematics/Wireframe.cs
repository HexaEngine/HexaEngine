namespace HexaEngine.Mathematics
{
    using System.Numerics;

    public struct Wireframe
    {
        public Vector3[] Points;
        public int[] Indices;

        public Wireframe(Vector3[] points)
        {
            Points = points;
            Indices = new int[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Indices[i] = i;
            }
        }

        public Wireframe(Vector3[] points, int[] indices, int vertexFaceCount = 3)
        {
            var vertexFaceCountMinusOne = vertexFaceCount - 1;
            var vertexFaceLineCount = vertexFaceCount + 1;
            var indexCount = indices.Length / vertexFaceCount * vertexFaceLineCount;
            Points = points;
            Indices = new int[indexCount];

            for (int i = 0; i < indexCount; i += vertexFaceLineCount)
            {
                for (int j = 0; j < vertexFaceCount; j++)
                {
                    Indices[i * vertexFaceLineCount + j] = indices[i * vertexFaceCount + j];
                }

                Indices[i * vertexFaceLineCount + vertexFaceCount] = indices[i * vertexFaceCount + vertexFaceCountMinusOne];
            }
        }

        public Wireframe(Vertex[] vertices)
        {
            Points = new Vector3[vertices.Length];
            Indices = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                Points[i] = vertices[i].Position;
            }
            for (int i = 0; i < vertices.Length; i++)
            {
                Indices[i] = i;
            }
        }

        public Wireframe(Vertex[] vertices, int[] indices, int vertexFaceCount = 3)
        {
            var vertexFaceCountMinusOne = vertexFaceCount - 1;
            var vertexFaceLineCount = vertexFaceCount + 1;
            var indexCount = indices.Length / vertexFaceCount * vertexFaceLineCount;
            Points = new Vector3[vertices.Length];
            Indices = new int[indexCount];

            for (int i = 0; i < vertices.Length; i++)
            {
                Points[i] = vertices[i].Position;
            }

            for (int i = 0; i < indexCount; i += vertexFaceLineCount)
            {
                for (int j = 0; j < vertexFaceCount; j++)
                {
                    Indices[i * vertexFaceLineCount + j] = indices[i * vertexFaceCount + j];
                }

                Indices[i * vertexFaceLineCount + vertexFaceCount] = indices[i * vertexFaceCount + vertexFaceCountMinusOne];
            }
        }
    }
}