namespace HexaEngine.Core.Collections
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    /// <summary>
    /// Represents a generic collection that provides notifications when items are added, removed, or cleared.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class ObservableList<T> : IList<T>, INotifyCollectionChanged
    {
        private readonly IList<T> _internalList;

        /// <summary>
        /// Occurs when the collection changes, either by adding or removing an item.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableList{T}"/> class.
        /// </summary>
        /// <param name="list">The internal list to be wrapped by the observable list.</param>
        public ObservableList(IList<T> list)
        {
            _internalList = list;
        }

        /// <inheritdoc/>
        public T this[int index]
        {
            get { return _internalList[index]; }
            set { _internalList[index] = value; }
        }

        /// <inheritdoc/>
        public int Count => _internalList.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public void Add(T item)
        {
            _internalList.Add(item);
            RaiseCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            var result = _internalList.Remove(item);
            RaiseCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            return result;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _internalList.Clear();
            RaiseCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }    /// <inheritdoc/>

        public bool Contains(T item)
        {
            return _internalList.Contains(item);
        }

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            return _internalList.IndexOf(item);
        }

        /// <inheritdoc/>
        public void Insert(int index, T item)
        {
            _internalList.Insert(index, item);
            RaiseCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            _internalList.RemoveAt(index);
            RaiseCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _internalList[index], index));
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _internalList.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        private void RaiseCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(sender, args);
        }
    }
}