namespace HexaEngine.Core.Audio
{
    using System.IO;

    public interface IAudioDevice : IDisposable
    {
        IAudioContext? Current { get; set; }

        IAudioContext Default { get; }

        IAudioContext CreateContext();

        IEmitter CreateEmitter();

        IListener CreateListener(IMasteringVoice voice);

        IMasteringVoice CreateMasteringVoice(string name);

        ISourceVoice CreateSourceVoice(IAudioStream audioStream);

        ISubmixVoice CreateSubmixVoice(string name, IMasteringVoice voice);

        IAudioStream CreateWaveAudioStream(Stream stream);

        void ProcessAudio();
    }
}