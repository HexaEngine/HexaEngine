namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Mathematics;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System.IO;
    using System.IO.Compression;
    using System.Numerics;
    using System.Text;

    public class ModelSource
    {
        public ModelHeader Header;
        private readonly MeshData[] Meshes;
        public string Name;

        private ModelSource(string path)
        {
            Name = path;
            Stream fs = FileSystem.Open(path);
            Header.Read(fs);

            Meshes = new MeshData[Header.MeshCount];

            var stream = fs;
            if (Header.Compression == Compression.Deflate)
                stream = new DeflateStream(fs, CompressionMode.Decompress, true);
            if (Header.Compression == Compression.LZ4)
                stream = LZ4Stream.Decode(fs, 0, true);

            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                Meshes[i] = MeshData.Read(stream, Encoding.UTF8, Header.Endianness);
            }
            stream.Close();
            fs.Close();
        }

        public ModelSource(string path, Stream fs)
        {
            Name = path;
            Header.Read(fs);

            Meshes = new MeshData[Header.MeshCount];

            var stream = fs;
            if (Header.Compression == Compression.Deflate)
                stream = new DeflateStream(fs, CompressionMode.Decompress, true);
            if (Header.Compression == Compression.LZ4)
                stream = LZ4Stream.Decode(fs, 0, true);

            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                Meshes[i] = MeshData.Read(stream, Encoding.UTF8, Header.Endianness);
            }

            fs.Close();
        }

        public ModelSource(string path, MeshData[] meshes)
        {
            Name = path;
            Header.Endianness = Endianness.LittleEndian;
            Header.MeshCount = (ulong)meshes.LongLength;
            Meshes = meshes;
        }

        public void Save(string dir, Encoding encoding, Compression compression = Compression.LZ4)
        {
            Directory.CreateDirectory(dir);
            Stream fs = File.Create(Path.Combine(dir, Name + ".model"));

            Header.Encoding = encoding;
            Header.Compression = compression;
            Header.Write(fs);

            var stream = fs;
            if (compression == Compression.Deflate)
                stream = new DeflateStream(fs, CompressionLevel.SmallestSize, true);
            if (compression == Compression.LZ4)
                stream = LZ4Stream.Encode(fs, LZ4Level.L12_MAX, 0, true);
            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                Meshes[i].Write(stream, encoding, Header.Endianness);
            }
            stream.Close();
            fs.Close();
        }

        public static ModelSource Load(string path)
        {
            return new ModelSource(path);
        }

        public static ModelSource LoadExternal(string path)
        {
            return new ModelSource(path, File.OpenRead(path + ".model"));
        }

        public void SetMaterial(int index, MaterialData material)
        {
            Meshes[index].Material = material;
        }

        public void SetMaterial(ulong index, MaterialData material)
        {
            Meshes[index].Material = material;
        }

        public MeshData GetMesh(int index)
        {
            return Meshes[index];
        }

        public MeshData GetMesh(ulong index)
        {
            return Meshes[index];
        }

        public MaterialData GetMaterial(int index)
        {
            return Meshes[index].Material;
        }

        public MaterialData GetMaterial(ulong index)
        {
            return Meshes[index].Material;
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
    }
}