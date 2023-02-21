namespace HexaEngine.Core.Graphics
{
    public interface IGraphicsAdapter
    {
        RenderBackend Backend { get; }

        IGraphicsDevice CreateGraphics(bool debug);
    }
}