namespace HexaEngine.Resources
{
    public interface IResourceFactory<T, TData> : IResourceFactory where T : ResourceInstance
    {
        public IReadOnlyDictionary<Guid, T> Instances { get; }

        new T? GetInstance(Guid id);

        T CreateInstance(Guid id, TData instanceData);

        Task<T> CreateInstanceAsync(Guid id, TData instanceData);

        T GetOrCreateInstance(Guid id, TData instanceData);

        Task<T> GetOrCreateInstanceAsync(Guid id, TData instanceData);

        bool DestroyInstance(T? instance);
    }

    public interface IResourceFactory : IDisposable
    {
        public ResourceManager ResourceManager { get; }

        public bool SuppressCleanup { get; set; }

        public bool IsType(Type type);

        public bool IsType(object? instance);

        public void Cleanup();

        public ResourceInstance? GetInstance(Guid id);

        public ResourceInstance CreateInstance(Guid id, object? instanceData);

        public Task<ResourceInstance> CreateInstanceAsync(Guid id, object? instanceData);

        public ResourceInstance GetOrCreateInstance(Guid id, object? instanceData);

        public Task<ResourceInstance> GetOrCreateInstanceAsync(Guid id, object? instanceData);

        public bool DestroyInstance(Guid id)
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