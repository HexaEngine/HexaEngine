namespace HexaEngine.XAudio
{
    using HexaEngine.Audio.Common.Wave;
    using System;

    public unsafe class XAudio2WaveAudioStream : IDisposable
    {
        private readonly Stream stream;
        private readonly bool leaveOpen;
        private readonly byte** buffers;
        private byte* fullCommitBuffer;
        private readonly int bufferCount;
        private readonly uint bufferSize;
        public readonly uint SampleOffset;
        public readonly uint ByteOffset;
        public readonly WaveHeader Header;
        private int currentBuffer;
        private int position;
        private bool looping;
        private bool reachedEnd;
        private bool disposedValue;

        public XAudio2WaveAudioStream(Stream stream, bool leaveOpen = false, int bufferCount = 3, uint bufferSize = 65536)
        {
            Header = new(stream);
            if (Header.WaveFormat != WaveFormatEncoding.Pcm)
            {
                throw new NotSupportedException("Wav PCM only");
            }

            this.stream = stream;
            this.leaveOpen = leaveOpen;
            this.bufferCount = bufferCount;
            this.bufferSize = bufferSize;
            buffers = (byte**)Alloc<nint>(bufferCount);
            for (int i = 0; i < bufferCount; i++)
            {
                buffers[i] = Alloc<byte>((int)bufferSize);
            }
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

        public void FullCommit(ComPtr<IXAudio2SourceVoice> source)
        {
            stream.Position = Header.DataBegin;
            fullCommitBuffer = Alloc<byte>(Header.DataSize);

            stream.Read(new Span<byte>(fullCommitBuffer, Header.DataSize));

            XAudio2Buffer buffer = default;
            buffer.PAudioData = fullCommitBuffer;
            buffer.AudioBytes = (uint)Header.DataSize;
            buffer.Flags = XAudio2.XAudio2_END_OF_STREAM;

            source.SubmitSourceBuffer(ref buffer, null);
        }

        public WaveFormatEx GetWaveFormat()
        {
            WaveFormatEx waveFormat;
            waveFormat.Size = 0;
            waveFormat.BitsPerSample = (ushort)Header.BitsPerSample;
            waveFormat.SamplesPerSec = (uint)Header.SampleRate;
            waveFormat.Channels = (ushort)Header.NumChannels;
            waveFormat.BlockAlign = (ushort)Header.BlockAlign;
            waveFormat.AvgBytesPerSec = (uint)Header.BytesPerSecond;
            waveFormat.FormatTag = (ushort)Header.WaveFormat;
            return waveFormat;
        }

        public void Initialize(ComPtr<IXAudio2SourceVoice> source)
        {
            for (int i = 0; i < bufferCount; i++)
            {
                if (reachedEnd)
                {
                    return;
                }

                XAudio2Buffer submitBuffer = default;
                var buffer = buffers[i];
                var absPosition = Header.DataBegin + position;

                long dataSizeToCopy = bufferSize;
                if (absPosition + bufferSize > stream.Length)
                {
                    dataSizeToCopy = stream.Length - absPosition;
                }

                stream.Position = absPosition;
                stream.Read(new Span<byte>(buffer, (int)dataSizeToCopy));
                position += (int)dataSizeToCopy;

                if (dataSizeToCopy < bufferSize)
                {
                    if (!looping)
                    {
                        position = 0;
                        EndOfStream?.Invoke();
                        reachedEnd = true;

                        submitBuffer.PAudioData = buffer;
                        submitBuffer.AudioBytes = (uint)dataSizeToCopy;
                        submitBuffer.Flags = XAudio2.XAudio2_END_OF_STREAM;
                        source.SubmitSourceBuffer(ref submitBuffer, null);

                        return;
                    }
                    stream.Position = Header.DataBegin;
                    stream.Read(new Span<byte>(&buffer[dataSizeToCopy], (int)(bufferSize - dataSizeToCopy)));
                    position = (int)(bufferSize - dataSizeToCopy);
                }

                submitBuffer.PAudioData = buffer;
                submitBuffer.AudioBytes = bufferSize;
                source.SubmitSourceBuffer(ref submitBuffer, null);
            }
        }

        public void Update(ComPtr<IXAudio2SourceVoice> source)
        {
            if (reachedEnd)
            {
                return;
            }

            XAudio2VoiceState state;
            source.GetState(&state, 0);
            if (state.BuffersQueued < bufferCount)
            {
                XAudio2Buffer submitBuffer = default;
                var buffer = buffers[currentBuffer];
                var absPosition = Header.DataBegin + position;

                long dataSizeToCopy = bufferSize;
                if (absPosition + bufferSize > stream.Length)
                {
                    dataSizeToCopy = stream.Length - absPosition;
                }

                stream.Position = absPosition;
                stream.Read(new Span<byte>(buffer, (int)dataSizeToCopy));
                position += (int)dataSizeToCopy;

                if (dataSizeToCopy < bufferSize)
                {
                    if (!looping)
                    {
                        position = 0;
                        EndOfStream?.Invoke();
                        reachedEnd = true;

                        submitBuffer.PAudioData = buffer;
                        submitBuffer.AudioBytes = (uint)dataSizeToCopy;
                        submitBuffer.Flags = XAudio2.XAudio2_END_OF_STREAM;
                        source.SubmitSourceBuffer(ref submitBuffer, null);

                        return;
                    }
                    stream.Position = Header.DataBegin;
                    stream.Read(new Span<byte>(&buffer[dataSizeToCopy], (int)(bufferSize - dataSizeToCopy)));
                    position = (int)(bufferSize - dataSizeToCopy);
                }

                submitBuffer.PAudioData = buffer;
                submitBuffer.AudioBytes = bufferSize;
                source.SubmitSourceBuffer(ref submitBuffer, null);

                currentBuffer = (currentBuffer + 1) % bufferCount;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (fullCommitBuffer != null)
                {
                    Free(fullCommitBuffer);
                }

                for (int i = 0; i < bufferCount; i++)
                {
                    Free(buffers[i]);
                }
                Free(buffers);
                if (!leaveOpen)
                {
                    stream.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}