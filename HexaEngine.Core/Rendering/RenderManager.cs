namespace HexaEngine.Core.Rendering
{
    using HexaEngine.Core.Graphics;

    public class RenderManager
    {
        public readonly RenderQueue RenderQueue = new();

        public RenderManager(IGraphicsDevice device)
        {
        }

        public unsafe void Register(IRenderer renderer)
        {
        }

        public unsafe void Draw(IGraphicsContext context)
        {
        }
    }
}