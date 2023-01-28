namespace HexaEngine.Core.Scenes.Systems
{
    using BepuUtilities;
    using System.Collections.Generic;

    public class PhysicsSystem : ISystem
    {
        private readonly List<ICollider> colliders = new();

        public string Name => "PhysicsUpdate";

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
            if (Application.InDesignMode) return;
            for (int i = 0; i < colliders.Count; i++)
            {
                colliders[i].Update();
            }
        }

        public void Update(int i)
        {
            colliders[i].Update();
        }

        public void FixedUpdate(ThreadDispatcher dispatcher)
        {
        }

        public void Destroy(ThreadDispatcher dispatcher)
        {
        }
    }
}