#nullable disable

namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;

    public class SSAO : IEffect
    {
        private IGraphicsPipeline ssao;

        public Task Initialize(IGraphicsDevice device, int width, int height)
        {
            throw new NotImplementedException();
        }

        public void BeginResize()
        {
            throw new NotImplementedException();
        }

        public void EndResize(int width, int height)
        {
            throw new NotImplementedException();
        }

        public void Draw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}