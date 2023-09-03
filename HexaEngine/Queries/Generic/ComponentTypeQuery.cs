namespace HexaEngine.Queries.Generic
{
    using HexaEngine.Core.Scenes;
    using HexaEngine.Scenes;
    using System.Collections;
    using System.Collections.Generic;

    public class ComponentTypeQuery<T> : IQuery, IReadOnlyList<T> where T : IComponent
    {
        private readonly List<T> cache = new();
        private bool disposedValue;

        public ComponentTypeQuery(QueryFlags flags = QueryFlags.Default)
        {
            Flags = flags;
        }

        public int Count => ((IReadOnlyCollection<T>)cache).Count;

        public QueryFlags Flags { get; }

        public T this[int index] => ((IReadOnlyList<T>)cache)[index];

        public event Action? OnQueryChanged;

        public event Action? OnQueryRefreshed;

        public event Action<GameObject, T>? OnAdded;

        public event Action<GameObject, T>? OnRemoved;

        public virtual void OnGameObjectAdded(GameObject gameObject)
        {
            AddObject(gameObject);
        }

        public virtual void OnGameObjectRemoved(GameObject gameObject)
        {
            RemoveObject(gameObject);
        }

        public virtual void OnGameObjectComponentAdded(GameObject gameObject, IComponent component)
        {
            if (component is T t)
            {
                cache.Add(t);
                OnAdded?.Invoke(gameObject, t);
                OnQueryChanged?.Invoke();
            }
        }

        public virtual void OnGameObjectComponentRemoved(GameObject gameObject, IComponent component)
        {
            if (component is T t && cache.Remove(t))
            {
                OnRemoved?.Invoke(gameObject, t);
                OnQueryChanged?.Invoke();
            }
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
            bool changed = false;
            foreach (var component in gameObject.GetComponents<T>())
            {
                var idx = cache.IndexOf(component);
                var contains = idx != -1;

                if (contains)
                {
                    continue;
                }

                cache.Add(component);
                OnAdded?.Invoke(gameObject, component);
                OnQueryChanged?.Invoke();
            }

            if (changed)
            {
                OnQueryChanged?.Invoke();
            }
        }

        protected void AddObject(GameObject gameObject)
        {
            bool changed = false;
            foreach (var component in gameObject.GetComponents<T>())
            {
                changed = true;
                cache.Add(component);
                OnAdded?.Invoke(gameObject, component);
            }

            if (changed)
            {
                OnQueryChanged?.Invoke();
            }
        }

        protected void RemoveObject(GameObject gameObject)
        {
            bool changed = false;
            foreach (var component in gameObject.GetComponents<T>())
            {
                if (cache.Remove(component))
                {
                    changed = true;
                    OnRemoved?.Invoke(gameObject, component);
                }
            }

            if (changed)
            {
                OnQueryChanged?.Invoke();
            }
        }

        public virtual void FlushQuery(IList<GameObject> objects)
        {
            cache.Clear();

            bool changed = false;
            for (int i = 0; i < objects.Count; i++)
            {
                var gameObject = objects[i];
                foreach (var component in gameObject.GetComponents<T>())
                {
                    changed = true;
                    cache.Add(component);
                    OnAdded?.Invoke(gameObject, component);
                }
            }

            if (changed)
            {
                OnQueryChanged?.Invoke();
            }
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

        ~ComponentTypeQuery()
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