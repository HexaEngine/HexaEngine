#nullable disable

namespace HexaEngine.Graphics.Filters
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;

    public class BRDFLUT
    {
        private IGraphicsPipelineState pipeline;

        public IRenderTargetView Target;
        private bool disposedValue;

        public BRDFLUT(bool multiscatter, bool cloth)
        {
            var device = Application.GraphicsDevice;
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/dfg/ps.hlsl",
                Macros = [new("MULTISCATTER", multiscatter ? "1" : "0"), new("CLOTH", cloth ? "1" : "0")]
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
        }

        public void Draw(IGraphicsContext context, uint width, uint height)
        {
            if (Target == null)
            {
                return;
            }

            context.ClearRenderTargetView(Target, default);

            context.SetRenderTarget(Target, null);
            context.SetViewport(new(width, height));

            context.SetGraphicsPipelineState(pipeline);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline.Dispose();
                Target = null;
                pipeline = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}