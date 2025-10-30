using System.Runtime.InteropServices;

namespace HexaEngine.Core.Utilities
{
    public unsafe struct HexaSemaphore : IDisposable
    {
        public void* Handle;

        [DllImport("HexaUtils.dll", EntryPoint = "HexaUtils_CreateHexaSemaphore", CallingConvention = CallingConvention.Cdecl)]
        private static extern void* CreateSemaphore(nuint maxCount, nuint initalCount);

        [DllImport("HexaUtils.dll", EntryPoint = "HexaUtils_DestroyHexaSemaphore", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroySemaphore(void* sema);

        [DllImport("HexaUtils.dll", EntryPoint = "HexaUtils_HexaSemaphoreAcquire", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Acquire(void* sema);

        [DllImport("HexaUtils.dll", EntryPoint = "HexaUtils_HexaSemaphoreTryAcquire", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool TryAcquire(void* sema);

        [DllImport("HexaUtils.dll", EntryPoint = "HexaUtils_HexaSemaphoreRelease", CallingConvention = CallingConvention.Cdecl)]
        private static extern nuint Release(void* sema);

        public HexaSemaphore(nuint maxCount)
        {
            Handle = CreateSemaphore(maxCount, maxCount);
        }

        public HexaSemaphore(nuint maxCount, nuint initalCount)
        {
            Handle = CreateSemaphore(maxCount, initalCount);
        }

        public readonly void Acquire()
        {
            Acquire(Handle);
        }

        public readonly bool TryAcquire()
        {
            return TryAcquire(Handle);
        }

        public readonly nuint Release()
        {
            return Release(Handle);
        }

        public void Dispose()
        {
            if (Handle != null)
            {
                DestroySemaphore(Handle);
                Handle = null;
            }
        }

        public readonly LockGuard Lock()
        {
            return new LockGuard(Handle);
        }

        public struct LockGuard : IDisposable
        {
            private void* semaphore;

            public LockGuard(void* semaphore)
            {
                Acquire(semaphore);
                this.semaphore = semaphore;
            }

            public void Dispose()
            {
                if (semaphore != null)
                {
                    Release(semaphore);
                    semaphore = null;
                }
            }
        }
    }
}
