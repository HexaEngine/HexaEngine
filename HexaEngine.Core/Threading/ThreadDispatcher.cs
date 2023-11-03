namespace HexaEngine.Core.Windows
{
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A class that provides a thread dispatcher to execute actions on a specific thread or execution context.
    /// </summary>
    public class ThreadDispatcher : IThreadDispatcher
    {
        protected ConcurrentQueue<ValueTuple<Action, EventWaitHandle?>> waitingQueue = new();
        protected ConcurrentQueue<ValueTuple<Action<object>, object, EventWaitHandle?>> waitingStateQueue = new();
        public readonly Thread DispatcherThread;

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadDispatcher"/> class with the specified thread.
        /// </summary>
        /// <param name="thread">The thread on which to execute actions.</param>
        public ThreadDispatcher(Thread thread)
        {
            DispatcherThread = thread;
        }

        /// <summary>
        /// Executes the actions in the waiting queue.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteQueue()
        {
            while (waitingQueue.TryDequeue(out var item))
            {
                item.Item1();
                item.Item2?.Set();
            }
            while (waitingStateQueue.TryDequeue(out var item))
            {
                item.Item1(item.Item2);
                item.Item3?.Set();
            }
        }

        /// <summary>
        /// Invokes the specified action on the associated thread or execution context.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(Action action)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action();
            }
            else
            {
                waitingQueue.Enqueue((action, null));
            }
        }

        /// <summary>
        /// Invokes the specified action on the associated thread or execution context, blocking the calling thread until the action completes.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
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

        /// <summary>
        /// Asynchronously invokes the specified action on the associated thread or execution context.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
                await Task.Run(handle.WaitOne);
                handle.Dispose();
            }
        }

        /// <summary>
        /// Invokes the specified action on the associated thread or execution context with additional state information.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="state">An object that contains data to be used by the action.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(Action<object> action, object state)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action(state);
            }
            else
            {
                waitingStateQueue.Enqueue((action, state, null));
            }
        }

        /// <summary>
        /// Asynchronously invokes the specified action on the associated thread or execution context with additional state information.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="state">An object that contains data to be used by the action.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task InvokeAsync(Action<object> action, object state)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action(state);
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                waitingStateQueue.Enqueue((action, state, handle));
                await Task.Run(handle.WaitOne);
                handle.Dispose();
            }
        }

        /// <summary>
        /// Invokes the specified action on the associated thread or execution context with additional state information, blocking the calling thread until the action completes.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="state">An object that contains data to be used by the action.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InvokeBlocking(Action<object> action, object state)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action(state);
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                waitingStateQueue.Enqueue((action, state, handle));
                handle.WaitOne();
                handle.Dispose();
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                waitingQueue.Clear();
                waitingStateQueue.Clear();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ThreadDispatcher"/> class.
        /// </summary>
        ~ThreadDispatcher()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Disposes of the resources used by the <see cref="ThreadDispatcher"/> instance.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}