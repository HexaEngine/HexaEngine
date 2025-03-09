namespace HexaEngine.Core.Assets
{
    using System.Collections;

    public class SourceAssetMetadataGroup : IList<SourceAssetMetadata>
    {
        private readonly List<SourceAssetMetadata> items = [];

        public SourceAssetMetadata this[int index] { get => ((IList<SourceAssetMetadata>)items)[index]; set => ((IList<SourceAssetMetadata>)items)[index] = value; }

        public int Count => ((ICollection<SourceAssetMetadata>)items).Count;

        public bool IsReadOnly => ((ICollection<SourceAssetMetadata>)items).IsReadOnly;

        public void Add(SourceAssetMetadata item)
        {
            ((ICollection<SourceAssetMetadata>)items).Add(item);
        }

        public void Clear()
        {
            ((ICollection<SourceAssetMetadata>)items).Clear();
        }

        public bool Contains(SourceAssetMetadata item)
        {
            return ((ICollection<SourceAssetMetadata>)items).Contains(item);
        }

        public void CopyTo(SourceAssetMetadata[] array, int arrayIndex)
        {
            ((ICollection<SourceAssetMetadata>)items).CopyTo(array, arrayIndex);
        }

        public IEnumerator<SourceAssetMetadata> GetEnumerator()
        {
            return ((IEnumerable<SourceAssetMetadata>)items).GetEnumerator();
        }

        public int IndexOf(SourceAssetMetadata item)
        {
            return ((IList<SourceAssetMetadata>)items).IndexOf(item);
        }

        public void Insert(int index, SourceAssetMetadata item)
        {
            ((IList<SourceAssetMetadata>)items).Insert(index, item);
        }

        public bool Remove(SourceAssetMetadata item)
        {
            return ((ICollection<SourceAssetMetadata>)items).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<SourceAssetMetadata>)items).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)items).GetEnumerator();
        }
    }
}