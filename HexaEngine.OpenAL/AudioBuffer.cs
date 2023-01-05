namespace HexaEngine.OpenAL
{
    using HexaEngine.IO;

    public unsafe class AudioStream
    {
        private readonly Stream stream;
        private readonly uint* buffers;
        private readonly int bufferCount;
        private readonly int bufferSize;
        private readonly byte[] buffer;
        public readonly uint SampleOffset;
        public readonly uint ByteOffset;
        public readonly SourceType Type;
        public readonly WaveHeader Header;
        public readonly BufferFormat Format;
        private int position;
        private bool looping;
        private bool reachedEnd;

        public AudioStream(Stream stream, int bufferCount = 4, int bufferSize = 65536)
        {
            Type = SourceType.Streaming;
            Header = new(stream);
            Format = Header.GetBufferFormat();
            this.stream = stream;
            this.bufferCount = bufferCount;
            this.bufferSize = bufferSize;
            buffer = new byte[bufferSize];
            buffers = Alloc<uint>(bufferCount);
            al.GenBuffers(bufferCount, buffers);
        }

        public int Position => position;

        public bool ReachedEnd => reachedEnd;

        public bool Looping { get => looping; set => looping = value; }

        public event Action? EndOfStream;

        public void Reset()
        {
            reachedEnd = false;
            position = 0;
        }

        public void FullCommit(uint source)
        {
            stream.Position = Header.DataBegin;
            var data = stream.Read(Header.DataSize);
            fixed (byte* buffer = data)
                al.BufferData(buffers[0], Format, buffer, Header.DataSize, Header.SampleRate);
            al.SetSourceProperty(source, SourceInteger.Buffer, buffers[0]);
        }

        public void Initialize(uint source)
        {
            for (int i = 0; i < bufferCount; i++)
            {
                if (reachedEnd) return;
                var absPosition = Header.DataBegin + position;

                long dataSizeToCopy = bufferSize;
                if (absPosition + bufferSize > stream.Length)
                    dataSizeToCopy = stream.Length - absPosition;
                stream.Position = absPosition;
                stream.Read(buffer, 0, (int)dataSizeToCopy);
                position += (int)dataSizeToCopy;

                if (dataSizeToCopy < bufferSize)
                {
                    if (!looping)
                    {
                        position = 0;
                        EndOfStream?.Invoke();
                        reachedEnd = true;
                        fixed (byte* pData = buffer)
                        {
                            al.BufferData(buffers[i], Format, pData, (int)dataSizeToCopy, Header.SampleRate);
                        }
                        return;
                    }
                    stream.Position = Header.DataBegin;
                    stream.Read(buffer, (int)dataSizeToCopy, (int)(bufferSize - dataSizeToCopy));
                    position = (int)(bufferSize - dataSizeToCopy);
                }

                fixed (byte* pData = buffer)
                {
                    al.BufferData(buffers[i], Format, pData, bufferSize, Header.SampleRate);
                }
                al.SourceQueueBuffers(source, 1, &buffers[i]);
            }
        }

        public void Update(uint source)
        {
            if (reachedEnd) return;
            al.GetSourceProperty(source, GetSourceInteger.BuffersProcessed, out int buffersProcessed);
            if (buffersProcessed <= 0)
                return;
            while (buffersProcessed-- != 0)
            {
                uint bufferId;
                al.SourceUnqueueBuffers(source, 1, &bufferId);

                var absPosition = Header.DataBegin + position;

                long dataSizeToCopy = bufferSize;
                if (absPosition + bufferSize > stream.Length)
                    dataSizeToCopy = stream.Length - absPosition;
                stream.Position = absPosition;
                stream.Read(buffer, 0, (int)dataSizeToCopy);
                position += (int)dataSizeToCopy;

                if (dataSizeToCopy < bufferSize)
                {
                    if (!looping)
                    {
                        position = 0;
                        EndOfStream?.Invoke();
                        reachedEnd = true;
                        fixed (byte* pData = buffer)
                        {
                            al.BufferData(bufferId, Format, pData, (int)dataSizeToCopy, Header.SampleRate);
                        }
                        return;
                    }
                    stream.Position = Header.DataBegin;
                    stream.Read(buffer, (int)dataSizeToCopy, (int)(bufferSize - dataSizeToCopy));
                    position = (int)(bufferSize - dataSizeToCopy);
                }

                fixed (byte* pData = buffer)
                {
                    al.BufferData(bufferId, Format, pData, bufferSize, Header.SampleRate);
                }
                al.SourceQueueBuffers(source, 1, &bufferId);
            }
        }
    }
}