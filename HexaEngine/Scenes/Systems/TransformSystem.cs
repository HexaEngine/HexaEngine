namespace HexaEngine.Scenes.Systems
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;
    using System.Collections.Concurrent;

    public class TransformSystem : ISceneSystem
    {
        private readonly ObjectTypeQuery<GameObject> objects = new(Queries.QueryFlags.ObjectAdded | Queries.QueryFlags.ObjectRemoved);
        private readonly ConcurrentQueue<Transform> updateQueue = new();

        public string Name => "TransformUpdate";

        public SystemFlags Flags => SystemFlags.LateUpdate | SystemFlags.Awake | SystemFlags.Destroy;

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(objects);
            objects.OnAdded += OnAdded;
            objects.OnRemoved += OnRemoved;
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].TransformChanged += TransformChanged;
                objects[i].Transform.Recalculate();
            }
        }

        private void OnRemoved(GameObject gameObject)
        {
            gameObject.TransformChanged -= TransformChanged;
        }

        private void OnAdded(GameObject gameObject)
        {
            gameObject.TransformChanged += TransformChanged;
            gameObject.Transform.Recalculate();
        }

        public void Destroy()
        {
            objects.OnAdded -= OnAdded;
            objects.OnRemoved -= OnRemoved;
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].TransformChanged -= TransformChanged;
            }
            objects.Dispose();
            updateQueue.Clear();
        }

        private void TransformChanged(GameObject sender, Transform e)
        {
            updateQueue.Enqueue(e);
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