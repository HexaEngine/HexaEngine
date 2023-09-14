namespace HexaEngine.Rendering.Graph
{
    using HexaEngine.Graph;
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    public class ResourceRefCollection<T> : IList<ResourceRef<T>>, INotifyCollectionChanged where T : class, IDisposable
    {
        private readonly List<T?> list = new();
        private readonly List<ResourceRef<T>> resourceRefList = new();

        public ResourceRefCollection()
        {
            list = new List<T?>();
            resourceRefList = new List<ResourceRef<T>>();
        }

        public ResourceRefCollection(int initialCapacity)
        {
            list = new List<T?>(initialCapacity);
            resourceRefList = new List<ResourceRef<T>>(initialCapacity);
        }

        public ResourceRefCollection(IEnumerable<ResourceRef<T>> resourceRefs)
        {
            list = new List<T?>(resourceRefs.Count());
            resourceRefList = new List<ResourceRef<T>>(resourceRefs);

            foreach (var resourceRef in resourceRefs)
            {
                list.Add(resourceRef.Value);
                resourceRef.ValueChanged += ValueChanged;
            }
        }

        public T? this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        ResourceRef<T> IList<ResourceRef<T>>.this[int index]
        {
            get => resourceRefList[index];
            set => resourceRefList[index] = value;
        }

        public int Count => resourceRefList.Count;

        public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public void Add(ResourceRef<T> item)
        {
            resourceRefList.Add(item);
            item.ValueChanged += ValueChanged;
            list.Add(item.Value);
        }

        public void Clear()
        {
            for (int i = 0; i < resourceRefList.Count; i++)
            {
                resourceRefList[i].ValueChanged -= ValueChanged;
            }
            resourceRefList.Clear();
            list.Clear();
        }

        public bool Contains(ResourceRef<T> item)
        {
            return resourceRefList.Contains(item);
        }

        public void CopyTo(ResourceRef<T>[] array, int arrayIndex)
        {
            resourceRefList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ResourceRef<T>> GetEnumerator()
        {
            return ((IEnumerable<ResourceRef<T>>)resourceRefList).GetEnumerator();
        }

        public int IndexOf(ResourceRef<T> item)
        {
            return resourceRefList.IndexOf(item);
        }

        public void Insert(int index, ResourceRef<T> item)
        {
            resourceRefList.Insert(index, item);
        }

        public bool Remove(ResourceRef<T> item)
        {
            var idx = resourceRefList.IndexOf(item);
            if (idx != -1)
            {
                item.ValueChanged -= ValueChanged;
                resourceRefList.RemoveAt(idx);
                list.RemoveAt(idx);
                CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item, idx));
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            var resourceRef = resourceRefList[index];
            resourceRef.ValueChanged -= ValueChanged;
            resourceRefList.RemoveAt(index);
            list.RemoveAt(index);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, resourceRef, index));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)resourceRefList).GetEnumerator();
        }

        private void ValueChanged(object? sender, T? e)
        {
            var idx = resourceRefList.IndexOf((ResourceRef<T>)sender);
            list[idx] = e;
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, e, idx));
        }
    }
}