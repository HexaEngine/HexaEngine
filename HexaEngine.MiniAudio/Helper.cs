namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;
    using HexaEngine.Core.Audio;

    public static class Helper
    {
        public static MaFormat Convert(AudioFormat format)
        {
            return format switch
            {
                AudioFormat.U8 => MaFormat.U8,
                AudioFormat.S16 => MaFormat.S16,
                AudioFormat.S24 => MaFormat.S24,
                AudioFormat.S32 => MaFormat.S32,
                AudioFormat.F32 => MaFormat.F32,
                _ => throw new NotSupportedException()
            };
        }
    }
}