namespace HexaEngine.Core.Audio
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents an audio adapter that provides audio device creation and information retrieval.
    /// </summary>
    public interface IAudioAdapter
    {
        /// <summary>
        /// Creates an audio device using the adapter, with an optional name.
        /// </summary>
        /// <param name="name">An optional name for the audio device.</param>
        /// <returns>An instance of the created audio device.</returns>
        IAudioDevice CreateAudioDevice(string? name);

        /// <summary>
        /// Retrieves a list of available audio devices provided by this adapter.
        /// </summary>
        /// <returns>A list of available audio device names.</returns>
        List<string> GetAvailableDevices();

        /// <summary>
        /// Gets the backend used by the audio adapter.
        /// </summary>
        AudioBackend Backend { get; }

        /// <summary>
        /// Gets the platform score for the audio adapter, indicating its compatibility and suitability for the current platform.
        /// </summary>
        int PlatformScore { get; }
    }
}