namespace HexaEngine.Resources
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using Microsoft.Extensions.DependencyInjection;
    using System.Diagnostics.CodeAnalysis;

    public class ResourceManager : IDisposable
    {
        private readonly object _lock = new();
        private readonly string name;
        private readonly IGraphicsDevice graphicsDevice;
        private readonly IAudioDevice audioDevice;
        private readonly List<IResourceFactory> factories = new();
        private readonly IServiceProvider serviceProvider;
        private bool suppressCleanup = false;
        private bool disposedValue;

        public ResourceManager(string name, IGraphicsDevice graphicsDevice, IAudioDevice audioDevice, ServiceCollection descriptors)
        {
            this.name = name;
            this.graphicsDevice = graphicsDevice;
            this.audioDevice = audioDevice;
            descriptors.AddSingleton(this);
            descriptors.AddSingleton(graphicsDevice);
            descriptors.AddSingleton(audioDevice);

            serviceProvider = descriptors.BuildServiceProvider();

            var services = serviceProvider.GetServices<IResourceFactory>();
            factories.AddRange(services);
        }

        public object SyncObject => _lock;

        public string Name => name;

        public IGraphicsDevice GraphicsDevice => graphicsDevice;

        public IAudioDevice AudioDevice => audioDevice;

        public IServiceProvider ServiceProvider => serviceProvider;

        public static ResourceManager Shared { get; internal set; }

        public void BeginNoGCRegion()
        {
            lock (_lock)
            {
                if (suppressCleanup)
                {
                    return;
                }

                suppressCleanup = true;

                for (int i = 0; i < factories.Count; i++)
                {
                    factories[i].SuppressCleanup = true;
                }
            }
        }

        public void EndNoGCRegion()
        {
            lock (_lock)
            {
                if (!suppressCleanup)
                {
                    return;
                }

                suppressCleanup = false;

                for (int i = 0; i < factories.Count; i++)
                {
                    var factory = factories[i];
                    factory.SuppressCleanup = false;
                    factory.Cleanup();
                }
            }
        }

        public void AddFactory(IResourceFactory factory)
        {
            lock (_lock)
            {
                factories.Add(factory);
            }
        }

        public void RemoveFactory(IResourceFactory factory)
        {
            lock (_lock)
            {
                factories.Remove(factory);
            }
        }

        public IResourceFactory? GetFactoryByResourceType(Type type)
        {
            for (int i = 0; i < factories.Count; i++)
            {
                var factory = factories[i];
                if (factory.IsType(type))
                {
                    return factory;
                }
            }

            return null;
        }

        public bool TryGetFactoryByResourceType(Type type, [NotNullWhen(true)] out IResourceFactory? factory)
        {
            factory = GetFactoryByResourceType(type);
            return factory != null;
        }

        public IResourceFactory<T, TData>? GetFactoryByResourceType<T, TData>() where T : ResourceInstance
        {
            for (int i = 0; i < factories.Count; i++)
            {
                var factory = factories[i];
                if (factory is IResourceFactory<T, TData> t)
                {
                    return t;
                }
            }

            return null;
        }

        public bool TryGetFactoryByResourceType<T, TData>([NotNullWhen(true)] out IResourceFactory<T, TData>? factory) where T : ResourceInstance
        {
            factory = GetFactoryByResourceType<T, TData>();
            return factory != null;
        }

        public T? GetFactory<T>() where T : IResourceFactory
        {
            for (int i = 0; i < factories.Count; i++)
            {
                var factory = factories[i];
                if (factory is T t)
                {
                    return t;
                }
            }

            return default;
        }

        public bool TryGetFactory<T>([NotNullWhen(true)] out T? factory) where T : IResourceFactory
        {
            factory = GetFactory<T>();
            return factory != null;
        }

        public T? GetInstance<T>(ResourceGuid id) where T : ResourceInstance
        {
            for (int i = 0; i < factories.Count; i++)
            {
                var factory = factories[i];
                if (factory.GetInstance(id) is T instance)
                {
                    return instance;
                }
            }

            return null;
        }

        public bool TryGetInstance<T>(ResourceGuid id, [NotNullWhen(true)] out T? instance) where T : ResourceInstance
        {
            instance = GetInstance<T>(id);
            return instance != null;
        }

        public T? CreateInstance<T, TData>(ResourceGuid id, TData data) where T : ResourceInstance
        {
            for (int i = 0; i < factories.Count; i++)
            {
                var factory = factories[i];
                if (factory is IResourceFactory<T, TData> t)
                {
                    var instance = t.GetOrCreateInstance(id, data);
                    instance?.AddRef();
                    return instance;
                }
            }

            return null;
        }

        public bool TryCreateInstance<T, TData>(ResourceGuid id, TData data, [NotNullWhen(true)] out T? instance) where T : ResourceInstance
        {
            instance = CreateInstance<T, TData>(id, data);
            return instance != null;
        }

        public async Task<T?> CreateInstanceAsync<T, TData>(ResourceGuid id, TData data) where T : ResourceInstance
        {
            for (int i = 0; i < factories.Count; i++)
            {
                var factory = factories[i];
                if (factory is IResourceFactory<T, TData> t)
                {
                    var instance = await t.GetOrCreateInstanceAsync(id, data);
                    instance.AddRef();
                    return instance;
                }
            }

            return null;
        }

        public bool DestroyInstance(ResourceInstance? instance)
        {
            if (instance == null)
            {
                return false;
            }

            if (suppressCleanup)
            {
                return false;
            }

            for (int i = 0; i < factories.Count; i++)
            {
                var factory = factories[i];
                if (factory.DestroyInstance(instance))
                {
                    return true;
                }
            }

            return false;
        }

        public void Release()
        {
            for (int i = 0; i < factories.Count; i++)
            {
                factories[i].Release();
            }
        }

        public override string ToString()
        {
            return name;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < factories.Count; i++)
                {
                    factories[i].Dispose();
                }
                factories.Clear();

                disposedValue = true;
            }
        }

        ~ResourceManager()
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