namespace HexaEngine.Core.IO.Binary.Terrains
{
    using HexaEngine.Mathematics;

    /// <summary>
    /// Represents a group of terrain layers.
    /// </summary>
    public class TerrainLayerGroup
    {
        private readonly TerrainLayer[] layers = new TerrainLayer[MaxLayers];
        private int count;

        /// <summary>
        /// The maximum number of layers allowed in a terrain layer group.
        /// </summary>
        public const int MaxLayers = 4;

        /// <summary>
        /// The number of layers in the group.
        /// </summary>
        public int Count => count;

        /// <summary>
        /// The mask associated with the layer group.
        /// </summary>
        public LayerMask Mask = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainLayerGroup"/> class.
        /// </summary>
        public TerrainLayerGroup()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainLayerGroup"/> class with specified parameters.
        /// </summary>
        /// <param name="layers">The array of terrain layers in the group.</param>
        /// <param name="layerCount">The number of layers in the group.</param>
        /// <param name="mask">The mask associated with the layer group.</param>
        public TerrainLayerGroup(TerrainLayer[] layers, int layerCount, LayerMask mask)
        {
            this.layers = layers;
            count = layerCount;
            Mask = mask;
        }

        public TerrainLayer this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                {
                    throw new IndexOutOfRangeException();
                }
                return layers[index];
            }
            set
            {
                if (index < 0 || index >= count)
                {
                    throw new IndexOutOfRangeException();
                }
                layers[index] = value;
            }
        }

        public bool Add(TerrainLayer layer)
        {
            if (count == MaxLayers || Contains(layer))
            {
                return false;
            }

            int index = count;
            count++;
            layers[index] = layer;

            return true;
        }

        public bool Contains(TerrainLayer layer)
        {
            for (int i = 0; i < count; i++)
            {
                var l = layers[i];
                if (l == layer)
                {
                    return true;
                }
            }
            return false;
        }

        public int IndexOf(TerrainLayer layer)
        {
            for (int i = 0; i < count; i++)
            {
                var l = layers[i];
                if (l == layer)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool Remove(TerrainLayer layer)
        {
            int index = IndexOf(layer);
            if (index == -1)
            {
                return false;
            }
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index == count - 1)
            {
                layers[index] = null;
                count--;
                return;
            }

            Array.Copy(layers, index + 1, layers, index, count - index);
            count--;
            return;
        }

        /// <summary>
        /// Writes the terrain layer group data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="endianness">The endianness used for binary data.</param>
        /// <param name="compression"></param>
        /// <param name="layers">The list of terrain layers used for indexing.</param>
        public void Write(Stream stream, Endianness endianness, Compression compression, List<TerrainLayer> layers)
        {
            stream.WriteInt32(count, endianness);
            for (int i = 0; i < count; i++)
            {
                int idx = layers.IndexOf(this.layers[i]);
                stream.WriteInt32(idx, endianness);
            }
            Mask.Write(stream, endianness, compression);
        }

        /// <summary>
        /// Reads terrain layer group data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness used for binary data.</param>
        /// <param name="compression"></param>
        /// <param name="mode"></param>
        /// <param name="layers">The list of terrain layers used for indexing.</param>
        /// <returns>A new instance of <see cref="TerrainLayerGroup"/> containing the read data.</returns>
        public static TerrainLayerGroup Read(Stream stream, Endianness endianness, Compression compression, TerrainLoadMode mode, List<TerrainLayer> layers)
        {
            var layersInGroup = new TerrainLayer[MaxLayers];
            var layerCount = stream.ReadInt32(endianness);
            for (int i = 0; i < layerCount; i++)
            {
                int idx = stream.ReadInt32(endianness);
                if (idx == -1)
                {
                    continue;
                }
                layersInGroup[i] = layers[idx];
            }
            LayerMask layerMask = LayerMask.ReadFrom(stream, endianness, compression, mode);
            return new TerrainLayerGroup(layersInGroup, layerCount, layerMask);
        }
    }
}