namespace HexaEngine.UI
{
    using HexaEngine.UI.Markup;
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;

    [Ambient]
    [UsableDuringInitialization(true)]
    public class ResourceDictionary : IDictionary, ISupportInitialize, INameScope, IUriContext
    {
        private static readonly Hashtable hashtable = [];
        private bool initialized = false;
        private ResourceDictionaryCollection? _mergedDictionaries;
        private Uri source;

        public object? this[object key]
        {
            get
            {
                var value = hashtable[key];
                OnGettingValue(key, ref value, out _);
                return value;
            }
            set => hashtable[key] = value;
        }

        public bool IsFixedSize => hashtable.IsFixedSize;

        public bool IsReadOnly => hashtable.IsReadOnly;

        public ICollection Keys => hashtable.Keys;

        public ICollection Values => hashtable.Values;

        public int Count => hashtable.Count;

        bool ICollection.IsSynchronized => hashtable.IsSynchronized;

        object ICollection.SyncRoot => hashtable.SyncRoot;

        Uri IUriContext.BaseUri { get; set; }

        public Uri Source
        {
            get => source;
            set => source = value;
        }

        public DeferrableContent DeferrableContent { get; set; }

        public bool InvalidatesImplicitDataTemplateResources { get; set; }

        public Collection<ResourceDictionary> MergedDictionaries
        {
            get
            {
                if (_mergedDictionaries == null)
                {
                    _mergedDictionaries = new ResourceDictionaryCollection(this);
                    _mergedDictionaries.CollectionChanged += OnMergedDictionariesChanged;
                }

                return _mergedDictionaries;
            }
        }

        private void OnMergedDictionariesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnGettingValue(object key, ref object? value, out bool canCache)
        {
            throw new NotImplementedException();
        }

        public void Add(object key, object? value)
        {
            hashtable.Add(key, value);
        }

        public void BeginInit()
        {
            if (initialized)
                throw new InvalidOperationException();
            throw new NotImplementedException();
        }

        public void EndInit()
        {
            initialized = true;
            throw new NotImplementedException();
        }

        public void Clear()
        {
            hashtable.Clear();
        }

        public bool Contains(object key)
        {
            return hashtable.Contains(key);
        }

        public void CopyTo(Array array, int index)
        {
            hashtable.CopyTo(array, index);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return hashtable.GetEnumerator();
        }

        public void Remove(object key)
        {
            hashtable.Remove(key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return hashtable.GetEnumerator();
        }

        public object? FindName(string name)
        {
            return null;
        }

        public void RegisterName(string name, object scopedElement)
        {
            throw new NotSupportedException();
        }

        public void UnregisterName(string name)
        {
        }

        internal void RemoveParentOwners(ResourceDictionary resourceDictionary)
        {
            throw new NotImplementedException();
        }
    }
}