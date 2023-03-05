namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.IO;
    using System.IO;
    using System.Text;

    public struct MeshHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6e, 0x73, 0x4d, 0x65, 0x73, 0x68, 0x00 };
        public const ulong Version = 3;

        public Compression Compression;
        public MeshType Type;
        public ulong MeshCount;
        public ulong BodyStart;

        public void Read(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
                throw new InvalidDataException();
            if (!stream.Compare(Version))
                throw new InvalidDataException();

            Compression = (Compression)stream.ReadInt();
            Type = (MeshType)stream.ReadInt();
            MeshCount = stream.ReadUInt64();
            BodyStart = (ulong)stream.Position;
        }

        public void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteUInt64(Version);
            stream.WriteInt((int)Compression);
            stream.WriteInt((int)Type);
            stream.WriteUInt64(MeshCount);
        }
    }
}