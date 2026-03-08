namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;

    public static class MiniAudioExtensions
    {
        public static void CheckError(this MaResult result)
        {
            if (result != MaResult.Success)
            {
                throw new MiniAudioException(result);
            }
        }
    }
}
