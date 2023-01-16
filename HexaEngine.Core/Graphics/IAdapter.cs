namespace HexaEngine.Core.Graphics
{
    public interface IAdapter
    {
        RenderBackend Backend { get; }

        IGraphicsDevice CreateGraphics(bool debug);
    }
}