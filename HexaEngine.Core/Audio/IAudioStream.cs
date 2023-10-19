namespace HexaEngine.Core.Audio
{
    using System;

    public interface IAudioStream : IDisposable
    {
        bool Looping { get; set; }

        event Action? EndOfStream;

        void FullCommit(uint source);

        void Initialize(uint source);

        void Reset();

        void Update(uint source);
    }
}