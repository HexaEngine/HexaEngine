namespace HexaEngine.Core.Graphics
{
    public interface ICommandBuffer : IGraphicsContext
    {
        void Begin();

        void End();
    }
}