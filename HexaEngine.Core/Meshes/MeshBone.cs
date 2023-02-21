namespace HexaEngine.Core.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Meshes;
    using System.IO;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;

    public unsafe struct MeshBone
    {
        public string Name;
        public MeshWeight[] Weights;
        public Matrix4x4 Offset;

        public MeshBone(string name, MeshWeight[] weights, Matrix4x4 offset)
        {
            Name = name;
            Weights = weights;
            Offset = offset;
        }

        public static MeshBone Read(Stream stream, Encoding encoding)
        {
            string name = stream.ReadString(encoding);
            uint nweights = stream.ReadUInt();
            MeshWeight[] weights = new MeshWeight[nweights];
            stream.Read(MemoryMarshal.Cast<MeshWeight, byte>(weights));
            Matrix4x4 offset = stream.ReadStruct<Matrix4x4>();
            return new MeshBone(name, weights, offset);
        }

        public void Write(Stream stream, Encoding encoding)
        {
            stream.WriteString(Name, encoding);
            stream.WriteUInt((uint)Weights.LongLength);
            stream.Write(MemoryMarshal.Cast<MeshWeight, byte>(Weights));
            stream.WriteStruct(Offset);
        }
    }
}