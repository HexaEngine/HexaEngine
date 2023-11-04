namespace HexaEngine.Core.Audio
{
    /// <summary>
    /// Enumerates different audio backends that can be used for audio processing.
    /// </summary>
    public enum AudioBackend
    {
        /// <summary>
        /// Automatically select the most suitable audio backend based on the platform.
        /// </summary>
        Auto,

        /// <summary>
        /// Use the OpenAL audio backend for audio processing.
        /// </summary>
        OpenAL,

        /// <summary>
        /// Use the XAudio2 audio backend for audio processing.
        /// </summary>
        XAudio2,
    }
}