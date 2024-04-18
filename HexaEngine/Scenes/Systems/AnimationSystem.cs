namespace HexaEngine.Scenes.Systems
{
    using HexaEngine.Core;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;

    public class AnimationSystem : ISceneSystem
    {
        private readonly ComponentTypeQuery<IAnimator> animators = new();
        private readonly Scene scene;

        public string Name => "Animations";

        public SystemFlags Flags => SystemFlags.Awake | SystemFlags.Destroy | SystemFlags.Update;

        public AnimationSystem(Scene scene)
        {
            this.scene = scene;
        }

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(animators);
        }

        public void Destroy()
        {
            animators.Dispose();
        }

        public void Update(float delta)
        {
            if (Application.InEditMode) return;

            for (int i = 0; i < animators.Count; i++)
            {
                animators[i].Update(scene, delta);
            }
        }
    }
}