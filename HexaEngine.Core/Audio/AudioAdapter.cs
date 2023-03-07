namespace HexaEngine.Core.Audio
{
    public static class AudioAdapter
    {
        public static readonly List<IAudioAdapter> Adapters = new();

        public static IAudioDevice CreateAudioDevice(AudioBackend backend, string? name)
        {
            if (backend == AudioBackend.Auto)
            {
                if (Adapters.Count == 1)
                    return Adapters[0].CreateAudioDevice(name);
                else
                {
                    IAudioAdapter audioAdapter = Adapters[0];
                    for (int i = 0; i < Adapters.Count; i++)
                    {
                        if (Adapters[i].PlatformScore > audioAdapter.PlatformScore)
                            audioAdapter = Adapters[i];
                    }
                    return audioAdapter.CreateAudioDevice(name);
                }
            }
            var adapter = Adapters.FirstOrDefault(x => x.Backend == backend) ?? throw new PlatformNotSupportedException();
            return adapter.CreateAudioDevice(name);
        }
    }
}