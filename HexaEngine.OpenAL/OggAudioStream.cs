namespace HexaEngine.OpenAL
{
    public struct OggHeader
    {
    }

    public unsafe class OggAudioStream
    {
#pragma warning disable CS0169 // The field 'OggAudioStream.stream' is never used
#pragma warning disable CS8618 // Non-nullable field 'stream' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private readonly Stream stream;
#pragma warning restore CS8618 // Non-nullable field 'stream' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS0169 // The field 'OggAudioStream.stream' is never used
#pragma warning disable CS0169 // The field 'OggAudioStream.buffers' is never used
        private readonly uint* buffers;
#pragma warning restore CS0169 // The field 'OggAudioStream.buffers' is never used
#pragma warning disable CS0169 // The field 'OggAudioStream.bufferCount' is never used
        private readonly int bufferCount;
#pragma warning restore CS0169 // The field 'OggAudioStream.bufferCount' is never used
#pragma warning disable CS0169 // The field 'OggAudioStream.bufferSize' is never used
        private readonly int bufferSize;
#pragma warning restore CS0169 // The field 'OggAudioStream.bufferSize' is never used
#pragma warning disable CS0169 // The field 'OggAudioStream.buffer' is never used
#pragma warning disable CS8618 // Non-nullable field 'buffer' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private readonly byte[] buffer;
#pragma warning restore CS8618 // Non-nullable field 'buffer' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS0169 // The field 'OggAudioStream.buffer' is never used
        public readonly uint SampleOffset;
        public readonly uint ByteOffset;
        public readonly SourceType Type;
        public readonly BufferFormat Format;
#pragma warning disable CS0169 // The field 'OggAudioStream.position' is never used
        private int position;
#pragma warning restore CS0169 // The field 'OggAudioStream.position' is never used
#pragma warning disable CS0169 // The field 'OggAudioStream.looping' is never used
        private bool looping;
#pragma warning restore CS0169 // The field 'OggAudioStream.looping' is never used
#pragma warning disable CS0169 // The field 'OggAudioStream.reachedEnd' is never used
        private bool reachedEnd;
#pragma warning restore CS0169 // The field 'OggAudioStream.reachedEnd' is never used
    }
}