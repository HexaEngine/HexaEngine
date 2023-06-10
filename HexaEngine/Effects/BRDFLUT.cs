#nullable disable

namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using System.Threading.Tasks;

    public class BRDFLUT : IEffect
    {
        private Quad quad;
        private IGraphicsPipeline pipeline;

        public IRenderTargetView Target;
        private bool disposedValue;

        public void Draw(IGraphicsContext context)
        {
            if (Target == null)
            {
                return;
            }

            context.ClearRenderTargetView(Target, default);
            context.SetRenderTarget(Target, null);
            context.SetViewport(Target.Viewport);
            quad.DrawAuto(context, pipeline);
        }

        public void BeginResize()
        {
        }

        public void EndResize(int width, int height)
        {
        }

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            quad = new Quad(device);
            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/brdf/vs.hlsl",
                PixelShader = "effects/brdf/brdf.hlsl"
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                quad.Dispose();
                pipeline.Dispose();
                Target = null;
                quad = null;
                pipeline = null;
                disposedValue = true;
            }
        }

        ~BRDFLUT()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}