namespace HexaEngine.Core.Threading
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A class that provides a thread dispatcher to execute actions on a specific thread or execution context.
    /// </summary>
    public class ThreadDispatcher : IThreadDispatcher
    {
        /// <summary>
        /// Represents a concurrent queue of actions with associated wait handles.
        /// </summary>
        protected ConcurrentQueue<IThreadDispatcherTask> waitingQueue = new();

        public interface IThreadDispatcherTask
        {
            void Invoke();
        }

        protected struct ThreadDispatcherTask(Action action, EventWaitHandle? waitHandle = null) : IThreadDispatcherTask
        {
            public Action Action = action;
            public EventWaitHandle? WaitHandle = waitHandle;

            public readonly void Invoke()
            {
                Action();
                WaitHandle?.Set();
            }
        }

        protected struct ThreadDispatcherTaskStated(Action<object> action, object state, EventWaitHandle? waitHandle = null) : IThreadDispatcherTask
        {
            public Action<object> Action = action;
            public object State = state;
            public EventWaitHandle? WaitHandle = waitHandle;

            public readonly void Invoke()
            {
                Action(State);
                WaitHandle?.Set();
            }
        }

        protected struct ThreadDispatcherTaskStated<T>(Action<T> action, T state, EventWaitHandle? waitHandle = null) : IThreadDispatcherTask
        {
            public Action<T> Action = action;
            public T State = state;
            public EventWaitHandle? WaitHandle = waitHandle;

            public readonly void Invoke()
            {
                Action(State);
                WaitHandle?.Set();
            }
        }

        /// <summary>
        /// Gets the thread associated with the dispatcher.
        /// </summary>
        public readonly Thread DispatcherThread;

        private bool disposedValue;
        private long timeBudget = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadDispatcher"/> class with the specified thread.
        /// </summary>
        /// <param name="thread">The thread on which to execute actions.</param>
        public ThreadDispatcher(Thread thread)
        {
            DispatcherThread = thread;
        }

        /// <summary>
        /// Sets the time budget of the thread dispatcher.
        /// </summary>
        public TimeSpan TimeBudget { get => new(timeBudget); set => timeBudget = value.Ticks; }

        /// <summary>
        /// Executes the actions in the waiting queue.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteQueue()
        {
            long start = Stopwatch.GetTimestamp();

            while (waitingQueue.TryDequeue(out var item))
            {
                item.Invoke();
                long now = Stopwatch.GetTimestamp();
                long delta = now - start;
                if (timeBudget != -1 && delta > timeBudget)
                {
                    return;
                }
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
                waitingQueue.Enqueue(new ThreadDispatcherTask(action));
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
                waitingQueue.Enqueue(new ThreadDispatcherTask(action, handle));
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
        public Task InvokeAsync(Action action)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action();
                return Task.CompletedTask;
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                waitingQueue.Enqueue(new ThreadDispatcherTask(action, handle));
                Task task = Task.Factory.StartNew(state => { handle.WaitOne(); handle.Dispose(); }, handle);
                return task;
            }
        }

        /// <summary>
        ///  Invokes the specified action on the associated thread or execution context with additional state information, and returns the wait handle, the caller takes ownership of the handle, dispose properly.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <returns>A <see cref="WaitHandle"/> to wait for operation completion, null when the current thread is the executing thread.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventWaitHandle? InvokeWaitHandle(Action action)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action();
                return null;
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                waitingQueue.Enqueue(new ThreadDispatcherTask(action, handle));
                return handle;
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
                waitingQueue.Enqueue(new ThreadDispatcherTaskStated(action, state));
            }
        }

        /// <summary>
        /// Asynchronously invokes the specified action on the associated thread or execution context with additional state information.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="state">An object that contains data to be used by the action.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task InvokeAsync(Action<object> action, object state)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action(state);
                return Task.CompletedTask;
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                waitingQueue.Enqueue(new ThreadDispatcherTaskStated(action, state, handle));
                Task task = Task.Factory.StartNew(state => { handle.WaitOne(); handle.Dispose(); }, handle);
                return task;
            }
        }

        /// <summary>
        ///  Invokes the specified action on the associated thread or execution context with additional state information, and returns the wait handle, the caller takes ownership of the handle, dispose properly.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="state">An object that contains data to be used by the action.</param>
        /// <returns>A <see cref="WaitHandle"/> to wait for operation completion, null when the current thread is the executing thread.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventWaitHandle? InvokeWaitHandle(Action<object> action, object state)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action(state);
                return null;
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                waitingQueue.Enqueue(new ThreadDispatcherTaskStated(action, state, handle));
                return handle;
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
                waitingQueue.Enqueue(new ThreadDispatcherTaskStated(action, state));
                handle.WaitOne();
                handle.Dispose();
            }
        }

        /// <summary>
        /// Invokes the specified action on the associated thread or execution context with additional state information.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="state">An object that contains data to be used by the action.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke<T>(Action<T> action, T state)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action(state);
            }
            else
            {
                waitingQueue.Enqueue(new ThreadDispatcherTaskStated<T>(action, state));
            }
        }

        /// <summary>
        /// Asynchronously invokes the specified action on the associated thread or execution context with additional state information.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="state">An object that contains data to be used by the action.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task InvokeAsync<T>(Action<T> action, T state)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action(state);
                return Task.CompletedTask;
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                waitingQueue.Enqueue(new ThreadDispatcherTaskStated<T>(action, state, handle));
                Task task = Task.Factory.StartNew(state => { handle.WaitOne(); handle.Dispose(); }, handle);
                return task;
            }
        }

        /// <summary>
        ///  Invokes the specified action on the associated thread or execution context with additional state information, and returns the wait handle, the caller takes ownership of the handle, dispose properly.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="state">An object that contains data to be used by the action.</param>
        /// <returns>A <see cref="WaitHandle"/> to wait for operation completion, null when the current thread is the executing thread.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventWaitHandle? InvokeWaitHandle<T>(Action<T> action, T state)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action(state);
                return null;
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                waitingQueue.Enqueue(new ThreadDispatcherTaskStated<T>(action, state, handle));
                return handle;
            }
        }

        /// <summary>
        /// Invokes the specified action on the associated thread or execution context with additional state information, blocking the calling thread until the action completes.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="state">An object that contains data to be used by the action.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InvokeBlocking<T>(Action<T> action, T state)
        {
            if (DispatcherThread == Thread.CurrentThread)
            {
                action(state);
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                waitingQueue.Enqueue(new ThreadDispatcherTaskStated<T>(action, state));
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
                disposedValue = true;
            }
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