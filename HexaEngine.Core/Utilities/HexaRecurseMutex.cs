using System.Runtime.InteropServices;

namespace HexaEngine.Core.Utilities
{
    public unsafe struct HexaRecurseMutex : IDisposable
    {
        public void* Handle;

        [DllImport("HexaUtils.dll", EntryPoint = "HexaUtils_CreateRecurseMutex", CallingConvention = CallingConvention.Cdecl)]
        private static extern void* CreateRecurseMutex();

        [DllImport("HexaUtils.dll", EntryPoint = "HexaUtils_DestroyRecurseMutex", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyRecurseMutex(void* mtx);

        [DllImport("HexaUtils.dll", EntryPoint = "HexaUtils_RecurseMutexLock", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Lock(void* mtx);

        [DllImport("HexaUtils.dll", EntryPoint = "HexaUtils_RecurseMutexTryLock", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool TryLock(void* mtx);

        [DllImport("HexaUtils.dll", EntryPoint = "HexaUtils_RecurseMutexUnlock", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Unlock(void* mtx);

        public HexaRecurseMutex()
        {
            Handle = CreateRecurseMutex();
        }

        public readonly LockGuard Lock()
        {
            return new(Handle);
        }

        public readonly bool TryLock()
        {
            return TryLock(Handle);
        }

        public readonly void Unlock()
        {
            Unlock(Handle);
        }

        public void Dispose()
        {
            if (Handle != null)
            {
                DestroyRecurseMutex(Handle);
                Handle = null;
            }
        }

        public struct LockGuard : IDisposable
        {
            private void* mutex;

            public LockGuard(void* mutex)
            {
                Lock(mutex);
                this.mutex = mutex;
            }

            public void Dispose()
            {
                if (mutex != null)
                {
                    Unlock(mutex);
                    mutex = null;
                }
            }
        }
    }
}

