namespace HexaEngine.Queries
{
    using HexaEngine.Scenes;
    using System.Collections;

    public class ObjectQuery : IQuery, IReadOnlyList<GameObject>
    {
        private readonly Func<GameObject, bool> selector;
        private readonly List<GameObject> cache = new();
        private bool disposedValue;

        public ObjectQuery(Func<GameObject, bool> selector, QueryFlags flags = QueryFlags.Default)
        {
            this.selector = selector;
            Flags = flags;
        }

        public int Count => ((IReadOnlyCollection<GameObject>)cache).Count;

        public QueryFlags Flags { get; }

        public QuerySystem QuerySystem { get; set; }

        public GameObject this[int index] => ((IReadOnlyList<GameObject>)cache)[index];

        public event Action? OnQueryChanged;

        public event Action? OnQueryRefreshed;

        public event Action<GameObject>? OnAdded;

        public event Action<GameObject>? OnRemoved;

        public virtual void OnGameObjectAdded(GameObject gameObject)
        {
            if (selector(gameObject))
            {
                cache.Add(gameObject);
                OnAdded?.Invoke(gameObject);
                OnQueryChanged?.Invoke();
            }
        }

        public virtual void OnGameObjectRemoved(GameObject gameObject)
        {
            if (cache.Remove(gameObject))
            {
                OnRemoved?.Invoke(gameObject);
                OnQueryChanged?.Invoke();
            }
        }

        public virtual void OnGameObjectComponentAdded(GameObject gameObject, IComponent component)
        {
            ProcessGameObject(gameObject);
        }

        public virtual void OnGameObjectComponentRemoved(GameObject gameObject, IComponent component)
        {
            ProcessGameObject(gameObject);
        }

        public virtual void OnGameObjectPropertyChanged(GameObject gameObject, string propertyName)
        {
            ProcessGameObject(gameObject);
        }

        public virtual void OnGameObjectTransformed(GameObject gameObject)
        {
            ProcessGameObject(gameObject);
        }

        public virtual void OnGameObjectTagChanged(GameObject gameObject, object? tag)
        {
            ProcessGameObject(gameObject);
        }

        public virtual void OnGameObjectParentChanged(GameObject gameObject, GameObject? parent)
        {
            ProcessGameObject(gameObject);
        }

        public virtual void OnGameObjectNameChanged(GameObject gameObject, string name)
        {
            ProcessGameObject(gameObject);
        }

        protected void ProcessGameObject(GameObject gameObject)
        {
            var idx = cache.IndexOf(gameObject);
            var contains = idx != -1;
            var selected = selector(gameObject);

            if (contains && selected)
            {
                OnQueryChanged?.Invoke();
            }
            else if (selected)
            {
                cache.Add(gameObject);
                OnAdded?.Invoke(gameObject);
                OnQueryChanged?.Invoke();
            }
            else if (contains)
            {
                cache.Remove(gameObject);
                OnRemoved?.Invoke(gameObject);
                OnQueryChanged?.Invoke();
            }
        }

        public virtual void FlushQuery(IList<GameObject> objects)
        {
            cache.Clear();
            for (int i = 0; i < objects.Count; i++)
            {
                var obj = objects[i];
                if (selector(obj))
                {
                    cache.Add(obj);
                    OnAdded?.Invoke(obj);
                }
            }

            OnQueryChanged?.Invoke();
            OnQueryRefreshed?.Invoke();
        }

        public IEnumerator<GameObject> GetEnumerator()
        {
            return ((IEnumerable<GameObject>)cache).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)cache).GetEnumerator();
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                cache.Clear();
                DisposeCore();
                disposedValue = true;
                QuerySystem.RemoveQuery(this);
            }
        }

        protected virtual void DisposeCore()
        {
        }

        ~ObjectQuery()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}