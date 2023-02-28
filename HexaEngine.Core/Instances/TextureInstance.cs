namespace HexaEngine.Core.Instances
{
    using HexaEngine.Core.Graphics;

    public unsafe class TextureInstance : IDisposable
    {
        public string _name;
        private IShaderResourceView? srv;
        private int instanceCount;
        private bool disposedValue;

        public TextureInstance(string name, int instances)
        {
            _name = name;
            Volatile.Write(ref instanceCount, instances);
        }

        public string Name => _name;

        public int InstanceCount => Volatile.Read(ref instanceCount);

        public bool IsUsed => Volatile.Read(ref instanceCount) > 0;
#nullable disable
        public void* Pointer => (void*)(srv?.NativePointer ?? null);
#nullable enable

        public void AddRef()
        {
            Volatile.Write(ref instanceCount, Volatile.Read(ref instanceCount) + 1);
        }

        public void RemoveRef()
        {
            Volatile.Write(ref instanceCount, Volatile.Read(ref instanceCount) - 1);
        }

        public void BeginLoad()
        {
            srv?.Dispose();
        }

        public void EndLoad(IShaderResourceView srv)
        {
            this.srv = srv;
        }

        public void Wait()
        {
            if (srv != null) return;
            SpinWait.SpinUntil(() => srv != null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                srv?.Dispose();
                disposedValue = true;
            }
        }

        ~TextureInstance()
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