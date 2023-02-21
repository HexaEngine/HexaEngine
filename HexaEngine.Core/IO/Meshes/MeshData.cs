namespace HexaEngine.Core.IO.Meshes
{
    using BepuPhysics;
    using HexaEngine.Core.Meshes;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public unsafe struct MeshData
    {
        public MeshVertex[] Vertices;
        public uint[] Indices;
        public MeshBone[] Bones;

        public MeshData(MeshVertex[] vertices, uint[] indices, MeshBone[] bones)
        {
            Vertices = vertices;
            Indices = indices;
            Bones = bones;
        }

        public static MeshData Read(MeshHeader header, Stream stream, Encoding encoding)
        {
            var vertices = new MeshVertex[header.VerticesCount];
            stream.Read(MemoryMarshal.Cast<MeshVertex, byte>(vertices));

            var indices = new uint[header.IndicesCount];
            stream.Read(MemoryMarshal.Cast<uint, byte>(indices));

            var bones = new MeshBone[header.BonesCount];
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i] = MeshBone.Read(stream, encoding);
            }

            return new MeshData(vertices, indices, bones);
        }

        public void Write(Stream stream, Encoding encoding)
        {
            stream.Write(MemoryMarshal.Cast<MeshVertex, byte>(Vertices));
            stream.Write(MemoryMarshal.Cast<uint, byte>(Indices));
            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].Write(stream, encoding);
            }
        }
    }
}