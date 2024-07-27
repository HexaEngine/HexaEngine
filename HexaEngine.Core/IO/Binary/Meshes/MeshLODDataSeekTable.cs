namespace HexaEngine.Core.IO.Binary.Meshes
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.IO;
    using System.Collections.Generic;
    using System.IO;

    public class MeshLODDataSeekTable
    {
        public struct Entry
        {
            public uint LODLevel;
            public uint Offset;
            public uint Size;

            public Entry(uint lODLevel, uint offset, uint size)
            {
                LODLevel = lODLevel;
                Offset = offset;
                Size = size;
            }
        }

        public List<Entry> Entries = [];

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

        public long GetOffsetFromIndex(int index)
        {
            return Entries[index].Offset;
        }

        public static unsafe uint GetSize(int entryCount)
        {
            return (uint)(entryCount * sizeof(Entry)) + 4;
        }

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

        public void Clear()
        {
            Entries.Clear();
        }
    }
}