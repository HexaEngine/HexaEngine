#nullable disable

using HexaEngine;

namespace HexaEngine.Filters
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Graphics.Structs;
    using Silk.NET.DirectStorage;
    using Silk.NET.Maths;
    using System.Threading.Tasks;

    public class BRDFLUT
    {
        private Quad quad;
        private IGraphicsPipeline pipeline;

        public IRenderTargetView Target;
        private bool disposedValue;

        public BRDFLUT(IGraphicsDevice device, bool multiscatter, bool cloth)
        {
            quad = new Quad(device);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/dfg/vs.hlsl",
                PixelShader = "effects/dfg/ps.hlsl"
            }, new ShaderMacro[2] { new("MULTISCATTER", multiscatter ? "1" : "0"), new("CLOTH", cloth ? "1" : "0") });
        }

        public void Draw(IGraphicsContext context)
        {
            if (Target == null)
            {
                return;
            }

            int width = (int)Target.Viewport.Width;
            int height = (int)Target.Viewport.Height;
            int xTileSize = width / 16;
            int yTileSize = height / 16;

            context.ClearRenderTargetView(Target, default);
            context.SetRenderTarget(Target, null);
            context.SetViewport(Target.Viewport);
            quad.DrawAuto(context, pipeline);
            /*
            for (int x = 0; x < width; x += xTileSize)
            {
                for (int y = 0; y < height; y += yTileSize)
                {
                    //context.SetScissorRect(x, y, x + xTileSize, y + yTileSize);
                    quad.DrawAuto(context, pipeline);
                    context.Flush();
                }
            }*/
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