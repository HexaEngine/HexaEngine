namespace HexaEngine.Queries.Generic
{
    using HexaEngine.Scenes;
    using System.Collections;

    public class ObjectQuery<T> : IQuery, IReadOnlyList<T> where T : GameObject
    {
        private readonly Func<T, bool> selector;
        private readonly List<T> cache = new();
        private bool disposedValue;

        public ObjectQuery(Func<T, bool> selector, QueryFlags flags = QueryFlags.Default)
        {
            this.selector = selector;
            Flags = flags;
        }

        public int Count => ((IReadOnlyCollection<T>)cache).Count;

        public QueryFlags Flags { get; }

        public T this[int index] => ((IReadOnlyList<T>)cache)[index];

        public event Action? OnQueryChanged;

        public event Action? OnQueryRefreshed;

        public event Action<T>? OnAdded;

        public event Action<T>? OnRemoved;

        public virtual void OnGameObjectAdded(GameObject gameObject)
        {
            if (gameObject is T t && selector(t))
            {
                cache.Add(t);
                OnAdded?.Invoke(t);
                OnQueryChanged?.Invoke();
            }
        }

        public virtual void OnGameObjectRemoved(GameObject gameObject)
        {
            if (gameObject is T t && cache.Remove(t))
            {
                OnRemoved?.Invoke(t);
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
            if (gameObject is not T t)
            {
                return;
            }
            var idx = cache.IndexOf(t);
            var contains = idx != -1;
            var selected = selector(t);

            if (contains && selected)
            {
                OnQueryChanged?.Invoke();
            }
            else if (selected)
            {
                cache.Add(t);
                OnAdded?.Invoke(t);
                OnQueryChanged?.Invoke();
            }
            else if (contains)
            {
                cache.Remove(t);
                OnRemoved?.Invoke(t);
                OnQueryChanged?.Invoke();
            }
        }

        public virtual void FlushQuery(IList<GameObject> objects)
        {
            cache.Clear();
            for (int i = 0; i < objects.Count; i++)
            {
                var obj = objects[i];
                if (obj is T t && selector(t))
                {
                    cache.Add(t);
                    OnAdded?.Invoke(t);
                }
            }

            OnQueryChanged?.Invoke();
            OnQueryRefreshed?.Invoke();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)cache).GetEnumerator();
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