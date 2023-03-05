namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public unsafe struct MeshData
    {
        public string Name;
        public MaterialData Material;
        public ulong VerticesCount;
        public ulong IndicesCount;
        public ulong BonesCount;
        public BoundingBox Box;
        public BoundingSphere Sphere;
        public MeshVertex[] Vertices;
        public uint[] Indices;
        public MeshBone[] Bones;

        public MeshData(string name, MaterialData material, BoundingBox box, BoundingSphere sphere, MeshVertex[] vertices, uint[] indices, MeshBone[] bones)
        {
            Name = name;
            Material = material;
            Box = box;
            Sphere = sphere;
            Vertices = vertices;
            Indices = indices;
            Bones = bones;
            VerticesCount = (ulong)vertices.Length;
            IndicesCount = (ulong)indices.Length;
            BonesCount = (ulong)bones.Length;
        }

        public static MeshData Read(Stream stream, Encoding encoding)
        {
            MeshData data = default;
            data.Name = stream.ReadString(encoding);
            data.Material = MaterialData.Read(stream, encoding);
            data.VerticesCount = stream.ReadUInt64();
            data.IndicesCount = stream.ReadUInt64();
            data.BonesCount = stream.ReadUInt64();
            data.Box = stream.ReadStruct<BoundingBox>();
            data.Sphere = stream.ReadStruct<BoundingSphere>();
            data.Vertices = new MeshVertex[data.VerticesCount];
            stream.Read(MemoryMarshal.Cast<MeshVertex, byte>(data.Vertices));

            data.Indices = new uint[data.IndicesCount];
            stream.Read(MemoryMarshal.Cast<uint, byte>(data.Indices));

            data.Bones = new MeshBone[data.BonesCount];
            for (ulong i = 0; i < data.BonesCount; i++)
            {
                data.Bones[i] = MeshBone.Read(stream, encoding);
            }

            return data;
        }

        public void Write(Stream stream, Encoding encoding)
        {
            stream.WriteString(Name, encoding);
            Material.Write(stream, encoding);
            stream.WriteUInt64(VerticesCount);
            stream.WriteUInt64(IndicesCount);
            stream.WriteUInt64(BonesCount);
            stream.WriteStruct(Box);
            stream.WriteStruct(Sphere);
            stream.Write(MemoryMarshal.Cast<MeshVertex, byte>(Vertices));
            stream.Write(MemoryMarshal.Cast<uint, byte>(Indices));
            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].Write(stream, encoding);
            }
        }
    }
}