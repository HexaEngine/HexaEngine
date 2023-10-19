namespace HexaEngine.OpenAL
{
    using HexaEngine.Core.Audio;
    using System;

    public abstract class OpenALAudioStream : IAudioStream
    {
        private bool disposedValue;

        public abstract bool Looping { get; set; }

        public abstract event Action? EndOfStream;

        public abstract void FullCommit(uint source);

        public abstract void Initialize(uint source);

        public abstract void Reset();

        public abstract void Update(uint source);

        protected abstract void DisposeCore();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposeCore();
                disposedValue = true;
            }
        }

        ~OpenALAudioStream()
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