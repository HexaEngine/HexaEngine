namespace HexaEngine.Core.Scenes
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Used to Sync threads with the main render thread.
    /// To prevent thread-safety problems.
    /// </summary>
    public class SceneDispatcher
    {
        private readonly ConcurrentQueue<(object, Action<object>)> invokes = new();

        public void ExecuteInvokes()
        {
            while (invokes.TryDequeue(out var action))
            {
                action.Item2.Invoke(action.Item1);
            }
        }

        public void Invoke(object context, Action<object> action)
        {
            invokes.Enqueue((context, action));
        }

        public void Invoke<T>(T context, Action<T> action)
        {
            invokes.Enqueue((context, x => action((T)x)));
        }
    }
}