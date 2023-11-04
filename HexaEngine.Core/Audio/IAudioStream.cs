namespace HexaEngine.Core.Audio
{
    using System;

    /// <summary>
    /// Represents an audio stream used for playing audio data.
    /// </summary>
    public interface IAudioStream : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the audio stream should loop.
        /// </summary>
        bool Looping { get; set; }

        /// <summary>
        /// Occurs when the end of the audio stream is reached.
        /// </summary>
        event Action? EndOfStream;

        /// <summary>
        /// Performs a full commit operation for the audio stream, associating it with a source.
        /// </summary>
        /// <param name="source">The source identifier to commit the stream to.</param>
        void FullCommit(uint source);

        /// <summary>
        /// Initializes the audio stream for playback on a specified source.
        /// </summary>
        /// <param name="source">The source identifier to initialize the stream on.</param>
        void Initialize(uint source);

        /// <summary>
        /// Resets the audio stream to its initial state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Updates the audio stream, typically called in a rendering loop to play audio data.
        /// </summary>
        /// <param name="source">The source identifier for which to update the stream.</param>
        void Update(uint source);
    }
}