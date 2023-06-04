namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System.IO;
    using System.IO.Compression;
    using System.Numerics;
    using System.Text;

    public unsafe class Model
    {
        public ModelHeader Header;
        public string Name;
        public string MaterialLibrary;
        public readonly MeshData[] Meshes;
        public readonly Node Root;

        private Model(string path, Stream fs)
        {
            Name = path;

            Header.Read(fs);
            MaterialLibrary = Header.MaterialLibrary;

            Meshes = new MeshData[Header.MeshCount];

            var stream = fs;
            if (Header.Compression == Compression.Deflate)
            {
                stream = new DeflateStream(fs, CompressionMode.Decompress, true);
            }

            if (Header.Compression == Compression.LZ4)
            {
                stream = LZ4Stream.Decode(fs, 0, true);
            }

            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                Meshes[i] = MeshData.Read(stream, Encoding.UTF8, Header.Endianness);
            }

            Root = Node.ReadFrom(stream, Encoding.UTF8, Header.Endianness);

            fs.Close();
        }

        private Model(string path) : this(path, FileSystem.Open(path))
        {
        }

        public Model(string path, string materialLibrary, MeshData[] meshes, Node root)
        {
            Name = path;
            MaterialLibrary = materialLibrary;
            Header.MeshCount = (ulong)meshes.LongLength;
            Meshes = meshes;
            Root = root;
        }

        public void Save(string path, Encoding encoding, Endianness endianness = Endianness.LittleEndian, Compression compression = Compression.LZ4)
        {
            Stream fs = File.Create(path);

            Header.Encoding = encoding;
            Header.Endianness = endianness;
            Header.Compression = compression;
            Header.MaterialLibrary = MaterialLibrary;
            Header.Write(fs);

            var stream = fs;
            if (compression == Compression.Deflate)
            {
                stream = new DeflateStream(fs, CompressionLevel.SmallestSize, true);
            }

            if (compression == Compression.LZ4)
            {
                stream = LZ4Stream.Encode(fs, LZ4Level.L12_MAX, 0, true);
            }

            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                Meshes[i].Write(stream, Header.Encoding, Header.Endianness);
            }

            Root.Write(stream, Header.Encoding, Header.Endianness);

            stream.Close();
            fs.Close();
        }

        public static Model Load(string path)
        {
            return new Model(path);
        }

        public static Model LoadExternal(string path)
        {
            return new Model(path, File.OpenRead(path));
        }

        public MeshData GetMesh(int index)
        {
            return Meshes[index];
        }

        public MeshData GetMesh(ulong index)
        {
            return Meshes[index];
        }

        public Vector3[] GetPoints(int index)
        {
            var data = GetMesh(index);
            Vector3[] points = new Vector3[data.Indices.Length];
            for (int i = 0; i < data.Indices.Length; i++)
            {
                points[i] = data.Positions[data.Indices[i]];
            }
            return points;
        }

        public Vector3[] GetAllPoints()
        {
            ulong count = 0;
            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                count += GetMesh(i).IndicesCount;
            }
            Vector3[] points = new Vector3[count];
            ulong m = 0;
            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                var data = GetMesh(i);
                for (int j = 0; j < data.Indices.Length; j++)
                {
                    points[m++] = data.Positions[data.Indices[j]];
                }
            }
            return points;
        }

        public void Debone()
        {
            for (int i = 0; i < Meshes.Length; i++)
            {
                Meshes[i].Debone();
            }
        }
    }
}