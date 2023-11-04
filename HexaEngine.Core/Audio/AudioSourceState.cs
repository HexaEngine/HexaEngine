namespace HexaEngine.Core.Audio
{
    /// <summary>
    /// Represents the state of an audio source.
    /// </summary>
    public enum AudioSourceState
    {
        /// <summary>
        /// The audio source is in its initial state.
        /// </summary>
        Initial,

        /// <summary>
        /// The audio source is currently playing.
        /// </summary>
        Playing,

        /// <summary>
        /// The audio source is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// The audio source has been stopped.
        /// </summary>
        Stopped
    }
}