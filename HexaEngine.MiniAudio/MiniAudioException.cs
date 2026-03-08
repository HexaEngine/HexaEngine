namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;

    public class MiniAudioException : Exception
    {
        public MiniAudioException(MaResult result) : base($"MiniAudio error: {result}")
        {
            Result = result;
        }

        public MaResult Result { get; }
    }
}
