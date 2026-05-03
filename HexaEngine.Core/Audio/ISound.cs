namespace HexaEngine.Core.Audio
{
    using System;

    public enum PanMode
    {
        Balance,
        Pan
    }

    /// <summary>
    /// Represents a source voice for playing audio with control over its properties and state.
    /// </summary>
    public interface ISound : IDisposable
    {
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
        /// Gets or sets the pan of the audio source, controlling the left-right balance.
        /// </summary>
        float Pan { get; set; }

        /// <summary>
        /// Gets or sets the current pan mode, which determines how the view is adjusted during panning operations.
        /// </summary>
        /// <remarks>The pan mode can affect user interactions and the behavior of the view when
        /// navigating through content. Ensure to set the pan mode appropriately to achieve the desired panning
        /// effect.</remarks>
        PanMode PanMode { get; set; }

        /// <summary>
        /// Gets the current state of the source voice.
        /// </summary>
        AudioSourceState State { get; }

        /// <summary>
        /// Gets or sets the submix voice to which this source voice is connected.
        /// </summary>
        IAudioInputNode? Submix { get; set; }

        /// <summary>
        /// Occurs when the state of the source voice changes.
        /// </summary>
        event EventHandler<AudioSourceState>? StateChanged;

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
    }
}