namespace HexaEngine.Scenes
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

#if !DEBUG

    using Hexa.NET.Logging;

#endif

    public struct Mutex
    {
        private int value;

        public unsafe static class Utils
        {
            [DllImport("API-MS-Win-Core-Synch-l1-2-0.dll", SetLastError = true)]
            public static extern bool WaitOnAddress(void* address, void* compareAddress, nuint addressSize, uint dwMilliseconds);

            [DllImport("api-ms-win-core-synch-l1-2-0.dll", ExactSpelling = true)]
            public static extern void WakeByAddressSingle(void* address);

            public static bool WaitOnAddress<T>(ref T address, ref T compareAddress, uint dwMilliseconds = uint.MaxValue) where T : unmanaged
            {
                return WaitOnAddress(Unsafe.AsPointer(ref address), Unsafe.AsPointer(ref compareAddress), (nuint)sizeof(T), dwMilliseconds);
            }

            public static void WakeByAddressSingle<T>(ref T address) where T : unmanaged
            {
                WakeByAddressSingle(Unsafe.AsPointer(ref address));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLock()
        {
            return Interlocked.CompareExchange(ref value, 1, 0) == 0;
        }

        public void Lock()
        {
            int desired = 1;
            while (!TryLock())
            {
                Utils.WaitOnAddress(ref value, ref desired);
            }
        }

        public void Unlock()
        {
            Interlocked.Exchange(ref value, 0);
            Utils.WakeByAddressSingle(ref value);
        }
    }

    public static class Lifetime
    {
        private static readonly HashSet<IDisposable> tracked = [];
        private static readonly HashSet<IDisposable> keepAlive = [];

        private static readonly Lock lockObj = new();
        private static int threadId = int.MaxValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ReturnIfCurrentThread()
        {
            return threadId != int.MaxValue && threadId == Environment.CurrentManagedThreadId;
        }

        public static void Track(IDisposable disposable)
        {
            if (ReturnIfCurrentThread()) return;
            lock (lockObj)
            {
                tracked.Add(disposable);
            }
        }

        public static void KeepAlive(IDisposable disposable)
        {
            if (ReturnIfCurrentThread()) return;
            lock (lockObj)
            {
                keepAlive.Add(disposable);
            }
        }

        public static void Untrack(IDisposable disposable)
        {
            if (ReturnIfCurrentThread()) return;
            lock (lockObj)
            {
                tracked.Remove(disposable);
                keepAlive.Remove(disposable);
            }
        }

        public static void DisposeAll()
        {
            lock (lockObj)
            {
                threadId = Environment.CurrentManagedThreadId;
                try
                {
                    List<IDisposable> toDispose = [];
                    foreach (var disposable in tracked)
                    {
                        if (!keepAlive.Contains(disposable))
                        {
                            toDispose.Add(disposable);
                        }
                    }

                    tracked.Clear(); // usually we have less to keep alive than to dispose, so we clear tracked first and then add back the ones we want to keep alive
                    foreach (var instance in keepAlive)
                    {
                        tracked.Add(instance);
                    }
                    keepAlive.Clear();

                    foreach (var disposable in toDispose)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
#if DEBUG
                        catch (Exception)
                        {
                            throw;
#else
                        catch (Exception ex)
                        {
                            LoggerFactory.General.Log(ex);
#endif
                        }
                    }
                }
                finally
                {
                    threadId = int.MaxValue;
                }
            }
        }
    }
}