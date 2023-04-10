namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Mathematics;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Numerics;
    using System.Text;

    public class ModelFile
    {
        public ModelHeader Header;
        public string Name;
        public readonly MeshData[] Meshes;
        public readonly Node Root;

        private ModelFile(string path, Stream fs)
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

        private ModelFile(string path) : this(path, FileSystem.Open(path))
        {
        }

        public ModelFile(string path, MeshData[] meshes)
        {
            Name = path;
            Header.MeshCount = (ulong)meshes.LongLength;
            Meshes = meshes;
        }

        public void Save(string dir, Encoding encoding, Endianness endianness = Endianness.LittleEndian, Compression compression = Compression.LZ4)
        {
            Directory.CreateDirectory(dir);
            Stream fs = File.Create(Path.Combine(dir, Path.GetFileNameWithoutExtension(Name) + ".model"));

            Header.Encoding = encoding;
            Header.Endianness = endianness;
            Header.Compression = compression;
            Header.Write(fs);

            var stream = fs;
            if (compression == Compression.Deflate)
                stream = new DeflateStream(fs, CompressionLevel.SmallestSize, true);
            if (compression == Compression.LZ4)
                stream = LZ4Stream.Encode(fs, LZ4Level.L12_MAX, 0, true);

            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                Meshes[i].Write(stream, Header.Encoding, Header.Endianness);
            }
            stream.Close();
            fs.Close();
        }

        public static ModelFile Load(string path)
        {
            return new ModelFile(path);
        }

        public static ModelFile LoadExternal(string path)
        {
            return new ModelFile(path, File.OpenRead(path + ".model"));
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

        public MaterialData[] GetMaterials()
        {
            MaterialData[] materials = new MaterialData[Meshes.Length];
            for (int i = 0; i < Meshes.Length; i++)
            {
                materials[i] = Meshes[i].Material;
            }
            return materials;
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