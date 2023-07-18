using HexaEngine.Core.Scenes;

namespace HexaEngine.Scenes.Systems
{
    using HexaEngine.Scenes;
    using System.Collections.Generic;

    public class AnimationSystem : ISystem
    {
        private readonly List<IAnimator> animations = new();
        private readonly Scene scene;

        public string Name => "Animations";

        public SystemFlags Flags => SystemFlags.Update;

        public AnimationSystem(Scene scene)
        {
            this.scene = scene;
        }

        public void Awake()
        {
        }

        public void Destroy()
        {
        }

        public void FixedUpdate()
        {
        }

        public void Update(float delta)
        {
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