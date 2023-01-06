namespace HexaEngine.OpenAL
{
    using System;

    public abstract class AudioStream
    {
        public abstract bool Looping { get; set; }

        public abstract event Action? EndOfStream;

        public abstract void FullCommit(uint source);

        public abstract void Initialize(uint source);

        public abstract void Reset();

        public abstract void Update(uint source);
    }
}