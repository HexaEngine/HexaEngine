namespace HexaEngine.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;

    public class Dispatcher : IDisposable
    {
        private static readonly Dictionary<int, Dispatcher> instances = new();
        protected ConcurrentQueue<Action> queue = new();
        protected ConcurrentQueue<ValueTuple<Action, EventWaitHandle>> waitingQueue = new();

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
            while (CurrentDispatcher.waitingQueue.TryDequeue(out var item))
            {
                item.Item1();
                item.Item2.Set();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(Action action)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action();
            }
            else
            {
                queue.Enqueue(action);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InvokeBlocking(Action action)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action();
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                waitingQueue.Enqueue((action, handle));
                handle.WaitOne();
                handle.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task InvokeAsync(Action action)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action();
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                waitingQueue.Enqueue((action, handle));
                await Task.Run(() => handle.WaitOne());
                handle.Dispose();
            }
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