namespace HexaEngine.Core.IO.Terrains
{
    using HexaEngine.Mathematics;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System.IO.Compression;
    using System.Text;

    /// <summary>
    /// Represents a terrain file containing information about terrain data, including a height map and level-of-detail (LOD) data.
    /// </summary>
    public class TerrainFile
    {
        private TerrainHeader header;
        private readonly HeightMap heightMap;
        private readonly List<TerrainCellData> lods;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainFile"/> class with default values.
        /// </summary>
        public TerrainFile()
        {
            header = default;
            heightMap = new();
            lods = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainFile"/> class with specified parameters.
        /// </summary>
        /// <param name="materialLibrary">The material library associated with the terrain.</param>
        /// <param name="x">The X-coordinate of the terrain.</param>
        /// <param name="y">The Y-coordinate of the terrain.</param>
        /// <param name="heightMap">The height map of the terrain.</param>
        public TerrainFile(string materialLibrary, int x, int y, HeightMap heightMap)
        {
            header = default;
            header.MaterialLibrary = materialLibrary;
            header.X = x;
            header.Y = y;
            this.heightMap = heightMap;
            lods = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainFile"/> class with specified parameters.
        /// </summary>
        /// <param name="materialLibrary">The material library associated with the terrain.</param>
        /// <param name="x">The X-coordinate of the terrain.</param>
        /// <param name="y">The Y-coordinate of the terrain.</param>
        /// <param name="heightMap">The height map of the terrain.</param>
        /// <param name="lods">The list of level-of-detail (LOD) data for the terrain.</param>
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

        /// <summary>
        /// Gets or sets the material library associated with the terrain.
        /// </summary>
        public string MaterialLibrary { get => header.MaterialLibrary; set => header.MaterialLibrary = value; }

        /// <summary>
        /// Gets or sets the X-coordinate of the terrain.
        /// </summary>
        public int X { get => header.X; set => header.X = value; }

        /// <summary>
        /// Gets or sets the Y-coordinate of the terrain.
        /// </summary>
        public int Y { get => header.Y; set => header.Y = value; }

        /// <summary>
        /// Gets the height map of the terrain.
        /// </summary>
        public HeightMap HeightMap => heightMap;

        /// <summary>
        /// Gets the list of level-of-detail (LOD) data for the terrain.
        /// </summary>
        public List<TerrainCellData> LODs => lods;

        /// <summary>
        /// Saves the terrain data to a file.
        /// </summary>
        /// <param name="path">The path where the terrain data will be saved.</param>
        /// <param name="encoding">The encoding used for the file.</param>
        /// <param name="endianness">The endianness used for binary data.</param>
        /// <param name="compression">The compression method used for the file.</param>
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

        /// <summary>
        /// Loads terrain data from a file.
        /// </summary>
        /// <param name="path">The path of the file containing terrain data.</param>
        /// <returns>A <see cref="TerrainFile"/> object representing the loaded terrain data.</returns>
        public static TerrainFile Load(string path)
        {
            return Load(FileSystem.OpenRead(path));
        }

        /// <summary>
        /// Loads terrain data from an external stream.
        /// </summary>
        /// <param name="path">The path of the file containing terrain data.</param>
        /// <returns>A <see cref="TerrainFile"/> object representing the loaded terrain data.</returns>
        public static TerrainFile LoadExternal(string path)
        {
            return Load(File.OpenRead(path));
        }

        /// <summary>
        /// Loads terrain data from a stream.
        /// </summary>
        /// <param name="fs">The stream containing terrain data.</param>
        /// <returns>A <see cref="TerrainFile"/> object representing the loaded terrain data.</returns>
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