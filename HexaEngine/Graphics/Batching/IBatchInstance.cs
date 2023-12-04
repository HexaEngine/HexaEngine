namespace HexaEngine.Graphics.Batching
{
    public interface IBatchInstance
    {
        bool CanMerge(IBatch batch, IBatchInstance other);

        bool CanInstantiate(IBatch batch, IBatchInstance other);

        IBatchInstance Merge(IBatch batch, IBatchInstance other);
    }
}