using HexaEngine.Core.IO.Animations;

namespace HexaEngine.Scenes
{
    public interface IAnimator : IComponent
    {
        void Play(Animation animation);

        void Stop();

        void Update(Scene scene, float deltaTime);
    }
}