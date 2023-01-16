namespace HexaEngine.IO.Meshes
{
    using HexaEngine.Mathematics;
    using System.Buffers.Binary;
    using System.IO;
    using System.Text;

    public struct MeshHeader : IBinarySerializable
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6e, 0x73, 0x4d, 0x65, 0x73, 0x68, 0x00 };
        public const ulong Version = 1;

        public Compression Compression;
        public MeshType Type;
        public BoundingBox BoundingBox;
        public BoundingSphere BoundingSphere;
        public ulong VerticesCount;
        public ulong IndicesCount;
        public ulong BodyStart;

        public void Read(Stream stream, Encoding encoding)
        {
            if (!stream.Compare(MagicNumber))
                throw new InvalidDataException();
            if (!stream.Compare(Version))
                throw new InvalidDataException();

            Compression = (Compression)stream.ReadInt();
            Type = (MeshType)stream.ReadInt();
            BoundingBox = stream.ReadStruct<BoundingBox>();
            BoundingSphere = stream.ReadStruct<BoundingSphere>();
            VerticesCount = stream.ReadUInt64();
            IndicesCount = stream.ReadUInt64();
            BodyStart = (ulong)stream.Position;
        }

        public int Read(ReadOnlySpan<byte> src, Encoding encoding)
        {
            if (!src[..MagicNumber.Length].SequenceEqual(MagicNumber))
                throw new InvalidDataException();
            if (!src[MagicNumber.Length..].Compare(Version))
                throw new InvalidDataException();
            int idx = MagicNumber.Length + 8;
            Compression = (Compression)BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;
            Type = (MeshType)BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;
            VerticesCount = BinaryPrimitives.ReadUInt64LittleEndian(src[idx..]);
            idx += 8;
            IndicesCount = BinaryPrimitives.ReadUInt64LittleEndian(src[idx..]);
            idx += 8;
            BodyStart = (ulong)idx;

            return idx;
        }

        public void Write(Stream stream, Encoding encoding)
        {
            stream.Write(MagicNumber);
            stream.WriteUInt64(Version);
            stream.WriteInt((int)Compression);
            stream.WriteInt((int)Type);
            stream.WriteStruct(BoundingBox);
            stream.WriteStruct(BoundingSphere);
            stream.WriteUInt64(VerticesCount);
            stream.WriteUInt64(IndicesCount);
        }

        public int Write(Span<byte> dst, Encoding encoding)
        {
            MagicNumber.CopyTo(dst);
            BinaryPrimitives.WriteUInt64LittleEndian(dst[MagicNumber.Length..], Version);
            int idx = MagicNumber.Length + 8;
            BinaryPrimitives.WriteInt32LittleEndian(dst[idx..], (int)Compression);
            idx += 4;
            BinaryPrimitives.WriteInt32LittleEndian(dst[idx..], (int)Type);
            idx += 4;
            BinaryPrimitives.WriteUInt64LittleEndian(dst[idx..], VerticesCount);
            idx += 8;
            BinaryPrimitives.WriteUInt64LittleEndian(dst[idx..], IndicesCount);
            idx += 8;

            return idx;
        }

        public int Size(Encoding encoding)
        {
            return MagicNumber.Length + 8 + 4 + 4 + 8 + 8;
        }
    }
}