namespace HexaEngine.Core.IO.Binary.Terrains
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a terrain file containing data about cells, layers, and layer groups.
    /// </summary>
    public class TerrainFile
    {
        private readonly List<TerrainCellData> cells;
        private readonly List<TerrainLayer> layers;
        private readonly List<TerrainLayerGroup> layerGroups;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainFile"/> class with default values.
        /// </summary>
        public TerrainFile()
        {
            cells = new();
            layers = new();
            layerGroups = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainFile"/> class with specified parameters.
        /// </summary>
        /// <param name="cells">The list of cells data for the terrain.</param>
        /// <param name="layers">The list of layers for the terrain.</param>
        /// <param name="layerGroups">The list of layer groups for the terrain.</param>
        public TerrainFile(IEnumerable<TerrainCellData> cells, IEnumerable<TerrainLayer> layers, IEnumerable<TerrainLayerGroup> layerGroups)
        {
            this.cells = new(cells);
            this.layers = new(layers);
            this.layerGroups = new(layerGroups);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainFile"/> class with specified parameters.
        /// </summary>
        /// <param name="cells">The list of cells data for the terrain.</param>
        /// <param name="layers">The list of layers for the terrain.</param>
        /// <param name="layerGroups">The list of layer groups for the terrain.</param>
        public TerrainFile(List<TerrainCellData> cells, List<TerrainLayer> layers, List<TerrainLayerGroup> layerGroups)
        {
            this.cells = cells;
            this.layers = layers;
            this.layerGroups = layerGroups;
        }

        /// <summary>
        /// Gets the list of cells for the terrain.
        /// </summary>
        public List<TerrainCellData> Cells => cells;

        /// <summary>
        /// Gets the list of layers for the terrain.
        /// </summary>
        public List<TerrainLayer> Layers => layers;

        /// <summary>
        /// Gets the list of layer groups for the terrain.
        /// </summary>
        public List<TerrainLayerGroup> LayerGroups => layerGroups;

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
            try
            {
                Save(fs, encoding, endianness, compression);
            }
            finally
            {
                fs.Close();
            }
        }

        /// <summary>
        /// Saves the terrain data to a stream.
        /// </summary>
        /// <param name="stream">The stream where the terrain data will be saved.</param>
        /// <param name="encoding">The encoding used for the file.</param>
        /// <param name="endianness">The endianness used for binary data.</param>
        /// <param name="compression">The compression method used for the file.</param>
        public void Save(Stream stream, Encoding encoding, Endianness endianness, Compression compression)
        {
            TerrainHeader header = default;
            header.Encoding = encoding;
            header.Endianness = endianness;
            header.Compression = compression;
            header.Layers = layers.Count;
            header.LayerGroups = layerGroups.Count;
            header.Cells = cells.Count;
            header.Write(stream);

            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].Write(stream, encoding, endianness);
            }

            for (int i = 0; i < layerGroups.Count; i++)
            {
                layerGroups[i].Write(stream, endianness, compression, layers);
            }

            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].Write(stream, compression, endianness, layerGroups);
            }
        }

        /// <summary>
        /// Loads terrain data from a stream.
        /// </summary>
        /// <param name="stream">The stream containing terrain data.</param>
        /// <param name="mode">The mode to use for loading terrain data.</param>
        /// <returns>A <see cref="TerrainFile"/> object representing the loaded terrain data.</returns>
        public static TerrainFile Load(Stream stream, TerrainLoadMode mode)
        {
            TerrainHeader header = default;
            header.Read(stream);

            List<TerrainLayer> layers = new(header.Layers);
            List<TerrainLayerGroup> layerGroups = new(header.LayerGroups);
            List<TerrainCellData> cells = new(header.Cells);

            for (int i = 0; i < header.Layers; i++)
            {
                layers.Add(TerrainLayer.Read(stream, header.Encoding, header.Endianness));
            }

            for (int i = 0; i < header.LayerGroups; i++)
            {
                layerGroups.Add(TerrainLayerGroup.Read(stream, header.Endianness, header.Compression, layers));
            }

            for (int i = 0; i < header.Cells; i++)
            {
                cells.Add(TerrainCellData.Read(stream, header.Compression, header.Endianness, mode, layerGroups));
            }

            return new TerrainFile(cells, layers, layerGroups);
        }
    }
}