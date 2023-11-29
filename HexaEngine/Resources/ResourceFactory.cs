namespace HexaEngine.Resources
{
    using System.Collections.Concurrent;

    public abstract class ResourceFactory<T, TData> : IResourceFactory<T, TData>, IResourceFactory where T : ResourceInstance
    {
        protected readonly ResourceManager resourceManager;
        protected readonly ConcurrentDictionary<string, T> instances = new();
        protected readonly object _lock = new();
        private readonly Type type = typeof(T);

        private bool disposedValue;
        private bool suppressCleanup;

        protected ResourceFactory(ResourceManager resourceManager)
        {
            this.resourceManager = resourceManager;
        }

        public IReadOnlyDictionary<string, T> Instances => instances;

        public ResourceManager ResourceManager => resourceManager;

        public bool SuppressCleanup { get => suppressCleanup; set => suppressCleanup = value; }

        public bool IsType(Type type)
        {
            return type == this.type;
        }

        public bool IsType(object? instance)
        {
            return instance is T;
        }

        ResourceInstance? IResourceFactory.GetInstance(string name)
        {
            return GetInstance(name);
        }

        public virtual T? GetInstance(string name)
        {
            lock (_lock)
            {
                if (instances.TryGetValue(name, out var instance))
                {
                    return instance;
                }
            }

            return null;
        }

        ResourceInstance IResourceFactory.CreateInstance(string name, object? instanceData)
        {
            if (instanceData is TData data)
            {
                return ((IResourceFactory<T, TData>)this).CreateInstance(name, data);
            }

            throw new InvalidOperationException(nameof(instanceData));
        }

        T IResourceFactory<T, TData>.CreateInstance(string name, TData instanceData)
        {
            lock (_lock)
            {
                var instance = CreateInstance(resourceManager, name, instanceData);
                if (instance == null)
                    return instance;
                instances.TryAdd(name, instance);
                return instance;
            }
        }

        protected abstract T CreateInstance(ResourceManager manager, string name, TData instanceData);

        protected abstract void LoadInstance(ResourceManager manager, T instance, TData instanceData);

        ResourceInstance IResourceFactory.GetOrCreateInstance(string name, object? instanceData)
        {
            if (instanceData is TData data)
            {
                return GetOrCreateInstance(name, data);
            }

            throw new InvalidOperationException(nameof(instanceData));
        }

        public T GetOrCreateInstance(string name, TData instanceData)
        {
            T? instance;
            lock (_lock)
            {
                instance = GetInstance(name);
                if (instance != null)
                {
                    if (instance is IWaitResource wait)
                    {
                        wait.Wait();
                    }

                    return instance;
                }
                else
                {
                    instance = CreateInstance(resourceManager, name, instanceData);
                    if (instance == null)
                        return instance;
                    instances.TryAdd(name, instance);
                }
            }

            LoadInstance(resourceManager, instance, instanceData);

            return instance;
        }

        protected abstract Task LoadInstanceAsync(ResourceManager manager, T instance, TData instanceData);

        async Task<ResourceInstance> IResourceFactory.CreateInstanceAsync(string name, object? instanceData)
        {
            if (instanceData is TData data)
            {
                await ((IResourceFactory<T, TData>)this).CreateInstanceAsync(name, data);
            }

            throw new InvalidOperationException(nameof(instanceData));
        }

        Task<T> IResourceFactory<T, TData>.CreateInstanceAsync(string name, TData instanceData)
        {
            var instance = CreateInstance(resourceManager, name, instanceData);
            if (instance == null)
                return Task.FromResult(instance ?? throw new InvalidDataException());

            var result = instances.TryAdd(name, instance);
            if (result)
            {
                return Task.FromResult(instance);
            }
            else
            {
                instances.TryGetValue(name, out instance);
                return Task.FromResult(instance ?? throw new InvalidDataException());
            }
        }

        async Task<ResourceInstance> IResourceFactory.GetOrCreateInstanceAsync(string name, object? instanceData)
        {
            if (instanceData is TData data)
            {
                await ((IResourceFactory<T, TData>)this).GetOrCreateInstanceAsync(name, data);
            }

            throw new InvalidOperationException(nameof(instanceData));
        }

        async Task<T> IResourceFactory<T, TData>.GetOrCreateInstanceAsync(string name, TData instanceData)
        {
            T? instance;
            bool doWait = false;
            lock (_lock)
            {
                instance = GetInstance(name);
                if (instance != null)
                {
                    if (instance is IWaitResource)
                    {
                        doWait = true;
                    }
                    else
                    {
                        return instance;
                    }
                }
                else
                {
                    instance = CreateInstance(resourceManager, name, instanceData);
                    if (instance == null)
                        return instance;
                    var result = instances.TryAdd(name, instance);
                }
            }

            if (doWait && instance is IWaitResource wait)
            {
                await wait.WaitAsync();
                return instance;
            }

            await LoadInstanceAsync(resourceManager, instance, instanceData);

            return instance;
        }

        public bool DestroyInstance(ResourceInstance? instance)
        {
            if (instance is T t)
            {
                return DestroyInstance(t);
            }
            return false;
        }

        protected abstract void UnloadInstance(ResourceManager manager, T instance);

        public virtual bool DestroyInstance(T? instance)
        {
            if (instance == null)
            {
                return false;
            }

            instance.RemoveRef();
            if (instance.IsUsed)
            {
                return false;
            }

            if (suppressCleanup)
            {
                return false;
            }

            lock (_lock)
            {
                instances.Remove(instance.Name, out _);
                UnloadInstance(resourceManager, instance);
                instance.Release();
            }

            return true;
        }

        public virtual void Cleanup()
        {
            lock (_lock)
            {
                foreach (var instance in instances.ToArray())
                {
                    if (!instance.Value.IsUsed)
                    {
                        DestroyInstance(instance.Value);
                    }
                }
            }
        }

        public void Release()
        {
            foreach (var instance in instances.Values.ToArray())
            {
                instances.Remove(instance.Name, out _);
                UnloadInstance(resourceManager, instance);
                instance.Release();
            }
            instances.Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        ~ResourceFactory()
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