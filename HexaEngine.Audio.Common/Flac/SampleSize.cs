namespace HexaEngine.Audio.Common.Flac
{
    public enum SampleSize : byte
    {
        GetFromStreamInfo = 0b000,
        Sample8Bit = 0b001,
        Sample12Bit = 0b010,
        Reserved = 0b011,
        Sample16Bit = 0b100,
        Sample20Bit = 0b101,
        Sample24Bit = 0b110,
        Sample32Bit = 0b111
    }
}