namespace HexaEngine.IO.Meshes
{
    using HexaEngine.Meshes;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public unsafe struct MeshBody
    {
        public MeshVertex[] Vertices;
        public uint[] Indices;

        public MeshBody(MeshVertex[] vertices, uint[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }

        public static MeshBody Read(MeshHeader header, Stream stream, Encoding encoding)
        {
            var vertices = new MeshVertex[header.VerticesCount];
            stream.Read(MemoryMarshal.Cast<MeshVertex, byte>(vertices));

            var indices = new uint[header.IndicesCount];
            stream.Read(MemoryMarshal.Cast<uint, byte>(indices));
            return new MeshBody(vertices, indices);
        }

        public int Read(MeshHeader header, ReadOnlySpan<byte> src, Encoding encoding)
        {
            int idx = 0;
            {
                Vertices = new MeshVertex[header.VerticesCount];
                src[idx..].CopyTo(MemoryMarshal.Cast<MeshVertex, byte>(Vertices));
                idx += Vertices.Length * sizeof(MeshVertex);
            }
            {
                Indices = new uint[header.IndicesCount];
                src[idx..].CopyTo(MemoryMarshal.Cast<uint, byte>(Indices));
                idx += Indices.Length * sizeof(uint);
            }

            return idx;
        }

        public void Write(MeshHeader header, Stream stream, Encoding encoding)
        {
            {
                stream.Write(MemoryMarshal.Cast<MeshVertex, byte>(Vertices));
            }
            {
                stream.Write(MemoryMarshal.Cast<uint, byte>(Indices));
            }
        }

        public int Write(MeshHeader header, Span<byte> dst, Encoding encoding)
        {
            int idx = 0;
            {
                MemoryMarshal.Cast<MeshVertex, byte>(Vertices).CopyTo(dst[idx..]);
                idx += Vertices.Length * sizeof(MeshVertex);
            }
            {
                MemoryMarshal.Cast<uint, byte>(Indices).CopyTo(dst[idx..]);
                idx += Indices.Length * sizeof(uint);
            }

            return idx;
        }
    }
}