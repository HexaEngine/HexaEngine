namespace HexaEngine.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;

    public class Dispatcher : IDisposable
    {
        private static readonly Dictionary<int, Dispatcher> instances = new();
        protected ConcurrentQueue<Action> queue = new();

        private bool disposedValue;

        protected Dispatcher()
        {
            DispatcherThread = Thread.CurrentThread;
        }

        public readonly Thread DispatcherThread;

        public static Dispatcher CurrentDispatcher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (instances.TryGetValue(Environment.CurrentManagedThreadId, out Dispatcher? dispatcher))
                {
                    return dispatcher;
                }
                else
                {
                    dispatcher = new();
                    instances[Environment.CurrentManagedThreadId] = dispatcher;
                    return dispatcher;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteQueue()
        {
            while (CurrentDispatcher.queue.TryDequeue(out Action? item))
            {
                item();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(Action action)
        {
            queue.Enqueue(action);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                queue.Clear();
                disposedValue = true;
            }
        }

        ~Dispatcher()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}