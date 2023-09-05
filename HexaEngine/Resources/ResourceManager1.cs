namespace HexaEngine.Resources
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using System.Diagnostics.CodeAnalysis;

    public class ResourceManager1
    {
        private readonly IGraphicsDevice graphicsDevice;
        private readonly IAudioDevice audioDevice;
        private readonly List<IResourceFactory> factories = new();
        private bool suppressCleanup = false;

        public ResourceManager1(IGraphicsDevice graphicsDevice, IAudioDevice audioDevice)
        {
            this.graphicsDevice = graphicsDevice;
            this.audioDevice = audioDevice;
        }

        public IGraphicsDevice GraphicsDevice => graphicsDevice;

        public IAudioDevice AudioDevice => audioDevice;

        public void BeginNoGCRegion()
        {
            if (suppressCleanup)
            {
                return;
            }

            suppressCleanup = true;
        }

        public void EndNoGCRegion()
        {
            if (!suppressCleanup)
            {
                return;
            }

            suppressCleanup = false;

            for (int i = 0; i < factories.Count; i++)
            {
                factories[i].Cleanup(this);
            }
        }

        public void AddFactory(IResourceFactory factory)
        {
            factories.Add(factory);
        }

        public void RemoveFactory(IResourceFactory factory)
        {
            factories.Remove(factory);
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

        public T? GetInstance<T>(string name) where T : ResourceInstance
        {
            for (int i = 0; i < factories.Count; i++)
            {
                var factory = factories[i];
                if (factory.GetInstance(name) is T instance)
                {
                    return instance;
                }
            }

            return null;
        }

        public bool TryGetInstance<T>(string name, [NotNullWhen(true)] out T? instance) where T : ResourceInstance
        {
            instance = GetInstance<T>(name);
            return instance != null;
        }

        public T? CreateInstance<T, TData>(string name, TData data) where T : ResourceInstance
        {
            for (int i = 0; i < factories.Count; i++)
            {
                var factory = factories[i];
                if (factory is IResourceFactory<T, TData> t)
                {
                    var instance = t.GetOrCreateInstance(this, name, data);
                    instance.AddRef();
                    return instance;
                }
            }

            return null;
        }

        public bool TryCreateInstance<T, TData>(string name, TData data, [NotNullWhen(true)] out T? instance) where T : ResourceInstance
        {
            instance = CreateInstance<T, TData>(name, data);
            return instance != null;
        }

        public async Task<T?> CreateInstanceAsync<T, TData>(string name, TData data) where T : ResourceInstance
        {
            for (int i = 0; i < factories.Count; i++)
            {
                var factory = factories[i];
                if (factory is IResourceFactory<T, TData> t)
                {
                    var instance = await t.GetOrCreateInstanceAsync(this, name, data);
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

            instance.RemoveRef();
            if (instance.IsUsed)
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
                if (factory.DestroyInstance(this, instance))
                {
                    return true;
                }
            }

            return false;
        }
    }
}