namespace HexaEngine.Graphics.Effects.Noise
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public class Noise : IDisposable
    {
        private readonly IGraphicsPipeline pipeline;
        private readonly ConstantBuffer<NoiseParams> scaleBuffer;
        private bool disposedValue;

        private struct NoiseParams
        {
            public Vector4 Scale;
            public Vector4 Offset;
            public Vector4 Period;
            public Vector4 Rotation;

            public NoiseParams()
            {
                Scale = Vector4.One;
                Offset = Vector4.Zero;
                Period = Vector4.Zero;
                Rotation = Vector4.Zero;
            }

            public NoiseParams(Vector4 scale)
            {
                Scale = scale;
                Offset = Vector4.Zero;
                Period = Vector4.Zero;
                Rotation = Vector4.Zero;
            }

            public NoiseParams(Vector4 scale, Vector4 offset)
            {
                Scale = scale;
                Offset = offset;
                Period = Vector4.Zero;
                Rotation = Vector4.Zero;
            }

            public NoiseParams(Vector4 scale, Vector4 offset, Vector4 period)
            {
                Scale = scale;
                Offset = offset;
                Period = period;
                Rotation = Vector4.Zero;
            }

            public NoiseParams(Vector4 scale, Vector4 offset, Vector4 period, Vector4 rotation)
            {
                Scale = scale;
                Offset = offset;
                Period = period;
                Rotation = rotation;
            }
        }

        public Noise(IGraphicsDevice device, NoiseType type)
        {
            ShaderMacro[] macros = [new ShaderMacro(type.ToString(), 1)];

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = $"effects/noise/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
                Macros = macros
            });

            scaleBuffer = new(device, new NoiseParams(), CpuAccessFlags.Write);
        }

        public void Draw(IGraphicsContext context, IRenderTargetView rtv, Viewport viewport)
        {
            scaleBuffer.Update(context, new NoiseParams());
            context.SetRenderTarget(rtv, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.PSSetConstantBuffer(0, scaleBuffer);
            context.DrawInstanced(4, 1, 0, 0);
            context.PSSetConstantBuffer(0, null);
            context.SetRenderTarget(null, null);
            context.SetViewport(default);
            context.SetGraphicsPipeline(null);
        }

        public void Draw1D(IGraphicsContext context, IRenderTargetView rtv, Viewport viewport, float scale)
        {
            scaleBuffer.Update(context, new NoiseParams(new(scale, 1, 1, 1)));
            context.SetRenderTarget(rtv, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.PSSetConstantBuffer(0, scaleBuffer);
            context.DrawInstanced(4, 1, 0, 0);
            context.PSSetConstantBuffer(0, null);
            context.SetRenderTarget(null, null);
            context.SetViewport(default);
            context.SetGraphicsPipeline(null);
        }

        public void Draw1D(IGraphicsContext context, IRenderTargetView rtv, Viewport viewport, float scale, float offset)
        {
            scaleBuffer.Update(context, new NoiseParams(new(scale, 1, 1, 1), new(offset, 0, 0, 0)));
            context.SetRenderTarget(rtv, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.PSSetConstantBuffer(0, scaleBuffer);
            context.DrawInstanced(4, 1, 0, 0);
            context.PSSetConstantBuffer(0, null);
            context.SetRenderTarget(null, null);
            context.SetViewport(default);
            context.SetGraphicsPipeline(null);
        }

        public void Draw2D(IGraphicsContext context, IRenderTargetView rtv, Viewport viewport, Vector2 scale)
        {
            scaleBuffer.Update(context, new NoiseParams(new(scale, 1, 1)));
            context.SetRenderTarget(rtv, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.PSSetConstantBuffer(0, scaleBuffer);
            context.DrawInstanced(4, 1, 0, 0);
            context.PSSetConstantBuffer(0, null);
            context.SetRenderTarget(null, null);
            context.SetViewport(default);
            context.SetGraphicsPipeline(null);
        }

        public void Draw2D(IGraphicsContext context, IRenderTargetView rtv, Viewport viewport, Vector2 scale, Vector2 offset)
        {
            scaleBuffer.Update(context, new NoiseParams(new(scale, 1, 1), new(offset, 0, 0)));
            context.SetRenderTarget(rtv, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.PSSetConstantBuffer(0, scaleBuffer);
            context.DrawInstanced(4, 1, 0, 0);
            context.PSSetConstantBuffer(0, null);
            context.SetRenderTarget(null, null);
            context.SetViewport(default);
            context.SetGraphicsPipeline(null);
        }

        public void Draw2D(IGraphicsContext context, IRenderTargetView rtv, Viewport viewport, Vector2 scale, Vector2 offset, Vector2 period)
        {
            scaleBuffer.Update(context, new NoiseParams(new(scale, 1, 1), new(offset, 0, 0), new(period, 0, 0)));
            context.SetRenderTarget(rtv, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.PSSetConstantBuffer(0, scaleBuffer);
            context.DrawInstanced(4, 1, 0, 0);
            context.PSSetConstantBuffer(0, null);
            context.SetRenderTarget(null, null);
            context.SetViewport(default);
            context.SetGraphicsPipeline(null);
        }

        public void Draw2D(IGraphicsContext context, IRenderTargetView rtv, Viewport viewport, Vector2 scale, Vector2 offset, Vector2 period, float rotation)
        {
            scaleBuffer.Update(context, new NoiseParams(new(scale, 1, 1), new(offset, 0, 0), new(period, 0, 0), new(rotation, 0, 0, 0)));
            context.SetRenderTarget(rtv, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.PSSetConstantBuffer(0, scaleBuffer);
            context.DrawInstanced(4, 1, 0, 0);
            context.PSSetConstantBuffer(0, null);
            context.SetRenderTarget(null, null);
            context.SetViewport(default);
            context.SetGraphicsPipeline(null);
        }

        public void Draw3D(IGraphicsContext context, IRenderTargetView rtv, Viewport viewport, Vector3 scale)
        {
            scaleBuffer.Update(context, new NoiseParams(new(scale, 1)));
            context.SetRenderTarget(rtv, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.PSSetConstantBuffer(0, scaleBuffer);
            context.DrawInstanced(4, 1, 0, 0);
            context.PSSetConstantBuffer(0, null);
            context.SetRenderTarget(null, null);
            context.SetViewport(default);
            context.SetGraphicsPipeline(null);
        }

        public void Draw3D(IGraphicsContext context, IRenderTargetView rtv, Viewport viewport, Vector3 scale, Vector3 offset)
        {
            scaleBuffer.Update(context, new NoiseParams(new(scale, 1), new(offset, 0)));
            context.SetRenderTarget(rtv, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.PSSetConstantBuffer(0, scaleBuffer);
            context.DrawInstanced(4, 1, 0, 0);
            context.PSSetConstantBuffer(0, null);
            context.SetRenderTarget(null, null);
            context.SetViewport(default);
            context.SetGraphicsPipeline(null);
        }

        public void Draw4D(IGraphicsContext context, IRenderTargetView rtv, Viewport viewport, Vector4 scale)
        {
            scaleBuffer.Update(context, new NoiseParams(scale));
            context.SetRenderTarget(rtv, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.PSSetConstantBuffer(0, scaleBuffer);
            context.DrawInstanced(4, 1, 0, 0);
            context.PSSetConstantBuffer(0, null);
            context.SetRenderTarget(null, null);
            context.SetViewport(default);
            context.SetGraphicsPipeline(null);
        }

        public void Draw4D(IGraphicsContext context, IRenderTargetView rtv, Viewport viewport, Vector4 scale, Vector4 offset)
        {
            scaleBuffer.Update(context, new NoiseParams(scale, offset));
            context.SetRenderTarget(rtv, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.PSSetConstantBuffer(0, scaleBuffer);
            context.DrawInstanced(4, 1, 0, 0);
            context.PSSetConstantBuffer(0, null);
            context.SetRenderTarget(null, null);
            context.SetViewport(default);
            context.SetGraphicsPipeline(null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline.Dispose();
                scaleBuffer.Dispose();
                disposedValue = true;
            }
        }

        ~Noise()
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