namespace HexaEngine.OpenAL
{
    using HexaEngine.Core.Audio;

    public unsafe class OpenALAudioContext : IAudioContext
    {
        internal readonly OpenALAudioDevice AudioDevice;
        internal readonly Device* Device;
        public readonly Context* Context;
        private bool disposedValue;

        public nint NativePointer => (nint)Context;

        internal OpenALAudioContext(OpenALAudioDevice audioDevice, Context* context)
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}