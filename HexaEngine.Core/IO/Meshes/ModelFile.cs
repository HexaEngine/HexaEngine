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

    public unsafe class ModelFile
    {
        private ModelHeader header;
        private Node root;
        private readonly List<MeshData> meshes;

        public ModelFile()
        {
            header = default;

            root = new("ROOT", Matrix4x4.Identity, NodeFlags.None, null);
            meshes = new();
        }

        public ModelFile(string materialLibrary, IList<MeshData> meshes, Node root)
        {
            header.MaterialLibrary = materialLibrary;
            header.MeshCount = (ulong)meshes.Count;
            this.meshes = new(meshes);
            this.root = root;
        }

        public ModelHeader Header => header;

        public string MaterialLibrary { get => header.MaterialLibrary; set => header.MaterialLibrary = value; }

        public List<MeshData> Meshes => meshes;

        public Node Root => root;

        public void Save(string path, Encoding encoding, Endianness endianness = Endianness.LittleEndian, Compression compression = Compression.LZ4)
        {
            Stream fs = File.Create(path);

            header.Encoding = encoding;
            header.Endianness = endianness;
            header.Compression = compression;
            header.Write(fs);

            var stream = fs;
            if (compression == Compression.Deflate)
            {
                stream = new DeflateStream(fs, CompressionLevel.SmallestSize, true);
            }

            if (compression == Compression.LZ4)
            {
                stream = LZ4Stream.Encode(fs, LZ4Level.L12_MAX, 0, true);
            }

            for (int i = 0; i < (int)header.MeshCount; i++)
            {
                meshes[i].Write(stream, header.Encoding, header.Endianness);
            }

            root.Write(stream, header.Encoding, header.Endianness);

            stream.Close();
            fs.Close();
        }

        public static ModelFile Load(string path)
        {
            return Load(FileSystem.Open(path));
        }

        public static ModelFile LoadExternal(string path)
        {
            return Load(File.OpenRead(path));
        }

        public static ModelFile Load(Stream fs)
        {
            ModelFile model = new();
            model.header.Read(fs);

            model.meshes.Clear();
            model.meshes.Capacity = (int)model.header.MeshCount;

            var stream = fs;
            if (model.header.Compression == Compression.Deflate)
            {
                stream = new DeflateStream(fs, CompressionMode.Decompress, true);
            }

            if (model.header.Compression == Compression.LZ4)
            {
                stream = LZ4Stream.Decode(fs, 0, true);
            }

            for (int i = 0; i < (int)model.header.MeshCount; i++)
            {
                model.meshes.Add(MeshData.Read(stream, model.Header.Encoding, model.header.Endianness));
            }

            model.root = Node.ReadFrom(stream, model.Header.Encoding, model.header.Endianness);

            stream.Close();
            fs.Close();

            return model;
        }

        public MeshData GetMesh(int index)
        {
            return meshes[index];
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
            for (int i = 0; i < (int)header.MeshCount; i++)
            {
                count += GetMesh(i).IndicesCount;
            }
            Vector3[] points = new Vector3[count];
            ulong m = 0;
            for (int i = 0; i < (int)header.MeshCount; i++)
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
            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].Debone();
            }
        }
    }
}