namespace HexaEngine.Queries
{
    using HexaEngine.Scenes;
    using System.Collections;
    using System.Collections.Generic;

    public class ComponentQuery : IQuery, IReadOnlyList<IComponent>
    {
        private readonly Func<IComponent, bool> selector;
        private readonly List<IComponent> cache = new();
        private bool disposedValue;

        public ComponentQuery(Func<IComponent, bool> selector, QueryFlags flags = QueryFlags.Default)
        {
            this.selector = selector;
            Flags = flags;
        }

        public int Count => ((IReadOnlyCollection<IComponent>)cache).Count;

        public QueryFlags Flags { get; }

        public QuerySystem QuerySystem { get; set; }

        public IComponent this[int index] => ((IReadOnlyList<IComponent>)cache)[index];

        public event Action? OnQueryChanged;

        public event Action? OnQueryRefreshed;

        public event Action<IComponent>? OnAdded;

        public event Action<IComponent>? OnRemoved;

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
            if (selector(component))
            {
                cache.Add(component);
                OnAdded?.Invoke(component);
                OnQueryChanged?.Invoke();
            }
        }

        public virtual void OnGameObjectComponentRemoved(GameObject gameObject, IComponent component)
        {
            if (cache.Remove(component))
            {
                OnRemoved?.Invoke(component);
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
            foreach (var component in gameObject.GetComponents(selector))
            {
                var idx = cache.IndexOf(component);
                var contains = idx != -1;
                var selected = selector(component);

                if (contains && selected)
                {
                    OnQueryChanged?.Invoke();
                }
                else if (selected)
                {
                    cache.Add(component);
                    OnAdded?.Invoke(component);
                    OnQueryChanged?.Invoke();
                }
                else if (contains)
                {
                    cache.Remove(component);
                    OnRemoved?.Invoke(component);
                }
            }

            if (changed)
            {
                OnQueryChanged?.Invoke();
            }
        }

        protected void ProcessComponent(IComponent component)
        {
            var idx = cache.IndexOf(component);
            var contains = idx != -1;
            var selected = selector(component);

            if (contains && selected)
            {
                OnQueryChanged?.Invoke();
            }
            else if (selected)
            {
                cache.Add(component);
                OnAdded?.Invoke(component);
                OnQueryChanged?.Invoke();
            }
            else if (contains)
            {
                cache.Remove(component);
                OnRemoved?.Invoke(component);
            }
        }

        protected void AddObject(GameObject gameObject)
        {
            bool changed = false;
            foreach (var component in gameObject.GetComponents(selector))
            {
                changed = true;
                cache.Add(component);
                OnAdded?.Invoke(component);
            }

            if (changed)
            {
                OnQueryChanged?.Invoke();
            }
        }

        protected void RemoveObject(GameObject gameObject)
        {
            bool changed = false;
            foreach (var component in gameObject.GetComponents(selector))
            {
                if (cache.Remove(component))
                {
                    changed = true;
                    OnRemoved?.Invoke(component);
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
                foreach (var component in gameObject.GetComponents(selector))
                {
                    changed = true;
                    cache.Add(component);
                    OnAdded?.Invoke(component);
                }
            }

            if (changed)
            {
                OnQueryChanged?.Invoke();
            }
            OnQueryRefreshed?.Invoke();
        }

        public IEnumerator<IComponent> GetEnumerator()
        {
            return ((IEnumerable<IComponent>)cache).GetEnumerator();
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}