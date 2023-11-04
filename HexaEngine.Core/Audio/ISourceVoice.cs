namespace HexaEngine.Core.Audio
{
    using System;

    /// <summary>
    /// Represents a source voice for playing audio with control over its properties and state.
    /// </summary>
    public interface ISourceVoice : IDisposable
    {
        /// <summary>
        /// Gets the audio stream buffer associated with the source voice.
        /// </summary>
        IAudioStream Buffer { get; }

        /// <summary>
        /// Gets or sets the emitter for controlling the audio source's spatial properties.
        /// </summary>
        IEmitter? Emitter { get; set; }

        /// <summary>
        /// Gets or sets the gain (volume) of the source voice.
        /// </summary>
        float Gain { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the source voice is looping.
        /// </summary>
        bool Looping { get; set; }

        /// <summary>
        /// Gets or sets the pitch of the audio source.
        /// </summary>
        float Pitch { get; set; }

        /// <summary>
        /// Gets the current state of the source voice.
        /// </summary>
        AudioSourceState State { get; }

        /// <summary>
        /// Gets or sets the submix voice to which this source voice is connected.
        /// </summary>
        ISubmixVoice? Submix { get; set; }

        /// <summary>
        /// Occurs when the source voice is paused.
        /// </summary>
        event Action? OnPause;

        /// <summary>
        /// Occurs when the source voice starts playing.
        /// </summary>
        event Action? OnPlay;

        /// <summary>
        /// Occurs when the source voice is rewound.
        /// </summary>
        event Action? OnRewind;

        /// <summary>
        /// Occurs when the state of the source voice changes.
        /// </summary>
        event Action<AudioSourceState>? OnStateChanged;

        /// <summary>
        /// Occurs when the source voice is stopped.
        /// </summary>
        event Action? OnStop;

        /// <summary>
        /// Pauses the source voice, if playing.
        /// </summary>
        void Pause();

        /// <summary>
        /// Starts or resumes playing the source voice.
        /// </summary>
        void Play();

        /// <summary>
        /// Rewinds the source voice to the beginning.
        /// </summary>
        void Rewind();

        /// <summary>
        /// Stops playing the source voice.
        /// </summary>
        void Stop();

        /// <summary>
        /// Updates the state and position of the source voice.
        /// </summary>
        void Update();
    }
}