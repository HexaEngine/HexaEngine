namespace HexaEngine.Core
{
    using HexaEngine.Core.Graphics;
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

    public class RenderDispatcher : IDisposable, IRenderDispatcher
    {
        protected ConcurrentQueue<Action> queue = new();
        protected ConcurrentQueue<ICommandList> drawQueue = new();
        protected ConcurrentQueue<ValueTuple<Action, EventWaitHandle>> waitingQueue = new();
        protected ConcurrentQueue<ValueTuple<ICommandList, EventWaitHandle>> drawWaitingQueue = new();
        public readonly Thread DispatcherThread;
        private readonly IGraphicsDevice device;
        private readonly IGraphicsContext context;

        private bool disposedValue;

        public RenderDispatcher(IGraphicsDevice device, Thread thread)
        {
            DispatcherThread = thread;
            this.device = device;
            context = device.CreateDeferredContext();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteQueue(IGraphicsContext immdiateContext)
        {
            while (queue.TryDequeue(out Action? item))
            {
                item();
            }
            while (waitingQueue.TryDequeue(out var item))
            {
                item.Item1();
                item.Item2.Set();
            }
            while (drawQueue.TryDequeue(out var item))
            {
                immdiateContext.ExecuteCommandList(item, 0);
            }
            while (drawWaitingQueue.TryDequeue(out var item))
            {
                immdiateContext.ExecuteCommandList(item.Item1, 0);
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
        public void InvokeOnehitDraw(Action<IGraphicsContext> action)
        {
            action(context);
            var commandList = context.FinishCommandList(0);
            if (DispatcherThread == Thread.CurrentThread)
            {
                drawQueue.Enqueue(commandList);
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                drawWaitingQueue.Enqueue((commandList, handle));
                handle.WaitOne();
                handle.Dispose();
                commandList.Dispose();
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
                await Task.Run(handle.WaitOne);
                handle.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task InvokeOnehitDrawAsync(Action<IGraphicsContext> action)
        {
            action(context);
            var commandList = context.FinishCommandList(0);
            if (DispatcherThread == Thread.CurrentThread)
            {
                drawQueue.Enqueue(commandList);
            }
            else
            {
                EventWaitHandle handle = new(false, EventResetMode.ManualReset);
                drawWaitingQueue.Enqueue((commandList, handle));
                await Task.Run(handle.WaitOne);
                handle.Dispose();
                commandList.Dispose();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                queue.Clear();
                context.Dispose();
                disposedValue = true;
            }
        }

        ~RenderDispatcher()
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