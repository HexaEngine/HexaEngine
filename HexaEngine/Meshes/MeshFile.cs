namespace HexaEngine.Meshes
{
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
    }
}