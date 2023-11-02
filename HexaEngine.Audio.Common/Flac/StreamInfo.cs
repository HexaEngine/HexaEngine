namespace HexaEngine.Audio.Common.Flac
{
    public struct StreamInfo
    {
        /// <summary>
        /// The minimum block size (in samples) used in the stream.
        /// </summary>
        public ushort MinimumBlockSize;

        /// <summary>
        /// The maximum block size (in samples) used in the stream. (Minimum blocksize == maximum blocksize) implies a fixed-blocksize stream.
        /// </summary>
        public ushort MaximumBlockSize;

        /// <summary>
        /// The minimum frame size (in bytes) used in the stream. May be 0 to imply the value is not known.
        /// </summary>
        public UInt24 MinimumFrameSize;

        /// <summary>
        /// The maximum frame size (in bytes) used in the stream. May be 0 to imply the value is not known.
        /// </summary>
        public UInt24 MaximumFrameSize;

        /// <summary>
        /// Sample rate in Hz. Though 20 bits are available, the maximum sample rate is limited by the structure of frame headers to 655350Hz. Also, a value of 0 is invalid.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// Number of channels. FLAC supports from 1 to 8 channels
        /// </summary>
        public byte NumberOfChannels;

        /// <summary>
        /// Bits per sample. FLAC supports from 4 to 32 bits per sample.
        /// </summary>
        public ushort BitsPerSample;

        /// <summary>
        /// Total samples in stream. 'Samples' means inter-channel sample, i.e. one second of 44.1Khz audio will have 44100 samples regardless of the number of channels. A value of zero here means the number of total samples is unknown.
        /// </summary>
        public ulong TotalSamples;

        /// <summary>
        /// MD5 signature of the unencoded audio data. This allows the decoder to determine if an error exists in the audio data even when the error does not result in an invalid bitstream.
        /// </summary>
        public UInt128 MD5;

        public void Read(BitReader br)
        {
            MinimumBlockSize = br.ReadUInt16BigEndian();
            MaximumBlockSize = br.ReadUInt16BigEndian();
            MinimumFrameSize = br.ReadUInt24BigEndian();
            MaximumFrameSize = br.ReadUInt24BigEndian();
            SampleRate = br.ReadRawUInt32(20);
            NumberOfChannels = (byte)(br.ReadRawUInt8(3) + 1);
            BitsPerSample = (ushort)(br.ReadRawUInt8(5) + 1);
            TotalSamples = br.ReadRawUInt64(36);
            MD5 = br.ReadUInt128BigEndian();
        }

        public readonly void Write(BitWriter br)
        {
            br.WriteUInt16BigEndian(MinimumBlockSize);
            br.WriteUInt16BigEndian(MaximumBlockSize);
            br.WriteUInt24BigEndian(MinimumFrameSize);
            br.WriteUInt24BigEndian(MaximumFrameSize);
            br.WriteRawUInt32(SampleRate, 20);
            br.WriteRawUInt8((byte)(NumberOfChannels - 1), 3);
            br.WriteRawUInt8((byte)(BitsPerSample - 1), 5);
            br.WriteRawUInt64(TotalSamples, 36);
            br.WriteUInt128BigEndian(MD5);
        }
    }
}