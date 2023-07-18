using HexaEngine.Core.Scenes;

namespace HexaEngine.Scenes
{
    public interface ISceneUpdateCallbacks
    {
        void Update(GameObject root);

        void FixedUpdate(GameObject root);
    }
}