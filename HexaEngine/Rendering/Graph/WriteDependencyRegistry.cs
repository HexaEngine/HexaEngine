namespace HexaEngine.Rendering.Graph
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    public class WriteDependencyRegistry : IDictionary<SubresourceName, Name>, IDictionary, IReadOnlyDictionary<SubresourceName, Name>, ISerializable, IDeserializationCallback
    {
        private readonly Dictionary<SubresourceName, Name> registry = new();

        public Name this[SubresourceName key] { get => ((IDictionary<SubresourceName, Name>)registry)[key]; set => ((IDictionary<SubresourceName, Name>)registry)[key] = value; }
        public object? this[object key] { get => ((IDictionary)registry)[key]; set => ((IDictionary)registry)[key] = value; }

        public ICollection<SubresourceName> Keys => ((IDictionary<SubresourceName, Name>)registry).Keys;

        public ICollection<Name> Values => ((IDictionary<SubresourceName, Name>)registry).Values;

        public int Count => ((ICollection<KeyValuePair<SubresourceName, Name>>)registry).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<SubresourceName, Name>>)registry).IsReadOnly;

        public bool IsFixedSize => ((IDictionary)registry).IsFixedSize;

        public bool IsSynchronized => ((ICollection)registry).IsSynchronized;

        public object SyncRoot => ((ICollection)registry).SyncRoot;

        ICollection IDictionary.Keys => ((IDictionary)registry).Keys;

        IEnumerable<SubresourceName> IReadOnlyDictionary<SubresourceName, Name>.Keys => ((IReadOnlyDictionary<SubresourceName, Name>)registry).Keys;

        ICollection IDictionary.Values => ((IDictionary)registry).Values;

        IEnumerable<Name> IReadOnlyDictionary<SubresourceName, Name>.Values => ((IReadOnlyDictionary<SubresourceName, Name>)registry).Values;

        public void Add(SubresourceName key, Name value)
        {
            ((IDictionary<SubresourceName, Name>)registry).Add(key, value);
        }

        public void Add(KeyValuePair<SubresourceName, Name> item)
        {
            ((ICollection<KeyValuePair<SubresourceName, Name>>)registry).Add(item);
        }

        public void Add(object key, object? value)
        {
            ((IDictionary)registry).Add(key, value);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<SubresourceName, Name>>)registry).Clear();
        }

        public bool Contains(KeyValuePair<SubresourceName, Name> item)
        {
            return ((ICollection<KeyValuePair<SubresourceName, Name>>)registry).Contains(item);
        }

        public bool Contains(object key)
        {
            return ((IDictionary)registry).Contains(key);
        }

        public bool ContainsKey(SubresourceName key)
        {
            return ((IDictionary<SubresourceName, Name>)registry).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<SubresourceName, Name>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<SubresourceName, Name>>)registry).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)registry).CopyTo(array, index);
        }

        public IEnumerator<KeyValuePair<SubresourceName, Name>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<SubresourceName, Name>>)registry).GetEnumerator();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ((ISerializable)registry).GetObjectData(info, context);
        }

        public void OnDeserialization(object? sender)
        {
            ((IDeserializationCallback)registry).OnDeserialization(sender);
        }

        public bool Remove(SubresourceName key)
        {
            return ((IDictionary<SubresourceName, Name>)registry).Remove(key);
        }

        public bool Remove(KeyValuePair<SubresourceName, Name> item)
        {
            return ((ICollection<KeyValuePair<SubresourceName, Name>>)registry).Remove(item);
        }

        public void Remove(object key)
        {
            ((IDictionary)registry).Remove(key);
        }

        public bool TryGetValue(SubresourceName key, [MaybeNullWhen(false)] out Name value)
        {
            return ((IDictionary<SubresourceName, Name>)registry).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)registry).GetEnumerator();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)registry).GetEnumerator();
        }
    }
}