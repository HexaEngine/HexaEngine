namespace HexaEngine.OpenAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public struct OggHeader
    {
    }

    public unsafe class OggAudioStream
    {
        private readonly Stream stream;
        private readonly uint* buffers;
        private readonly int bufferCount;
        private readonly int bufferSize;
        private readonly byte[] buffer;
        public readonly uint SampleOffset;
        public readonly uint ByteOffset;
        public readonly SourceType Type;
        public readonly BufferFormat Format;
        private int position;
        private bool looping;
        private bool reachedEnd;
    }
}