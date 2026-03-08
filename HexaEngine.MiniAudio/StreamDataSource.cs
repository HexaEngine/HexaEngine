namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;
    using Hexa.NET.Utilities;

    public unsafe struct StreamDataSource
    {
        public MaDataSourceVtable VTable;
        public GCObject<Stream> Stream;
        public MaFormat Format;
        public uint Channels;
        public uint SampleRate;
        public uint FrameSizeInBytes;
        public uint IsLooping;

        public StreamDataSource(Stream stream, MaFormat format, uint channels, uint sampleRate, bool looping = false)
        {
            VTable = new(&Read, &Seek, &GetDataFormat, &GetCursor, &GetLength, &SetLooping);
            Stream = (GCObject<Stream>)stream;
            Format = format;
            Channels = channels;
            SampleRate = sampleRate;
            FrameSizeInBytes = GetFrameSizeInBytes(format, channels);
            IsLooping = looping ? 1u : 0u;
        }

        public void Dispose()
        {
            Stream.Dispose();
        }

        private static uint GetFrameSizeInBytes(MaFormat format, uint channels)
        {
            uint sampleSize = format switch
            {
                MaFormat.U8 => 1,
                MaFormat.S16 => 2,
                MaFormat.S24 => 3,
                MaFormat.S32 => 4,
                MaFormat.F32 => 4,
                _ => throw new NotSupportedException($"Unsupported format: {format}")
            };

            return sampleSize * channels;
        }

        private static MaResult SetLooping(void* pDataSource, uint isLooping)
        {
            if (pDataSource == null)
            {
                return MaResult.InvalidArgs;
            }

            var dataSource = (StreamDataSource*)pDataSource;
            dataSource->IsLooping = isLooping != 0 ? 1u : 0u;
            return MaResult.Success;
        }

        private static MaResult GetLength(void* pDataSource, ulong* pLength)
        {
            if (pDataSource == null || pLength == null)
            {
                return MaResult.InvalidArgs;
            }

            try
            {
                var dataSource = (StreamDataSource*)pDataSource;
                Stream? stream = dataSource->Stream;

                if (stream == null)
                {
                    *pLength = 0;
                    return MaResult.InvalidOperation;
                }

                if (!stream.CanSeek)
                {
                    *pLength = 0;
                    return MaResult.NotImplemented;
                }

                *pLength = (ulong)(stream.Length / dataSource->FrameSizeInBytes);
                return MaResult.Success;
            }
            catch
            {
                *pLength = 0;
                return MaResult.Error;
            }
        }

        private static MaResult GetCursor(void* pDataSource, ulong* pCursor)
        {
            if (pDataSource == null || pCursor == null)
            {
                return MaResult.InvalidArgs;
            }

            try
            {
                var dataSource = (StreamDataSource*)pDataSource;
                Stream? stream = dataSource->Stream;

                if (stream == null)
                {
                    *pCursor = 0;
                    return MaResult.InvalidOperation;
                }

                if (!stream.CanSeek)
                {
                    *pCursor = 0;
                    return MaResult.NotImplemented;
                }

                *pCursor = (ulong)(stream.Position / dataSource->FrameSizeInBytes);
                return MaResult.Success;
            }
            catch
            {
                *pCursor = 0;
                return MaResult.Error;
            }
        }

        private static MaResult GetDataFormat(void* pDataSource, MaFormat* pFormat, uint* pChannels, uint* pSampleRate, byte* pChannelMap, nuint channelMapCap)
        {
            if (pDataSource == null)
            {
                return MaResult.InvalidArgs;
            }
            var dataSource = (StreamDataSource*)pDataSource;

            if (pFormat != null)
            {
                *pFormat = dataSource->Format;
            }

            if (pChannels != null)
            {
                *pChannels = dataSource->Channels;
            }

            if (pSampleRate != null)
            {
                *pSampleRate = dataSource->SampleRate;
            }

            return MaResult.Success;
        }

        private static MaResult Seek(void* pDataSource, ulong frameIndex)
        {
            if (pDataSource == null)
            {
                return MaResult.InvalidArgs;
            }

            try
            {
                var dataSource = (StreamDataSource*)pDataSource;
                Stream? stream = dataSource->Stream;

                if (stream == null)
                {
                    return MaResult.InvalidOperation;
                }

                if (!stream.CanSeek)
                {
                    return MaResult.NotImplemented;
                }

                long byteOffset = checked((long)(frameIndex * dataSource->FrameSizeInBytes));
                stream.Seek(byteOffset, SeekOrigin.Begin);
                return MaResult.Success;
            }
            catch
            {
                return MaResult.Error;
            }
        }

        private static MaResult Read(void* pDataSource, void* pFramesOut, ulong frameCount, ulong* pFramesRead)
        {
            if (pDataSource == null || pFramesOut == null)
            {
                if (pFramesRead != null)
                {
                    *pFramesRead = 0;
                }

                return MaResult.InvalidArgs;
            }

            try
            {
                var dataSource = (StreamDataSource*)pDataSource;
                Stream? stream = dataSource->Stream;

                if (stream == null)
                {
                    if (pFramesRead != null)
                    {
                        *pFramesRead = 0;
                    }

                    return MaResult.InvalidOperation;
                }

                int bytesToRead = checked((int)(frameCount * dataSource->FrameSizeInBytes));
                Span<byte> dst = new(pFramesOut, bytesToRead);

                int bytesReadTotal = 0;

                while (bytesReadTotal < bytesToRead)
                {
                    int read = stream.Read(dst[bytesReadTotal..]);
                    if (read == 0)
                    {
                        if (dataSource->IsLooping != 0 && stream.CanSeek)
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            continue;
                        }

                        break;
                    }

                    bytesReadTotal += read;
                }

                ulong framesRead = (ulong)(bytesReadTotal / dataSource->FrameSizeInBytes);

                if (pFramesRead != null)
                {
                    *pFramesRead = framesRead;
                }

                if (bytesReadTotal < bytesToRead)
                {
                    dst[bytesReadTotal..].Clear();
                }

                return MaResult.Success;
            }
            catch
            {
                if (pFramesRead != null)
                {
                    *pFramesRead = 0;
                }

                return MaResult.Error;
            }
        }
    }
}
