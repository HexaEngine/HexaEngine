namespace HexaEngine.Core.Animations
{
    using HexaEngine.Core.Scenes;

    public interface IAnimator : IComponent
    {
        void Play(Animation animation);

        void Stop();

        void Update(Scene scene, float deltaTime);
    }
}