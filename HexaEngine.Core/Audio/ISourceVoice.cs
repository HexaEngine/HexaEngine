namespace HexaEngine.Core.Audio
{
    using System;

    public interface ISourceVoice : IDisposable
    {
        IAudioStream Buffer { get; }

        IEmitter? Emitter { get; set; }

        float Gain { get; set; }

        bool Looping { get; set; }

        float Pitch { get; set; }

        AudioSourceState State { get; }

        ISubmixVoice? Submix { get; set; }

        event Action? OnPause;

        event Action? OnPlay;

        event Action? OnRewind;

        event Action<AudioSourceState>? OnStateChanged;

        event Action? OnStop;

        void Pause();

        void Play();

        void Rewind();

        void Stop();

        void Update();
    }
}