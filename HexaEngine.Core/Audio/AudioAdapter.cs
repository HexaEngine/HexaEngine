namespace HexaEngine.Core.Audio
{
    /// <summary>
    /// Provides a set of audio adapters to create audio devices for various audio backends.
    /// </summary>
    public static class AudioAdapter
    {
        /// <summary>
        /// Gets a list of available audio adapters for creating audio devices.
        /// </summary>
        public static readonly List<IAudioAdapter> Adapters = new();

        /// <summary>
        /// Creates an audio device for the specified audio backend.
        /// </summary>
        /// <param name="backend">The audio backend to use for audio processing.</param>
        /// <param name="name">An optional name for the audio device.</param>
        /// <returns>An audio device created using the specified backend.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if the specified backend is not supported.</exception>
        public static IAudioDevice CreateAudioDevice(AudioBackend backend, string? name)
        {
            if (backend == AudioBackend.Auto)
            {
                if (Adapters.Count == 1)
                {
                    return Adapters[0].CreateAudioDevice(name);
                }
                else
                {
                    IAudioAdapter audioAdapter = Adapters[0];
                    for (int i = 0; i < Adapters.Count; i++)
                    {
                        if (Adapters[i].PlatformScore > audioAdapter.PlatformScore)
                        {
                            audioAdapter = Adapters[i];
                        }
                    }
                    return audioAdapter.CreateAudioDevice(name);
                }
            }
            var adapter = Adapters.FirstOrDefault(x => x.Backend == backend) ?? throw new PlatformNotSupportedException();
            return adapter.CreateAudioDevice(name);
        }
    }
}