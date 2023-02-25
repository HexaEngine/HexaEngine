#nullable disable

using HexaEngine;

namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;

    public class SSAO : IEffect
    {
#pragma warning disable CS0169 // The field 'SSAO.ssao' is never used
        private IGraphicsPipeline ssao;
#pragma warning restore CS0169 // The field 'SSAO.ssao' is never used

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