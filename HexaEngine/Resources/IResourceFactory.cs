namespace HexaEngine.Resources
{
    public interface IResourceFactory<T, TData> : IResourceFactory where T : ResourceInstance
    {
        public IReadOnlyDictionary<string, T> Instances { get; }

        new T? GetInstance(string name);

        T CreateInstance(string name, TData instanceData);

        Task<T> CreateInstanceAsync(string name, TData instanceData);

        T GetOrCreateInstance(string name, TData instanceData);

        Task<T> GetOrCreateInstanceAsync(string name, TData instanceData);

        bool DestroyInstance(T? instance);
    }

    public interface IResourceFactory : IDisposable
    {
        public ResourceManager ResourceManager { get; }

        public bool SuppressCleanup { get; set; }

        public bool IsType(Type type);

        public bool IsType(object? instance);

        public void Cleanup();

        public ResourceInstance? GetInstance(string name);

        public ResourceInstance CreateInstance(string name, object? instanceData);

        public Task<ResourceInstance> CreateInstanceAsync(string name, object? instanceData);

        public ResourceInstance GetOrCreateInstance(string name, object? instanceData);

        public Task<ResourceInstance> GetOrCreateInstanceAsync(string name, object? instanceData);

        public bool DestroyInstance(string name)
        {
            var instance = GetInstance(name);
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