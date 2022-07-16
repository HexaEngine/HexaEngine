namespace HexaEngine.Meshes
{
    using BepuPhysics.Collidables;
    using System;

    public class MeshFile : IBinarySerializable
    {
        public MeshVertex[] Vertices = Array.Empty<MeshVertex>();
        public MeshGroup[] Groups = Array.Empty<MeshGroup>();

        public unsafe int SizeOf()
        {
            int size = BinaryHelper.SizeOfBinaryArray(Groups);
            size += BinaryHelper.SizeOfStructArray(Vertices, sizeof(MeshVertex));
            return size;
        }

        public unsafe int Write(Span<byte> dest)
        {
            int idx = BinaryHelper.WriteBinaryArray(dest, Groups);
            idx += BinaryHelper.WriteStructArray(dest[idx..], sizeof(MeshVertex), Vertices);
            return idx;
        }

        public unsafe int Read(Span<byte> src)
        {
            int idx = BinaryHelper.ReadBinaryArray(src, out Groups);
            idx += BinaryHelper.ReadStructArray(src[idx..], sizeof(MeshVertex), out Vertices);
            return idx;
        }

        public Triangle[] GetTriangles()
        {
            Triangle[] triangles = new Triangle[Groups[0].Faces.Length];
            for (int i = 0; i < Groups[0].Faces.Length; i++)
            {
                var face = Groups[0].Faces[i];
                triangles[i] = new(Vertices[face.Vertex1].Position, Vertices[face.Vertex2].Position, Vertices[face.Vertex3].Position);
            }
            return triangles;
        }
    }
}