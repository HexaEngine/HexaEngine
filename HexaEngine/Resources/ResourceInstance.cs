namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;

    public class ResourceInstance : IDisposable
    {
        private readonly IResourceFactory factory;
        private readonly string _name;
        private bool releasedValue;
        private volatile int instanceCount;

        public ResourceInstance(IResourceFactory factory, string name)
        {
            this.factory = factory;
            _name = name;
            instanceCount = 0;
        }

        public int InstanceCount => instanceCount;

        public bool IsUsed => instanceCount > 0;

        public string Name => _name;

        public IResourceFactory Factory => factory;

        public void AddRef()
        {
            Interlocked.Increment(ref instanceCount);
        }

        public void RemoveRef()
        {
            Interlocked.Decrement(ref instanceCount);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Release()
        {
            if (!releasedValue)
            {
                releasedValue = true;
                ReleaseResources();
            }
        }

        protected virtual void ReleaseResources()
        {
        }

        protected void Dispose(bool disposing)
        {
            factory.DestroyInstance(this);
        }
    }

    public interface IWaitResource
    {
        public void Wait();

        public Task WaitAsync();
    }

    public class ResourceInstance<T> : ResourceInstance, IWaitResource where T : class, IDisposable
    {
        private readonly Func<bool> waitDelegate;
        private T? value;

        public ResourceInstance(IResourceFactory factory, string name) : base(factory, name)
        {
            waitDelegate = WaitCondition;
        }

        ~ResourceInstance()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public nint Pointer => (value as IDeviceChild)?.NativePointer ?? 0;

        public T? Value => value;

        public virtual void BeginLoad()
        {
            value?.Dispose();
        }

        public virtual void EndLoad(T value)
        {
            this.value = value;
        }

        private bool WaitCondition()
        {
            return value != null;
        }

        public void Wait()
        {
            if (value != null)
            {
                return;
            }

            SpinWait.SpinUntil(waitDelegate);
        }

        public async Task WaitAsync()
        {
            await Task.Run(Wait);
        }

        protected override void ReleaseResources()
        {
            value?.Dispose();
        }
    }
}