namespace HexaEngine.Resources
{
    public interface IResourceFactory<T, TData> : IResourceFactory where T : ResourceInstance
    {
        public IReadOnlyDictionary<ResourceGuid, T> Instances { get; }

        new T? GetInstance(ResourceGuid id);

        T CreateInstance(ResourceGuid id, TData instanceData);

        Task<T> CreateInstanceAsync(ResourceGuid id, TData instanceData);

        T GetOrCreateInstance(ResourceGuid id, TData instanceData);

        Task<T> GetOrCreateInstanceAsync(ResourceGuid id, TData instanceData);

        bool DestroyInstance(T? instance);
    }

    public interface IResourceFactory : IDisposable
    {
        public ResourceManager ResourceManager { get; }

        public bool SuppressCleanup { get; set; }

        public bool IsType(Type type);

        public bool IsType(object? instance);

        public void Cleanup();

        public ResourceInstance? GetInstance(ResourceGuid id);

        public ResourceInstance CreateInstance(ResourceGuid id, object? instanceData);

        public Task<ResourceInstance> CreateInstanceAsync(ResourceGuid id, object? instanceData);

        public ResourceInstance GetOrCreateInstance(ResourceGuid id, object? instanceData);

        public Task<ResourceInstance> GetOrCreateInstanceAsync(ResourceGuid id, object? instanceData);

        public bool DestroyInstance(ResourceGuid id)
        {
            var instance = GetInstance(id);
            if (instance != null)
            {
                return DestroyInstance(instance);
            }
            return false;
        }

        public bool DestroyInstance(ResourceInstance? instance);

        public void Release();
    }
}