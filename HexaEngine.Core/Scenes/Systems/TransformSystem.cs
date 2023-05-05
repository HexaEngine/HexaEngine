﻿namespace HexaEngine.Core.Scenes.Systems
{
    using HexaEngine.Mathematics;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class TransformSystem : ISystem
    {
        private readonly List<GameObject> objects = new();
        private readonly ConcurrentQueue<Transform> updateQueue = new();

        public string Name => "TransformUpdate";

        public SystemFlags Flags => SystemFlags.LateUpdate;

        public void Awake()
        {
        }

        public void Destroy()
        {
        }

        public void FixedUpdate()
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
            {
                if (!updateQueue.Contains(transform))
                {
                    updateQueue.Enqueue(transform);
                }
            }
        }

        public void Unregister(GameObject gameObject)
        {
            objects.Remove(gameObject);
            gameObject.Transform.Changed -= TransformChanged;
        }

        public void Update(float dt)
        {
            if (updateQueue.IsEmpty)
            {
                return;
            }

            while (updateQueue.TryDequeue(out Transform? transform))
            {
                transform.Recalculate();
            }
        }
    }
}