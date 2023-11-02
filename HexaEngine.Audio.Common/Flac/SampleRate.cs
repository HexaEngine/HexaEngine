namespace HexaEngine.Audio.Common.Flac
{
    public enum SampleRate : byte
    {
        StreamInfo = 0b0000,
        Hz88200 = 0b0001,
        Hz176400 = 0b0010,
        Hz192000 = 0b0011,
        Hz8000 = 0b0100,
        Hz16000 = 0b0101,
        Hz22050 = 0b0110,
        Hz24000 = 0b0111,
        Hz32000 = 0b1000,
        Hz44100 = 0b1001,
        Hz48000 = 0b1010,
        Hz96000 = 0b1011,
        Bits8KHz = 0b1100,
        Bits16Hz = 0b1101,
        Bits16TensHz = 0b1110,
        Invalid = 0b1111
    }
}