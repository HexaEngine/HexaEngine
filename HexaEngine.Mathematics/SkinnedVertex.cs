namespace HexaEngine.Mathematics
{
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SkinnedVertex
    {
        public Vector3 Position;
        public Vector3 Texture;
        public Vector3 Normal;
        public Vector3 Tangent;
        public float Weight;
        public BonePalette BoneIndices;

        public SkinnedVertex(Vertex vertex, float weight, byte[] boneIndices) : this(vertex.Position, vertex.Normal, vertex.Texture, vertex.Tangent, weight, boneIndices)
        {
        }

        public SkinnedVertex(Vector3 pos, Vector3 norm, Vector3 uv, Vector3 tan, float weight, byte[] boneIndices)
        {
            Position = pos;
            Normal = norm;
            Texture = uv;
            Tangent = tan;
            Weight = weight;
            BoneIndices = new BonePalette();
            for (int index = 0; index < boneIndices.Length; index++)
            {
                switch (index)
                {
                    case 0:
                        BoneIndices.B0 = boneIndices[index];
                        break;

                    case 1:
                        BoneIndices.B1 = boneIndices[index];
                        break;

                    case 2:
                        BoneIndices.B2 = boneIndices[index];
                        break;

                    case 3:
                        BoneIndices.B3 = boneIndices[index];
                        break;
                }
            }
        }
    }
}