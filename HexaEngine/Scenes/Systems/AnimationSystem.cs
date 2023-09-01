namespace HexaEngine.Scenes.Systems
{
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;

    public class AnimationSystem : ISystem
    {
        private readonly ComponentTypeQuery<IAnimator> animations = new();
        private readonly Scene scene;

        public string Name => "Animations";

        public SystemFlags Flags => SystemFlags.Awake | SystemFlags.Update;

        public AnimationSystem(Scene scene)
        {
            this.scene = scene;
        }

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(animations);
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
    }
}