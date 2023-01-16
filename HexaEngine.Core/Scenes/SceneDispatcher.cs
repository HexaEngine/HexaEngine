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
        private readonly ConcurrentQueue<Action> invokes = new();

        public void ExecuteInvokes()
        {
            while (invokes.TryDequeue(out Action? action))
            {
                action.Invoke();
            }
        }

        public void Invoke(Action action)
        {
            invokes.Enqueue(action);
        }
    }
}