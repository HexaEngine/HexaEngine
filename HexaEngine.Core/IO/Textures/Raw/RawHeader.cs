namespace HexaEngine.Core.IO.Textures.Raw
{
    using HexaEngine.Core.Graphics;
    using System.Buffers.Binary;

    public struct RawHeader
    {
        public int Width;
        public int Height;
        public Format Format;

        public readonly void Write(Stream dst)
        {
            Span<byte> buffer = stackalloc byte[12];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, Width);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[4..], Height);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[8..], (int)Format);
            dst.Write(buffer);
        }

        public static RawHeader ReadFrom(Stream src)
        {
            RawHeader header = default;
            header.Read(src);
            return header;
        }

        public void Read(Stream src)
        {
            Span<byte> buffer = stackalloc byte[12];
            src.Read(buffer);
            Width = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            Height = BinaryPrimitives.ReadInt32LittleEndian(buffer[4..]);
            Format = (Format)BinaryPrimitives.ReadInt32LittleEndian(buffer[8..]);
        }
    }
}