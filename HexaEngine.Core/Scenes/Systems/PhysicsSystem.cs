namespace HexaEngine.Core.Scenes.Systems
{
    using BepuUtilities;
    using System;
    using System.Collections.Generic;

    public class PhysicsSystem : ISystem
    {
        private readonly List<IBaseCollider> colliders = new();

        public void Register(GameObject gameObject)
        {
            colliders.AddComponentIfIs(gameObject);
        }

        public void Unregister(GameObject gameObject)
        {
            colliders.RemoveComponentIfIs(gameObject);
        }

        public void Awake(ThreadDispatcher dispatcher)
        {
        }

        public void Update(ThreadDispatcher dispatcher)
        {
            dispatcher.DispatchWorkers(i => colliders[i].Update());
        }

        public void FixedUpdate(ThreadDispatcher dispatcher)
        {
        }

        public void Destroy(ThreadDispatcher dispatcher)
        {
        }
    }
}