namespace HexaEngine.Core.Scenes
{
    public interface ISceneUpdateCallbacks
    {
        void Update(GameObject root);

        void FixedUpdate(GameObject root);
    }
}