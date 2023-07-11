namespace HexaEngine.Scenes
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
#pragma warning disable CS8620 // Argument of type '(object? context, Action<object>)' cannot be used for parameter 'item' of type '(object, Action<object>)' in 'void ConcurrentQueue<(object, Action<object>)>.Enqueue((object, Action<object>) item)' due to differences in the nullability of reference types.
            invokes.Enqueue((context, x => action((T)x)));
#pragma warning restore CS8620 // Argument of type '(object? context, Action<object>)' cannot be used for parameter 'item' of type '(object, Action<object>)' in 'void ConcurrentQueue<(object, Action<object>)>.Enqueue((object, Action<object>) item)' due to differences in the nullability of reference types.
        }
    }
}