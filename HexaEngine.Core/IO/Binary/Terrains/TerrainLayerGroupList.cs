namespace HexaEngine.Core.IO.Binary.Terrains
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Collections;
    using System.IO;

    public class TerrainLayerGroupList : IList<TerrainLayerGroup>
    {
        private readonly List<TerrainLayerGroup> groups;

        public TerrainLayerGroupList()
        {
            groups = new();
        }

        public TerrainLayerGroupList(int capacity)
        {
            groups = new(capacity);
        }

        public TerrainLayerGroupList(List<TerrainLayerGroup> groups)
        {
            this.groups = groups;
        }

        public TerrainLayerGroupList(IEnumerable<TerrainLayerGroup> groups)
        {
            this.groups = new(groups);
        }

        public TerrainLayerGroup this[int index] { get => groups[index]; set => groups[index] = value; }

        public int Count => groups.Count;

        public bool IsReadOnly => false;

        public void Add(TerrainLayerGroup item)
        {
            groups.Add(item);
        }

        public void Clear()
        {
            groups.Clear();
        }

        public bool Contains(TerrainLayerGroup item)
        {
            return groups.Contains(item);
        }

        public void CopyTo(TerrainLayerGroup[] array, int arrayIndex)
        {
            groups.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TerrainLayerGroup> GetEnumerator()
        {
            return groups.GetEnumerator();
        }

        public int IndexOf(TerrainLayerGroup item)
        {
            return groups.IndexOf(item);
        }

        public void Insert(int index, TerrainLayerGroup item)
        {
            groups.Insert(index, item);
        }

        public bool Remove(TerrainLayerGroup item)
        {
            return groups.Remove(item);
        }

        public void RemoveAt(int index)
        {
            groups.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return groups.GetEnumerator();
        }

        public void Write(Stream stream, Endianness endianness, List<TerrainLayerGroup> globalGroups)
        {
            stream.WriteInt32(groups.Count, endianness);
            for (int i = 0; i < groups.Count; i++)
            {
                int idx = globalGroups.IndexOf(groups[i]);
                stream.WriteInt32(idx, endianness);
            }
        }

        public static TerrainLayerGroupList Read(Stream stream, Endianness endianness, List<TerrainLayerGroup> groups)
        {
            int count = stream.ReadInt32(endianness);
            List<TerrainLayerGroup> localGroups = new(count);
            for (int i = 0; i < count; i++)
            {
                int idx = stream.ReadInt32(endianness);
                localGroups.Add(groups[idx]);
            }
            return new(localGroups);
        }
    }
}