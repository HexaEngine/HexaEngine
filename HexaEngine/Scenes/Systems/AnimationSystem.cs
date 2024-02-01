namespace HexaEngine.Scenes.Systems
{
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;

    public class AnimationSystem : ISystem
    {
        private readonly ComponentTypeQuery<IAnimator> animators = new();
        private readonly Scene scene;

        public string Name => "Animations";

        public SystemFlags Flags => SystemFlags.Awake | SystemFlags.Update;

        public AnimationSystem(Scene scene)
        {
            this.scene = scene;
        }

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(animators);
        }

        public void Update(float delta)
        {
            for (int i = 0; i < animators.Count; i++)
            {
                animators[i].Update(scene, delta);
            }
        }
    }
}