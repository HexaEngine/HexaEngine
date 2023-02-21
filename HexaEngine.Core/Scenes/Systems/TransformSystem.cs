namespace HexaEngine.Core.Scenes.Systems
{
    using BepuUtilities;
    using HexaEngine.Mathematics;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class TransformSystem : ISystem
    {
        private readonly List<GameObject> objects = new();
        private readonly ConcurrentQueue<Transform> updateQueue = new();

        public string Name => "TransformUpdate";

        public void Awake(ThreadDispatcher dispatcher)
        {
        }

        public void Destroy(ThreadDispatcher dispatcher)
        {
        }

        public void FixedUpdate(ThreadDispatcher dispatcher)
        {
        }

        public void Register(GameObject gameObject)
        {
            objects.Add(gameObject);
            gameObject.Transform.Changed += TransformChanged;
            updateQueue.Enqueue(gameObject.Transform);
        }

        private void TransformChanged(object? sender, EventArgs e)
        {
            if (sender is Transform transform)
                if (!updateQueue.Contains(transform))
                    updateQueue.Enqueue(transform);
        }

        public void Unregister(GameObject gameObject)
        {
            objects.Remove(gameObject);
            gameObject.Transform.Changed -= TransformChanged;
        }

        public void Update(ThreadDispatcher dispatcher)
        {
            if (updateQueue.IsEmpty) return;
            while (updateQueue.TryDequeue(out Transform? transform))
            {
                transform.Recalculate();
            }
        }

        private void Update(int i)
        {
            updateQueue.TryDequeue(out var transform);
            transform?.Recalculate();
        }
    }
}