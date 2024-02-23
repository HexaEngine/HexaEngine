namespace HexaEngine.Core.IO.Binary.Terrains
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.IO;

    /// <summary>
    /// Represents a seek table for LOD data associated with terrain cells.
    /// </summary>
    public class TerrainCellLODDataSeekTable
    {
        /// <summary>
        /// Represents an entry in the seek table.
        /// </summary>
        public struct Entry
        {
            /// <summary>
            /// The level of detail (LOD) associated with the entry.
            /// </summary>
            public uint LODLevel;

            /// <summary>
            /// The offset in the stream where the LOD data begins.
            /// </summary>
            public uint Offset;

            /// <summary>
            /// The size of the LOD data.
            /// </summary>
            public uint Size;

            /// <summary>
            /// Initializes a new instance of the <see cref="Entry"/> struct with specified parameters.
            /// </summary>
            /// <param name="lODLevel">The level of detail (LOD) associated with the entry.</param>
            /// <param name="offset">The offset in the stream where the LOD data begins.</param>
            /// <param name="size">The size of the LOD data.</param>
            public Entry(uint lODLevel, uint offset, uint size)
            {
                LODLevel = lODLevel;
                Offset = offset;
                Size = size;
            }
        }

        /// <summary>
        /// The list of entries in the seek table.
        /// </summary>
        public List<Entry> Entries = new List<Entry>();

        /// <summary>
        /// Gets the offset in the stream for the specified LOD level.
        /// </summary>
        /// <param name="lodLevel">The level of detail (LOD) to get the offset for.</param>
        /// <returns>The offset in the stream for the specified LOD level, or -1 if not found.</returns>
        public long GetOffset(uint lodLevel)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                if (entry.LODLevel == lodLevel)
                {
                    return entry.Offset;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the offset in the stream for the entry at the specified index.
        /// </summary>
        /// <param name="index">The index of the entry to get the offset for.</param>
        /// <returns>The offset in the stream for the entry at the specified index.</returns>
        public long GetOffsetFromIndex(int index)
        {
            return Entries[index].Offset;
        }

        /// <summary>
        /// Calculates the size of the seek table.
        /// </summary>
        /// <param name="entryCount">The number of entries in the seek table.</param>
        /// <returns>The size of the seek table in bytes.</returns>
        public static unsafe uint GetSize(int entryCount)
        {
            return (uint)(entryCount * sizeof(Entry)) + 4;
        }

        /// <summary>
        /// Reads the seek table data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness used for binary data.</param>
        public void Read(Stream stream, Endianness endianness)
        {
            int entryCount = stream.ReadInt32(endianness);
            Entries.Clear();
            Entries.Capacity = entryCount;
            for (int i = 0; i < entryCount; i++)
            {
                Entry entry;
                entry.LODLevel = stream.ReadUInt32(endianness);
                entry.Offset = stream.ReadUInt32(endianness);
                entry.Size = stream.ReadUInt32(endianness);
                Entries.Add(entry);
            }
        }

        /// <summary>
        /// Writes the seek table data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="endianness">The endianness used for binary data.</param>
        public void Write(Stream stream, Endianness endianness)
        {
            stream.WriteInt32(Entries.Count, endianness);
            for (int i = 0; i < Entries.Count; i++)
            {
                Entry entry = Entries[i];
                stream.WriteUInt32(entry.LODLevel, endianness);
                stream.WriteUInt32(entry.Offset, endianness);
                stream.WriteUInt32(entry.Size, endianness);
            }
        }

        /// <summary>
        /// Clears the seek table.
        /// </summary>
        public void Clear()
        {
            Entries.Clear();
        }
    }
}