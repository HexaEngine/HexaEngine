namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Rendering;

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