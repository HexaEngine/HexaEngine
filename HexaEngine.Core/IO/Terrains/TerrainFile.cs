namespace HexaEngine.Core.IO.Terrains
{
    using HexaEngine.Mathematics;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System.IO.Compression;
    using System.Text;

    public class TerrainFile
    {
        private TerrainHeader header;
        private readonly HeightMap heightMap;
        private readonly List<TerrainCellData> lods;

        public TerrainFile()
        {
            header = default;
            heightMap = new();
            lods = new();
        }

        public TerrainFile(string materialLibrary, int x, int y, HeightMap heightMap)
        {
            header = default;
            header.MaterialLibrary = materialLibrary;
            header.X = x;
            header.Y = y;
            this.heightMap = heightMap;
            lods = new();
        }

        public TerrainFile(string materialLibrary, int x, int y, HeightMap heightMap, IList<TerrainCellData> lods)
        {
            header = default;
            header.MaterialLibrary = materialLibrary;
            header.X = x;
            header.Y = y;
            header.LODLevels = (uint)lods.Count;
            this.heightMap = heightMap;
            this.lods = new(lods);
        }

        public string MaterialLibrary { get => header.MaterialLibrary; set => header.MaterialLibrary = value; }

        public int X { get => header.X; set => header.X = value; }

        public int Y { get => header.Y; set => header.Y = value; }

        public HeightMap HeightMap => heightMap;

        public List<TerrainCellData> LODs => lods;

        public void Save(string path, Encoding encoding, Endianness endianness, Compression compression)
        {
            Stream fs = File.Create(path);

            header.Encoding = encoding;
            header.Endianness = endianness;
            header.Compression = compression;
            header.LODLevels = (uint)lods.Count;
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

            heightMap.Write(stream, encoding, endianness);

            for (int i = 0; i < lods.Count; i++)
            {
                lods[i].Write(stream, encoding, endianness);
            }

            stream.Close();
            fs.Close();
        }

        public static TerrainFile Load(string path)
        {
            return Load(FileSystem.OpenRead(path));
        }

        public static TerrainFile LoadExternal(string path)
        {
            return Load(File.OpenRead(path));
        }

        public static TerrainFile Load(Stream fs)
        {
            TerrainFile terrain = new();

            terrain.header.Read(fs);
            var header = terrain.header;

            terrain.lods.Clear();
            terrain.lods.Capacity = (int)header.LODLevels;

            var stream = fs;
            if (header.Compression == Compression.Deflate)
            {
                stream = new DeflateStream(fs, CompressionMode.Decompress, true);
            }

            if (header.Compression == Compression.LZ4)
            {
                stream = LZ4Stream.Decode(fs, 0, true);
            }

            terrain.heightMap.Read(stream, header.Encoding, header.Endianness);

            for (int i = 0; i < (int)header.LODLevels; i++)
            {
                terrain.lods.Add(TerrainCellData.Read(stream, header.Encoding, header.Endianness));
            }

            return terrain;
        }
    }
}