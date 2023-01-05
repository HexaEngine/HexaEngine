namespace HexaEngine.OpenAL
{
    public unsafe class AudioContext : IDisposable
    {
        internal readonly AudioDevice AudioDevice;
        internal readonly Device* Device;
        public readonly Context* Context;
        private bool disposedValue;

        internal AudioContext(AudioDevice audioDevice, Context* context)
        {
            AudioDevice = audioDevice;
            Device = audioDevice.Device;
            Context = context;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                alc.DestroyContext(Context);
                disposedValue = true;
            }
        }

        ~AudioContext()
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