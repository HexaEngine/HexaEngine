namespace HexaEngine.OpenAL
{
    using HexaEngine.Core.Audio;
    using Hexa.NET.OpenAL;

    public unsafe class OpenALAudioContext : IAudioContext
    {
        internal readonly OpenALAudioDevice AudioDevice;
        internal readonly ALCdevice* Device;
        public readonly ALCcontext* Context;
        private bool disposedValue;

        public nint NativePointer => (nint)Context;

        internal OpenALAudioContext(OpenALAudioDevice audioDevice, ALCcontext* context)
        {
            AudioDevice = audioDevice;
            Device = audioDevice.Device;
            Context = context;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                OpenAL.DestroyContext(Context);
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