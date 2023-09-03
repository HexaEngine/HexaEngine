namespace HexaEngine.Rendering.Batching
{
    using HexaEngine.Core.Scenes;

    public interface IBatcher
    {
        bool TryBatch(GameObject gameObject, IBatchRenderer renderer);
        bool TryRemove(GameObject gameObject, IBatchRenderer renderer);
    }
}