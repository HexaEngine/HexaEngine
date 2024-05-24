namespace HexaEngine.Core.Audio
{
    /// <summary>
    /// Enumerates different audio backends that can be used for audio processing.
    /// </summary>
    public enum AudioBackend
    {
        /// <summary>
        /// Disables the audio sub system.
        /// </summary>
        Disabled = -1,

        /// <summary>
        /// Automatically select the most suitable audio backend based on the platform.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Use the OpenAL audio backend for audio processing.
        /// </summary>
        OpenAL = 1,

        /// <summary>
        /// Use the XAudio2 audio backend for audio processing.
        /// </summary>
        XAudio2 = 2,
    }
}