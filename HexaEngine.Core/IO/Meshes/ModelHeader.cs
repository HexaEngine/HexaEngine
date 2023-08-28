﻿namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    public struct ModelHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6e, 0x73, 0x4d, 0x65, 0x73, 0x68, 0x00 };
        public static readonly Version Version = 8;
        public static readonly Version MinVersion = 7;
        public Endianness Endianness;
        public Encoding Encoding;
        public Compression Compression;
        public string MaterialLibrary;
        public ulong MeshCount;
        public ulong ContentStart;

        public void Read(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
            {
                throw new InvalidDataException("Magic number mismatch");
            }

            Endianness = (Endianness)stream.ReadByte();
            if (!stream.CompareVersion(MinVersion, Version, Endianness, out var version))
            {
                throw new InvalidDataException($"Version mismatch, file: {version} min: {MinVersion} max: {Version}");
            }

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
            Compression = (Compression)stream.ReadInt32(Endianness);
            MaterialLibrary = stream.ReadString(Encoding, Endianness) ?? string.Empty;
            MeshCount = stream.ReadUInt64(Endianness);
            ContentStart = (ulong)stream.Position;
        }

        public void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteInt32((int)Compression, Endianness);
            stream.WriteString(MaterialLibrary, Encoding, Endianness);
            stream.WriteUInt64(MeshCount, Endianness);
        }
    }
}