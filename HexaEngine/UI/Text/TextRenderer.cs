namespace HexaEngine.UI.Text
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System.Numerics;

    public class TextRenderer : IDisposable
    {
        private readonly IGraphicsPipelineState pipeline;
        private readonly ConstantBuffer<TextConstants> constantsBuffer;
        private readonly ConstantBuffer<Vector4> colorBuffer;
        private bool disposedValue;

        private struct TextConstants
        {
            public Matrix4x4 World;
            public Vector2 ScreenSize;
            public Vector2 AtlasSize;
        }

        public TextRenderer(IGraphicsDevice device)
        {
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "forward/text/vs.hlsl",
                PixelShader = "forward/text/ps.hlsl"
            }, GraphicsPipelineStateDesc.Default);

            constantsBuffer = new(device, CpuAccessFlags.Write);
            colorBuffer = new(device, CpuAccessFlags.Write);
        }

        public void Draw(IGraphicsContext context, GlyphRun run, Vector4 color, FontAtlas font, Matrix4x4 transform)
        {
            if (!pipeline.IsValid || !pipeline.IsInitialized)
            {
                return;
            }

            run.VertexBuffer.Update(context);
            run.IndexBuffer.Update(context);

            TextConstants constants;
            constants.World = Matrix4x4.Transpose(transform);
            constants.ScreenSize = Application.MainWindow.RenderViewport.Size;
            constants.AtlasSize = font.AtlasSize;

            constantsBuffer.Update(context, constants);
            colorBuffer.Update(context, color);

            context.SetPipelineState(pipeline);

            context.VSSetConstantBuffer(0, constantsBuffer);

            context.PSSetConstantBuffer(0, colorBuffer);
            context.PSSetShaderResource(0, font.SRV);
            context.PSSetSampler(0, font.SamplerState);

            run.Draw(context);

            context.PSSetSampler(0, null);
            context.PSSetShaderResource(0, null);
            context.PSSetConstantBuffer(0, null);

            context.VSSetConstantBuffer(0, null);

            context.SetPipelineState(null);
        }

        public void Draw(IGraphicsContext context, GlyphRun run, Vector4 color, Matrix4x4 transform)
        {
            Draw(context, run, color, run.Font, transform);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline.Dispose();
                colorBuffer.Dispose();
                constantsBuffer.Dispose();
                disposedValue = true;
            }
        }

        ~TextRenderer()
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