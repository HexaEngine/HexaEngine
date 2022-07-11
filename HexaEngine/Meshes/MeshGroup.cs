namespace HexaEngine.Meshes
{
    using System;

    public struct MeshGroup : IBinarySerializable
    {
        public string Name;

        public string MaterialName;

        public int[] Indices;

        public MeshFace[] Faces;

        public MeshMaterial Material;

        public unsafe int SizeOf()
        {
            int size = BinaryHelper.SizeOfString(Name);
            size += BinaryHelper.SizeOfString(Material.Name);
            size += BinaryHelper.SizeOfStructArray(Faces, sizeof(MeshFace));
            size += BinaryHelper.SizeOfStructArray(Indices, sizeof(int));
            return size;
        }

        public unsafe int Write(Span<byte> dest)
        {
            int idx = BinaryHelper.WriteString(dest, Name);
            idx += BinaryHelper.WriteString(dest[idx..], Material.Name);
            idx += BinaryHelper.WriteStructArray(dest[idx..], sizeof(MeshFace), Faces);
            idx += BinaryHelper.WriteStructArray(dest[idx..], sizeof(int), Indices);
            return idx;
        }

        public unsafe int Read(Span<byte> src)
        {
            int idx = BinaryHelper.ReadString(src, out Name);
            idx += BinaryHelper.ReadString(src[idx..], out MaterialName);
            idx += BinaryHelper.ReadStructArray(src[idx..], sizeof(MeshFace), out Faces);
            idx += BinaryHelper.ReadStructArray(src[idx..], sizeof(int), out Indices);
            return idx;
        }
    }
}