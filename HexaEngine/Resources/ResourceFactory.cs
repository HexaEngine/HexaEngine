namespace HexaEngine.Resources
{
    using System.Collections.Concurrent;

    public abstract class ResourceFactory<T, TData> : IResourceFactory<T, TData>, IResourceFactory where T : ResourceInstance
    {
        protected readonly ConcurrentDictionary<string, T> instances = new();
        protected readonly object _lock = new();
        private readonly Type type = typeof(T);
        private bool disposedValue;

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

        ResourceInstance IResourceFactory.CreateInstance(ResourceManager1 manager, string name, object? instanceData)
        {
            if (instanceData is TData data)
            {
                return ((IResourceFactory<T, TData>)this).CreateInstance(manager, name, data);
            }

            throw new InvalidOperationException(nameof(instanceData));
        }

        T IResourceFactory<T, TData>.CreateInstance(ResourceManager1 manager, string name, TData instanceData)
        {
            lock (_lock)
            {
                var instance = CreateInstance(manager, name, instanceData);
                instances.TryAdd(name, instance);
                return instance;
            }
        }

        protected abstract T CreateInstance(ResourceManager1 manager, string name, TData instanceData);

        protected abstract void LoadInstance(ResourceManager1 manager, T instance, TData instanceData);

        ResourceInstance IResourceFactory.GetOrCreateInstance(ResourceManager1 manager, string name, object? instanceData)
        {
            if (instanceData is TData data)
            {
                return GetOrCreateInstance(manager, name, data);
            }

            throw new InvalidOperationException(nameof(instanceData));
        }

        public T GetOrCreateInstance(ResourceManager1 manager, string name, TData instanceData)
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
                    instance = CreateInstance(manager, name, instanceData);
                    instances.TryAdd(name, instance);
                }
            }

            LoadInstance(manager, instance, instanceData);

            return instance;
        }

        protected abstract Task LoadInstanceAsync(ResourceManager1 manager, T instance, TData instanceData);

        async Task<ResourceInstance> IResourceFactory.CreateInstanceAsync(ResourceManager1 manager, string name, object? instanceData)
        {
            if (instanceData is TData data)
            {
                await ((IResourceFactory<T, TData>)this).CreateInstanceAsync(manager, name, data);
            }

            throw new InvalidOperationException(nameof(instanceData));
        }

        Task<T> IResourceFactory<T, TData>.CreateInstanceAsync(ResourceManager1 manager, string name, TData instanceData)
        {
            var instance = CreateInstance(manager, name, instanceData);
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

        async Task<ResourceInstance> IResourceFactory.GetOrCreateInstanceAsync(ResourceManager1 manager, string name, object? instanceData)
        {
            if (instanceData is TData data)
            {
                await ((IResourceFactory<T, TData>)this).GetOrCreateInstanceAsync(manager, name, data);
            }

            throw new InvalidOperationException(nameof(instanceData));
        }

        async Task<T> IResourceFactory<T, TData>.GetOrCreateInstanceAsync(ResourceManager1 manager, string name, TData instanceData)
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
                    instance = CreateInstance(manager, name, instanceData);
                    var result = instances.TryAdd(name, instance);
                }
            }

            if (doWait && instance is IWaitResource wait)
            {
                await wait.WaitAsync();
                return instance;
            }

            await LoadInstanceAsync(manager, instance, instanceData);

            return instance;
        }

        public bool DestroyInstance(ResourceManager1 manager, ResourceInstance instance)
        {
            if (instance is T t)
            {
                lock (_lock)
                {
                    instances.Remove(t.Name, out _);
                    return DestroyInstance(manager, t);
                }
            }
            return false;
        }

        protected abstract void UnloadInstance(ResourceManager1 manager, T instance);

        public virtual bool DestroyInstance(ResourceManager1 manager, T instance)
        {
            instances.Remove(instance.Name, out _);
            UnloadInstance(manager, instance);
            instance.Dispose();
            return true;
        }

        public virtual void Cleanup(ResourceManager1 manager)
        {
            foreach (var instance in instances.ToArray())
            {
                if (!instance.Value.IsUsed)
                {
                    DestroyInstance(manager, instance.Value);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                foreach (var instance in instances.Values)
                {
                    instance.Dispose();
                }
                instances.Clear();
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