namespace HexaEngine.Core.Meshes.IO
{
    using System;
    using System.IO;
    using System.Text;

    public interface IMeshBody
    {
        int Read(MeshHeader header, ReadOnlySpan<byte> src, Encoding encoding);

        void Read(MeshHeader header, Stream stream, Encoding encoding);

        int Write(MeshHeader header, Span<byte> dst, Encoding encoding);

        void Write(MeshHeader header, Stream stream, Encoding encoding);
    }
}