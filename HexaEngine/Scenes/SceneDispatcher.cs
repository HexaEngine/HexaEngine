namespace HexaEngine.Scenes
{
    using HexaEngine.Core.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;

    /// <summary>
    /// Used to Sync threads with the main render thread.
    /// To prevent thread-safety problems.
    /// </summary>
    public class SceneDispatcher
    {
        private readonly ConcurrentQueue<IDispatcherTask> invokes = new();

        private static readonly ILogger logger = LoggerFactory.GetLogger(nameof(SceneDispatcher));
        private long timeBudget = long.MaxValue;

        public interface IDispatcherTask
        {
            void Invoke();
        }

        public struct DispatcherTask(object? context, Action<object?> callback) : IDispatcherTask
        {
            public object? Context = context;
            public Action<object?> Callback = callback;

            public readonly void Invoke()
            {
                Callback(Context);
            }
        }

        public struct DispatcherTask<T>(T context, Action<T> callback) : IDispatcherTask
        {
            public T Context = context;
            public Action<T> Callback = callback;

            public readonly void Invoke()
            {
                Callback(Context);
            }
        }

        /// <summary>
        /// Sets the time budget of the thread dispatcher.
        /// </summary>
        public TimeSpan TimeBudget { get => new(timeBudget); set => timeBudget = value.Ticks; }

        public void ExecuteInvokes()
        {
            long start = Stopwatch.GetTimestamp();
            try
            {
                while (invokes.TryDequeue(out var task))
                {
                    task.Invoke();
                    long now = Stopwatch.GetTimestamp();
                    long delta = now - start;
                    if (delta > timeBudget)
                    {
                        logger.Warn("Time budget exceeded.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Invoke failed.");
                logger.Log(ex);
            }
        }

        public void Invoke(object? context, Action<object?> action)
        {
            invokes.Enqueue(new DispatcherTask(context, action));
        }

        public void Invoke<T>(T context, Action<T> action)
        {
            invokes.Enqueue(new DispatcherTask<T>(context, x => action(x)));
        }

        public void Invoke(IDispatcherTask task)
        {
            invokes.Enqueue(task);
        }
    }
}