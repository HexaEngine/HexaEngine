namespace HexaEngine.Core.Collections
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents an observable dictionary that allows you to observe changes to its contents.
    /// </summary>
    [Serializable]
    public class ObservableDictionary<TKey, TValue> : ObservableCollection<ObservableKeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue> where TKey : notnull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        public ObservableDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class with an existing collection.
        /// </summary>
        /// <param name="collection">An existing collection of key-value pairs.</param>
        public ObservableDictionary(IEnumerable<ObservableKeyValuePair<TKey, TValue>> collection) : base(collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class with an existing list.
        /// </summary>
        /// <param name="list">An existing list of key-value pairs.</param>
        public ObservableDictionary(List<ObservableKeyValuePair<TKey, TValue>> list) : base(list)
        {
        }

        /// <summary>
        /// Adds a key-value pair to the dictionary.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value associated with the key.</param>
        public virtual void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                throw new ArgumentException("The dictionary already contains the key");
            }
            Add(new ObservableKeyValuePair<TKey, TValue>() { Key = key, Value = value });
        }

        /// <summary>
        /// Determines whether the dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns><c>true</c> if the dictionary contains the key; otherwise, <c>false</c>.</returns>
        public virtual bool ContainsKey(TKey key)
        {
            var r = ThisAsCollection().FirstOrDefault((i) => ObservableDictionary<TKey, TValue>.Equals(key, i.Key));

            return !Equals(default(ObservableKeyValuePair<TKey, TValue>), r);
        }

        /// <summary>
        /// Determines whether two keys are equal using the default equality comparer for the key type.
        /// </summary>
        /// <param name="a">The first key to compare.</param>
        /// <param name="b">The second key to compare.</param>
        /// <returns><c>true</c> if the keys are equal; otherwise, <c>false</c>.</returns>
        private static bool Equals(TKey a, TKey b)
        {
            return EqualityComparer<TKey>.Default.Equals(a, b);
        }

        /// <summary>
        /// Converts the dictionary to an ObservableCollection of key-value pairs.
        /// </summary>
        /// <returns>An ObservableCollection of key-value pairs.</returns>
        private ObservableCollection<ObservableKeyValuePair<TKey, TValue>> ThisAsCollection()
        {
            return this;
        }

        /// <summary>
        /// Gets a collection containing the keys of the dictionary.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return (from i in ThisAsCollection() select i.Key).ToList(); }
        }

        /// <summary>
        /// Removes all elements from the dictionary.
        /// </summary>
        public virtual new void Clear()
        {
            base.Clear();
        }

        /// <summary>
        /// Removes the element with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><c>true</c> if the element is successfully removed; otherwise, <c>false</c>.</returns>
        public virtual bool Remove(TKey key)
        {
            var remove = ThisAsCollection().Where(pair => ObservableDictionary<TKey, TValue>.Equals(key, pair.Key)).ToList();
            foreach (var pair in remove)
            {
                ThisAsCollection().Remove(pair);
            }
            return remove.Count > 0;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <typeparamref name="TValue"/> parameter.</param>
        /// <returns><c>true</c> if the dictionary contains an element with the specified key; otherwise, <c>false</c>.</returns>
        public virtual bool TryGetValue(TKey key, out TValue value)
        {
#nullable disable
            value = default;
#nullable restore
            var r = GetKvpByTheKey(key);
            if (r == null)
            {
                return false;
            }
            value = r.Value;
            return true;
        }

        /// <summary>
        /// Gets the key-value pair associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>The key-value pair associated with the specified key, or <c>null</c> if the key is not found.</returns>
        private ObservableKeyValuePair<TKey, TValue> GetKvpByTheKey(TKey key)
        {
#nullable disable
            return ThisAsCollection().FirstOrDefault((i) => i.Key.Equals(key));
#nullable restore
        }

        /// <summary>
        /// Gets a collection containing the values in the dictionary.
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return (from i in ThisAsCollection() select i.Value).ToList(); }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (!TryGetValue(key, out result))
                {
                    throw new ArgumentException("Key not found");
                }
                return result;
            }
            set
            {
                if (ContainsKey(key))
                {
                    GetKvpByTheKey(key).Value = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        /// <summary>
        /// Adds a key-value pair to the dictionary.
        /// </summary>
        /// <param name="item">The key-value pair to add to the dictionary.</param>
        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific key-value pair.
        /// </summary>
        /// <param name="item">The key-value pair to locate in the dictionary.</param>
        /// <returns><c>true</c> if the dictionary contains the specified key-value pair; otherwise, <c>false</c>.</returns>
        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var r = GetKvpByTheKey(item.Key);
            if (Equals(r, default(ObservableKeyValuePair<TKey, TValue>)))
            {
                return false;
            }
            return Equals(r.Value, item.Value);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ObservableDictionary{TKey, TValue}"/> to an array of key-value pairs, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional array of key-value pairs that is the destination of the elements copied from the dictionary. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array), "The destination array is null.");
            }

            if (arrayIndex < 0 || arrayIndex >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The array index is out of range.");
            }

            if (array.Length - arrayIndex < Count)
            {
                throw new ArgumentException("The number of elements in the dictionary is greater than the available space from arrayIndex to the end of the destination array.");
            }

            int i = arrayIndex;
            foreach (var pair in ThisAsCollection())
            {
                array[i] = new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);
                i++;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableDictionary{TKey, TValue}"/> is read-only.
        /// </summary>
        /// <value>true if the dictionary is read-only; otherwise, false. This property always returns false.</value>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific key and value from the dictionary.
        /// </summary>
        /// <param name="item">The key-value pair to remove from the dictionary.</param>
        /// <returns>true if the key-value pair was successfully removed; otherwise, false.</returns>
        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var r = GetKvpByTheKey(item.Key);
            if (Equals(r, default(ObservableKeyValuePair<TKey, TValue>)))
            {
                return false;
            }
            if (!Equals(r.Value, item.Value))
            {
                return false;
            }
            return ThisAsCollection().Remove(r);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="ObservableDictionary{TKey, TValue}"/>.</returns>
        public virtual new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return (from i in ThisAsCollection() select new KeyValuePair<TKey, TValue>(i.Key, i.Value)).ToList().GetEnumerator();
        }
    }
}