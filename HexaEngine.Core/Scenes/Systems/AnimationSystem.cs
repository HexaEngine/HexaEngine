namespace HexaEngine.Core.Scenes.Systems
{
    using BepuUtilities;
    using HexaEngine.Core.Animations;
    using System.Collections.Generic;

    public class AnimationSystem : ISystem
    {
        private readonly List<IAnimator> animations = new();
        private readonly Scene scene;

        public string Name => "Animations";

        public AnimationSystem(Scene scene)
        {
            this.scene = scene;
        }

        public void Awake(ThreadDispatcher dispatcher)
        {
        }

        public void Destroy(ThreadDispatcher dispatcher)
        {
        }

        public void FixedUpdate(ThreadDispatcher dispatcher)
        {
        }

        public void Update(ThreadDispatcher dispatcher)
        {
            var delta = Time.Delta;
            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].Update(scene, delta);
            }
        }

        public void Register(GameObject gameObject)
        {
            animations.AddComponentIfIs(gameObject);
        }

        public void Unregister(GameObject gameObject)
        {
            animations.RemoveComponentIfIs(gameObject);
        }
    }
}