namespace HexaEngine.Core.Audio
{
    using System.Collections.Generic;

    public interface IAudioAdapter
    {
        IAudioDevice CreateAudioDevice(string? name);

        List<string> GetAvailableDevices();

        AudioBackend Backend { get; }

        int PlatformScore { get; }
    }
}