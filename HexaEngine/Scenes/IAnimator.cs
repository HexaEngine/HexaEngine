using HexaEngine.Core.IO.Binary.Animations;

namespace HexaEngine.Scenes
{
    public interface IAnimator : IComponent
    {
        void Play(AnimationClip animation);

        void Stop();

        void Update(Scene scene, float deltaTime);
    }
}