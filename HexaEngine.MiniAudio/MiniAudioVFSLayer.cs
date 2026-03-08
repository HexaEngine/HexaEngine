namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;
    using HexaGen.Runtime;
    using System.Runtime.InteropServices;

    internal unsafe class MiniAudioVFSLayer : IDisposable
    {
        MaVfsCallbacks* callbacks;
        NativeCallback<OnOpen> onOpenCallback;
        NativeCallback<OnOpenW> onOpenWCallback;
        NativeCallback<OnClose> onCloseCallback;
        NativeCallback<OnRead> onReadCallback;
        NativeCallback<OnWrite> onWriteCallback;
        NativeCallback<OnSeek> onSeekCallback;
        NativeCallback<OnTell> onTellCallback;
        NativeCallback<OnInfo> onInfoCallback;

        public MiniAudioVFSLayer()
        {
            onOpenCallback = new(Open);
            onOpenWCallback = new(OpenW);
            onCloseCallback = new(Close);
            onReadCallback = new(Read);
            onWriteCallback = new(Write);
            onSeekCallback = new(Seek);
            onTellCallback = new(Tell);
            onInfoCallback = new(Info);
            callbacks = AllocT<MaVfsCallbacks>();
            callbacks->OnOpen = (void*)Marshal.GetFunctionPointerForDelegate(onOpenCallback);
            callbacks->OnOpenW = (void*)Marshal.GetFunctionPointerForDelegate(onOpenWCallback);
            callbacks->OnClose = (void*)Marshal.GetFunctionPointerForDelegate(onCloseCallback);
            callbacks->OnRead = (void*)Marshal.GetFunctionPointerForDelegate(onReadCallback);
            callbacks->OnWrite = (void*)Marshal.GetFunctionPointerForDelegate(onWriteCallback);
            callbacks->OnSeek = (void*)Marshal.GetFunctionPointerForDelegate(onSeekCallback);
            callbacks->OnTell = (void*)Marshal.GetFunctionPointerForDelegate(onTellCallback);
            callbacks->OnInfo = (void*)Marshal.GetFunctionPointerForDelegate(onInfoCallback);
        }

        public MaVfsCallbacks* Callbacks => callbacks;

        private MaResult Open(void* pVFs, byte* pFilePath, uint openMode, void** pFile)
        {
            throw new NotImplementedException();
        }

        private MaResult OpenW(void* pVFs, char* pFilePath, uint openMode, void** pFile)
        {
            throw new NotImplementedException();
        }

        private MaResult Close(void* pVFs, void* file)
        {
            throw new NotImplementedException();
        }

        private MaResult Read(void* pDataSource, void* pFramesOut, ulong frameCount, ulong* pFramesRead)
        {
            throw new NotImplementedException();
        }

        private MaResult Write(void* pVFs, void* file, void* pSrc, nuint sizeInBytes, nuint* pBytesWritten)
        {
            throw new NotImplementedException();
        }

        private MaResult Seek(void* pDataSource, ulong frameIndex)
        {
            throw new NotImplementedException();
        }

        private MaResult Tell(void* pVFs, void* file, long* pCursor)
        {
            throw new NotImplementedException();
        }

        private MaResult Info(void* pVFs, void* file, MaFileInfo* pInfo)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (callbacks != null)
            {
                onOpenCallback.Dispose();
                onOpenWCallback.Dispose();
                onCloseCallback.Dispose();
                onReadCallback.Dispose();
                onWriteCallback.Dispose();
                onSeekCallback.Dispose();
                onTellCallback.Dispose();
                onInfoCallback.Dispose();
                Free(callbacks);
                callbacks = null;
            }
        }
    }
}
