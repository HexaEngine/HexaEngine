namespace HexaEngine.Queries
{
    using HexaEngine.Collections;
    using Hexa.NET.Mathematics;
    using HexaEngine.Scenes;

    public class QuerySystem : ISceneSystem, IDisposable
    {
        private readonly Scene scene;
        private readonly FlaggedList<QueryFlags, IQuery> queries = new();
        private bool disposedValue;

        public QuerySystem(Scene scene)
        {
            this.scene = scene;
            scene.OnGameObjectAdded += OnGameObjectAdded;
            scene.OnGameObjectRemoved += OnGameObjectRemoved;
        }

        public static QuerySystem? Current => SceneManager.Current?.QueryManager;

        public string Name { get; } = "Query System";

        public SystemFlags Flags { get; } = SystemFlags.Destroy;

        private void OnGameObjectAdded(GameObject obj)
        {
            lock (queries)
            {
                obj.ComponentAdded += OnComponentAdded;
                obj.ComponentRemoved += OnComponentRemoved;
                obj.NameChanged += OnNameChanged;
                obj.ParentChanged += OnParentChanged;
                obj.TagChanged += OnTagChanged;
                obj.TransformUpdated += OnTransformed;
                obj.PropertyChanged += PropertyChanged;

                var queue = queries[QueryFlags.ObjectAdded];
                for (int i = 0; i < queue.Count; i++)
                {
                    queue[i].OnGameObjectAdded(obj);
                }
            }
        }

        private void PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not GameObject gameObject || e.PropertyName == null)
            {
                return;
            }

            lock (queries)
            {
                var queue = queries[QueryFlags.PropertyChanged];
                for (int i = 0; i < queue.Count; i++)
                {
                    queue[i].OnGameObjectPropertyChanged(gameObject, e.PropertyName);
                }
            }
        }

        private void OnTransformed(GameObject sender, Transform transform)
        {
            lock (queries)
            {
                var queue = queries[QueryFlags.Transformed];
                for (int i = 0; i < queue.Count; i++)
                {
                    queue[i].OnGameObjectTransformed(sender);
                }
            }
        }

        private void OnTagChanged(GameObject sender, object? tag)
        {
            lock (queries)
            {
                var queue = queries[QueryFlags.TagChanged];
                for (int i = 0; i < queue.Count; i++)
                {
                    queue[i].OnGameObjectTagChanged(sender, tag);
                }
            }
        }

        private void OnParentChanged(GameObject sender, GameObject? parent)
        {
            lock (queries)
            {
                var queue = queries[QueryFlags.ParentChanged];
                for (int i = 0; i < queue.Count; i++)
                {
                    queue[i].OnGameObjectParentChanged(sender, parent);
                }
            }
        }

        private void OnNameChanged(GameObject sender, string name)
        {
            lock (queries)
            {
                var queue = queries[QueryFlags.NameChanged];
                for (int i = 0; i < queue.Count; i++)
                {
                    queue[i].OnGameObjectNameChanged(sender, name);
                }
            }
        }

        private void OnComponentRemoved(GameObject sender, IComponent component)
        {
            lock (queries)
            {
                var queue = queries[QueryFlags.ComponentRemoved];
                for (int i = 0; i < queue.Count; i++)
                {
                    queue[i].OnGameObjectComponentRemoved(sender, component);
                }
            }
        }

        private void OnComponentAdded(GameObject sender, IComponent component)
        {
            lock (queries)
            {
                var queue = queries[QueryFlags.ComponentAdded];
                for (int i = 0; i < queue.Count; i++)
                {
                    queue[i].OnGameObjectComponentAdded(sender, component);
                }
            }
        }

        private void OnGameObjectRemoved(GameObject obj)
        {
            lock (queries)
            {
                obj.ComponentAdded -= OnComponentAdded;
                obj.ComponentRemoved -= OnComponentRemoved;
                obj.NameChanged -= OnNameChanged;
                obj.ParentChanged -= OnParentChanged;
                obj.TagChanged -= OnTagChanged;
                obj.TransformUpdated -= OnTransformed;
                obj.PropertyChanged -= PropertyChanged;

                var queue = queries[QueryFlags.ObjectRemoved];
                for (int i = 0; i < queue.Count; i++)
                {
                    queue[i].OnGameObjectRemoved(obj);
                }
            }
        }

        public void AddQuery(IQuery query)
        {
            lock (queries)
            {
                query.QuerySystem = this;
                queries.Add(query);
                query.FlushQuery(scene.GameObjects);
            }
        }

        public void RemoveQuery(IQuery query)
        {
            lock (queries)
            {
                queries.Remove(query);
                query.Dispose();
            }
        }

        public void Destroy()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < queries.Count; i++)
                {
                    queries[i].Dispose();
                }
                for (int i = 0; i < scene.GameObjects.Count; i++)
                {
                    var obj = scene.GameObjects[i];
                    obj.ComponentAdded -= OnComponentAdded;
                    obj.ComponentRemoved -= OnComponentRemoved;
                    obj.NameChanged -= OnNameChanged;
                    obj.ParentChanged -= OnParentChanged;
                    obj.TagChanged -= OnTagChanged;
                    obj.TransformUpdated -= OnTransformed;
                    obj.PropertyChanged -= PropertyChanged;
                }
                queries.Clear();
                scene.OnGameObjectAdded -= OnGameObjectAdded;
                scene.OnGameObjectRemoved -= OnGameObjectRemoved;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}