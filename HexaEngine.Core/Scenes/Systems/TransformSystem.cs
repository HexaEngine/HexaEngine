namespace HexaEngine.Core.Scenes.Systems
{
    using BepuUtilities;
    using System.Collections.Generic;

    public class TransformSystem : ISystem
    {
        private readonly List<GameObject> objects = new();

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
        }

        public void Unregister(GameObject gameObject)
        {
            objects.Remove(gameObject);
        }

        public void Update(ThreadDispatcher dispatcher)
        {
            dispatcher.DispatchWorkers(i =>
            {
                objects[i].Transform.Recalculate();
            }, objects.Count);
        }
    }
}