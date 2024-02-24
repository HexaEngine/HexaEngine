namespace HexaEngine.Core.IO.Binary.Terrains
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Mathematics;
    using System.IO;

    /// <summary>
    /// Represents a terrain mesh generated from a height map.
    /// </summary>
    public unsafe class TerrainCellData : IMeshData
    {
        private Point2 position;
        private Guid guid;
        private HeightMap heightMap = new();
        private TerrainLayerGroupList layerGroups = [];
        private TerrainCellLODDataSeekTable seekTable = new();
        private List<TerrainCellLODData> lods = [];
        private Endianness endianness;
        private Compression compression;

        public TerrainCellData(Point2 position, Guid guid, HeightMap heightMap, TerrainLayerGroupList layerGroups, TerrainCellLODDataSeekTable seekTable, List<TerrainCellLODData> lods)
        {
            this.position = position;
            this.guid = guid;
            this.heightMap = heightMap;
            this.layerGroups = layerGroups;
            this.seekTable = seekTable;
            this.lods = lods;
        }

        public TerrainCellData(Point2 position, Guid guid, HeightMap heightMap)
        {
            this.position = position;
            this.guid = guid;
            this.heightMap = heightMap;
            layerGroups = [];
            seekTable = new();
            lods = [];
        }

        public TerrainCellData(Point2 position, HeightMap heightMap)
        {
            this.position = position;
            guid = Guid.NewGuid();
            this.heightMap = heightMap;
            layerGroups = [];
            seekTable = new();
            lods = [];
        }

        public TerrainCellData(Point2 position, Guid guid)
        {
            this.position = position;
            this.guid = guid;
            heightMap = new();
            layerGroups = [];
            seekTable = new();
            lods = [];
        }

        public TerrainCellData(Point2 position)
        {
            this.position = position;
            guid = Guid.NewGuid();
            heightMap = new();
            layerGroups = [];
            seekTable = new();
            lods = [];
        }

        public Point2 Position { get => position; set => position = value; }

        public string Name => position.ToString();

        public Guid Guid => guid;

        public HeightMap HeightMap { get => heightMap; set => heightMap = value; }

        public TerrainLayerGroupList LayerGroups { get => layerGroups; set => layerGroups = value; }

        public TerrainCellLODDataSeekTable SeekTable { get => seekTable; set => seekTable = value; }

        public List<TerrainCellLODData> LODs { get => lods; set => lods = value; }

        InputElementDescription[] IMeshData.InputElements => InputElements;

        /// <summary>
        /// Reads terrain cell data from a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The source stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading binary data.</param>
        /// <param name="compression"></param>
        /// <param name="mode"></param>
        /// <param name="groups"></param>
        /// <returns>A <see cref="TerrainCellData"/> instance populated with data from the stream.</returns>
        public static TerrainCellData Read(Stream stream, Endianness endianness, Compression compression, TerrainLoadMode mode, List<TerrainLayerGroup> groups)
        {
            Point2 position = Point2.Read(stream, endianness);
            Guid guid = stream.ReadGuid(endianness);
            HeightMap heightMap = HeightMap.ReadFrom(stream, endianness, compression, mode);

            var layerGroups = TerrainLayerGroupList.Read(stream, endianness, groups);

            TerrainCellLODDataSeekTable seekTable = new();
            seekTable.Read(stream, endianness);

            List<TerrainCellLODData> lods = new(seekTable.Entries.Count);
            if (mode == TerrainLoadMode.Immediate)
            {
                long lastPosEnd = stream.Position;
                for (int i = 0; i < seekTable.Entries.Count; i++)
                {
                    var entry = seekTable.Entries[i];
                    stream.Position = entry.Offset;

                    Stream decompressor = stream.CreateDecompressionStream(compression, out var isCompressed);

                    TerrainCellLODData lod = TerrainCellLODData.Read(decompressor, endianness);
                    lods.Add(lod);

                    if (isCompressed)
                    {
                        decompressor.Dispose();
                    }

                    lastPosEnd = entry.Offset + entry.Size;
                }

                stream.Position = lastPosEnd;
            }
            else
            {
                for (int i = 0; i < seekTable.Entries.Count; i++)
                {
                    var entry = seekTable.Entries[i];
                    stream.Position += entry.Size;
                }
            }

            TerrainCellData data = new(position, guid, heightMap, layerGroups, seekTable, lods);
            data.endianness = endianness;
            data.compression = compression;
            return data;
        }

        /// <summary>
        /// Writes the terrain cell data to a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The destination stream to write to.</param>
        /// <param name="endianness">The endianness to use for writing binary data.</param>
        /// <param name="compression"></param>
        /// <param name="groups"></param>
        public void Write(Stream stream, Endianness endianness, Compression compression, List<TerrainLayerGroup> groups)
        {
            position.Write(stream, endianness);
            stream.WriteGuid(guid, endianness);
            heightMap.Write(stream, endianness, compression);
            layerGroups.Write(stream, endianness, groups);

            seekTable.Clear();

            long baseOffset = stream.Position;

            stream.Position += TerrainCellLODDataSeekTable.GetSize(lods.Count);

            long last = stream.Position;
            for (int i = 0; i < lods.Count; i++)
            {
                Stream compressor = stream.CreateCompressionStream(compression, out var isCompressed);

                TerrainCellLODData lod = lods[i];
                lod.Write(compressor, endianness);

                if (isCompressed)
                {
                    compressor.Flush();
                    compressor.Dispose();
                }

                long now = stream.Position;
                long size = now - last;
                seekTable.Entries.Add(new(lod.LODLevel, (uint)last, (uint)size));
                last = now;
            }

            long before = stream.Position;
            stream.Position = baseOffset;
            seekTable.Write(stream, endianness);
            stream.Position = before;
        }

        /// <summary>
        /// The input element descriptions for the terrain vertex format.
        /// </summary>
        public static readonly InputElementDescription[] InputElements =
        [
            new("POSITION", 0, Format.R32G32B32Float, 0),
            new("TEXCOORD", 0, Format.R32G32Float, 0),
            new("NORMAL", 0, Format.R32G32B32Float, 0),
            new("TANGENT", 0, Format.R32G32B32Float, 0),
        ];

        public void LoadHeightMapData(Stream stream)
        {
            heightMap.LoadHeightMapData(stream, endianness);
        }

        public TerrainCellLODData LoadLODData(int lodIndex, Stream stream)
        {
            if (LODs.Count != 0)
            {
                return LODs[lodIndex];
            }

            long offset = seekTable.GetOffsetFromIndex(lodIndex);
            stream.Position = offset;

            Stream decompressor = stream.CreateDecompressionStream(compression, out var isCompressed);
            TerrainCellLODData data = TerrainCellLODData.Read(decompressor, endianness);

            if (isCompressed)
            {
                decompressor.Dispose();
            }

            return data;
        }

        public void GenerateLOD(int levels = 4)
        {
            int tessellation = 5;
            for (int i = 0; i < levels; i++, tessellation--)
            {
                uint lodLevel = i == 0 ? 0 : (uint)Math.Pow(2, i);
                TerrainCellLODData lod;
                if (LODs.Count == i)
                {
                    lod = new(lodLevel, 32, 32, heightMap, tessellation);
                    LODs.Add(lod);
                }
                else
                {
                    lod = LODs[i];
                    lod.Generate(heightMap);
                }
            }
        }

        public void GenerateLODLevel(int level)
        {
            TerrainCellLODData lod = LODs[level];
            lod.Generate(heightMap);
        }

        public void GenerateIndicesAndUVsLevel(int level)
        {
            LODs[level].GenerateIndicesAndUVs();
        }

        public void AverageEdge(Edge xPos, TerrainCellData other)
        {
            if (lods.Count == 0 || other.lods.Count == 0)
            {
                throw new InvalidOperationException("No LOD data available");
            }
            if (lods.Count != other.lods.Count)
            {
                throw new InvalidOperationException("LOD count must match");
            }

            for (int i = 0; i < other.seekTable.Entries.Count; i++)
            {
                var lodOther = other.lods[i];
                var lodSelf = lods[i];
                lodSelf.AverageEdge(xPos, lodOther);
            }
        }

        public void AverageEdgeLevel(int level, Edge xPos, TerrainCellData other)
        {
            if (lods.Count == 0 || other.lods.Count == 0)
            {
                throw new InvalidOperationException("No LOD data available");
            }
            if (lods.Count != other.lods.Count)
            {
                throw new InvalidOperationException("LOD count must match");
            }

            var lodOther = other.lods[level];
            var lodSelf = lods[level];
            lodSelf.AverageEdge(xPos, lodOther);
        }
    }
}