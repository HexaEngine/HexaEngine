namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;
    using Hexa.NET.Utilities;
    using HexaEngine.Core.IO;

    internal unsafe class MiniAudioVFSLayer : IDisposable
    {
        MaVfsCallbacks* callbacks;

        public MiniAudioVFSLayer()
        {
            callbacks = AllocT<MaVfsCallbacks>();
            *callbacks = new(&Open, &OpenW, &Close, &Read, &Write, &Seek, &Tell, &Info);
        }

        private static MaResult Write(void* pVFs, void* file, void* pSrc, nuint sizeInBytes, nuint* pBytesWritten)
        {
            throw new NotImplementedException();
        }

        private static MaResult Seek(void* pVFs, void* file, long offset, MaSeekOrigin origin)
        {
            var stream = GCUtils.GetObject<VirtualStream>(file);

            SeekOrigin seekOrigin = origin switch
            {
                MaSeekOrigin.Start => SeekOrigin.Begin,
                MaSeekOrigin.Current => SeekOrigin.Current,
                MaSeekOrigin.End => SeekOrigin.End,
                _ => (SeekOrigin)int.MaxValue
            };

            if (seekOrigin == (SeekOrigin)int.MaxValue)
            {
                return MaResult.InvalidArgs;
            }

            stream.Seek(offset, seekOrigin);
            return MaResult.Success;
        }

        private static MaResult Read(void* pVFs, void* file, void* pDst, nuint sizeInBytes, nuint* pBytesRead)
        {
            var stream = GCUtils.GetObject<VirtualStream>(file);
            ulong read = 0;
            byte* dst = (byte*)pDst;
            while (read < sizeInBytes)
            {
                ulong remaining = sizeInBytes - read;
                ulong toRead = Math.Min(remaining, int.MaxValue);
                Span<byte> span = new(dst + read, (int)toRead);
                int rd = stream.Read(span);
                read += (ulong)rd;
                if (rd == 0)
                {
                    break;
                }
            }

            *pBytesRead = (nuint)read;
            return MaResult.Success;
        }

        public MaVfsCallbacks* Callbacks => callbacks;

        private static MaResult Open(void* pVFs, byte* pFilePath, uint openMode, void** pFile)
        {
            var path = ToStringFromUTF8(pFilePath);
            if (string.IsNullOrEmpty(path))
            {
                return MaResult.InvalidArgs;
            }
            var stream = FileSystem.OpenRead(path);
            *pFile = GCUtils.GCAlloc(stream);
            return MaResult.Success;
        }

        private static MaResult OpenW(void* pVFs, char* pFilePath, uint openMode, void** pFile)
        {
            throw new NotImplementedException();
        }

        private static MaResult Close(void* pVFs, void* file)
        {
            var stream = GCUtils.GetObject<VirtualStream>(file);
            stream.Dispose();
            GCUtils.GCFree(file);
            return MaResult.Success;
        }


        private static MaResult Tell(void* pVFs, void* file, long* pCursor)
        {
            var stream = GCUtils.GetObject<VirtualStream>(file);
            *pCursor = stream.Position;
            return MaResult.Success;
        }

        private static MaResult Info(void* pVFs, void* file, MaFileInfo* pInfo)
        {
            var stream = GCUtils.GetObject<VirtualStream>(file);
            pInfo->SizeInBytes = (ulong)stream.Length;
            return MaResult.Success;
        }

        public void Dispose()
        {
            if (callbacks != null)
            {
                Free(callbacks);
                callbacks = null;
            }
        }
    }
}
