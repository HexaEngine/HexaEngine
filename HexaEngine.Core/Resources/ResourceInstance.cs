namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Graphics;

    public class ResourceInstance
    {
        public string _name;
        private bool disposedValue;
        private volatile int instanceCount;

        public ResourceInstance(string name, int instances)
        {
            _name = name;
            instanceCount = instances;
        }

        public int InstanceCount => instanceCount;

        public bool IsUsed => instanceCount > 0;

        public string Name => _name;

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }
    }

    public class ResourceInstance<T> : ResourceInstance where T : class, IDisposable
    {
        private readonly Func<bool> waitDelegate;
        private T? value;

        public ResourceInstance(string name, int instances) : base(name, instances)
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
            if (value != null) return;
            SpinWait.SpinUntil(waitDelegate);
        }

        protected override void Dispose(bool disposing)
        {
            value?.Dispose();
            base.Dispose(disposing);
        }
    }
}