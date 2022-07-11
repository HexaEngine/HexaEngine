namespace HexaEngine.Meshes
{
    using System;
    using System.Linq;

    public struct MeshMaterialLibrary
    {
        public MeshMaterial[] Materials;

        public int SizeOf()
        {
            return 4 + Materials.Sum(x => x.SizeOf());
        }

        public int Write(Span<byte> dest)
        {
            int idx = BinaryHelper.WriteInt32(dest, Materials.Length);
            for (int i = 0; i < Materials.Length; i++)
            {
                idx += Materials[i].Write(dest[idx..]);
            }
            return idx;
        }

        public int Read(Span<byte> src)
        {
            int idx = BinaryHelper.ReadInt32(src, out int len);
            Materials = new MeshMaterial[len];
            for (int i = 0; i < len; i++)
            {
                idx += Materials[i].Read(src[idx..]);
            }
            return idx;
        }
    }
}