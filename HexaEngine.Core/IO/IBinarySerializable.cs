namespace HexaEngine.IO
{
    using System;
    using System.IO;
    using System.Text;

    public interface IBinarySerializable
    {
        int Read(ReadOnlySpan<byte> src, Encoding encoding);

        void Read(Stream stream, Encoding encoding);

        int Size(Encoding encoding);

        int Write(Span<byte> dst, Encoding encoding);

        void Write(Stream stream, Encoding encoding);
    }
}