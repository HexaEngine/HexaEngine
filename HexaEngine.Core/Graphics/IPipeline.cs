namespace HexaEngine.Core.Graphics
{
    public interface IPipeline : IDisposable
    {
        event Action<IPipeline>? OnCompile;
    }
}