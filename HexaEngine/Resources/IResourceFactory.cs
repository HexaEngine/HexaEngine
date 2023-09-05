namespace HexaEngine.Resources
{
    public interface IResourceFactory<T, TData> : IResourceFactory where T : ResourceInstance
    {
        new T? GetInstance(string name);

        T CreateInstance(ResourceManager1 manager, string name, TData instanceData);

        Task<T> CreateInstanceAsync(ResourceManager1 manager, string name, TData instanceData);

        T GetOrCreateInstance(ResourceManager1 manager, string name, TData instanceData);

        Task<T> GetOrCreateInstanceAsync(ResourceManager1 manager, string name, TData instanceData);

        bool DestroyInstance(ResourceManager1 manager, T instance);
    }

    public interface IResourceFactory : IDisposable
    {
        public bool IsType(Type type);

        public bool IsType(object? instance);

        public void Cleanup(ResourceManager1 manager);

        public ResourceInstance? GetInstance(string name);

        public ResourceInstance CreateInstance(ResourceManager1 manager, string name, object? instanceData);

        public Task<ResourceInstance> CreateInstanceAsync(ResourceManager1 manager, string name, object? instanceData);

        public ResourceInstance GetOrCreateInstance(ResourceManager1 manager, string name, object? instanceData);

        public Task<ResourceInstance> GetOrCreateInstanceAsync(ResourceManager1 manager, string name, object? instanceData);

        public bool DestroyInstance(ResourceManager1 manager, string name)
        {
            var instance = GetInstance(name);
            if (instance != null)
            {
                return DestroyInstance(manager, instance);
            }
            return false;
        }

        public bool DestroyInstance(ResourceManager1 manager, ResourceInstance instance);
    }
}