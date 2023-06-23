namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public enum NoiseType
    {
        /// <summary>
        /// Blue noise 2D
        /// </summary>
        Blue2D,

        /// <summary>
        /// Cellular noise 2D, returning F1 and F2 in a float2.
        /// Standard 3x3 search window for good F1 and F2 values
        /// </summary>
        Cellular2D,

        /// <summary>
        /// Cellular noise 2D, returning F1 and F2 in a float2.
        /// Speeded up by using 2x2 search window instead of 3x3,
        /// at the expense of some strong pattern artifacts.
        /// F2 is often wrong and has sharp discontinuities.
        /// If you need a smooth F2, use the slower 3x3 version.
        /// F1 is sometimes wrong, too, but OK for most purposes.
        /// </summary>
        Cellular2x2,

        /// <summary>
        /// Cellular noise 3D, returning F1 and F2 in a float2.
        /// Speeded up by using 2x2x2 search window instead of 3x3x3,
        /// at the expense of some pattern artifacts.
        /// F2 is often wrong and has sharp discontinuities.
        /// If you need a good F2, use the slower 3x3x3 version.
        /// </summary>
        Cellular2x2x2,

        /// <summary>
        /// Cellular noise 3D, returning F1 and F2 in a float2.
        /// 3x3x3 search region for good F2 everywhere, but a lot
        /// slower than the 2x2x2 version.
        /// The code below is a bit scary even to its author,
        /// but it has at least half decent performance on a
        /// modern GPU. In any case, it beats any software
        /// implementation of Worley noise hands down.
        /// </summary>
        Cellular3D,

        /// <summary>
        /// White noise 1D
        /// </summary>
        Hash1D,

        /// <summary>
        /// White noise 2D
        /// </summary>
        Hash2D,

        /// <summary>
        /// White noise 3D
        /// </summary>
        Hash3D,

        /// <summary>
        /// Classic Perlin noise 2D
        /// </summary>
        Perlin2D,

        /// <summary>
        /// Classic Perlin noise 3D
        /// </summary>
        Perlin3D,

        /// <summary>
        /// Classic Perlin noise 4D
        /// </summary>
        Perlin4D,

        /// <summary>
        /// Simplex noise 2D
        /// </summary>
        Simplex2D,

        /// <summary>
        /// Simplex noise 3D
        /// </summary>
        Simplex3D,

        /// <summary>
        /// Simplex noise 4D
        /// </summary>
        Simplex4D,

        /// <summary>
        /// 2-D tiling simplex noise with rotating gradients and analytical derivative.
        /// The first component of the 3-element return vector is the noise value,
        /// and the second and third components are the x and y partial derivatives.
        /// </summary>
        Simplex2DPSRD,

        /// <summary>
        /// 2-D tiling simplex noise with rotating gradients,
        /// but without the analytical derivative.
        /// </summary>
        Simplex2DPSD,

        /// <summary>
        /// 2-D non-tiling simplex noise with rotating gradients and analytical derivative.
        /// The first component of the 3-element return vector is the noise value,
        /// and the second and third components are the x and y partial derivatives.
        /// </summary>
        Simplex2DSRD,

        /// <summary>
        /// 2-D non-tiling simplex noise with rotating gradients,
        /// without the analytical derivative.
        /// </summary>
        Simplex2DSR,
    }

    public class Noise : IDisposable
    {
        private readonly Quad quad;
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
            ShaderMacro[] macros = { new ShaderMacro(type.ToString(), 1) };
            quad = new(device);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/noise/vs.hlsl",
                PixelShader = $"effects/noise/ps.hlsl",
            }, macros);

            scaleBuffer = new(device, new NoiseParams(), CpuAccessFlags.Write);
        }

        public void Draw(IGraphicsContext context, IRenderTargetView rtv, Viewport viewport)
        {
            scaleBuffer.Update(context, new NoiseParams());
            context.SetRenderTarget(rtv, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.PSSetConstantBuffer(scaleBuffer, 0);
            quad.DrawAuto(context);
            context.PSSetConstantBuffer(null, 0);
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
            context.PSSetConstantBuffer(scaleBuffer, 0);
            quad.DrawAuto(context);
            context.PSSetConstantBuffer(null, 0);
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
            context.PSSetConstantBuffer(scaleBuffer, 0);
            quad.DrawAuto(context);
            context.PSSetConstantBuffer(null, 0);
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
            context.PSSetConstantBuffer(scaleBuffer, 0);
            quad.DrawAuto(context);
            context.PSSetConstantBuffer(null, 0);
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
            context.PSSetConstantBuffer(scaleBuffer, 0);
            quad.DrawAuto(context);
            context.PSSetConstantBuffer(null, 0);
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
            context.PSSetConstantBuffer(scaleBuffer, 0);
            quad.DrawAuto(context);
            context.PSSetConstantBuffer(null, 0);
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
            context.PSSetConstantBuffer(scaleBuffer, 0);
            quad.DrawAuto(context);
            context.PSSetConstantBuffer(null, 0);
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
            context.PSSetConstantBuffer(scaleBuffer, 0);
            quad.DrawAuto(context);
            context.PSSetConstantBuffer(null, 0);
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
            context.PSSetConstantBuffer(scaleBuffer, 0);
            quad.DrawAuto(context);
            context.PSSetConstantBuffer(null, 0);
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
            context.PSSetConstantBuffer(scaleBuffer, 0);
            quad.DrawAuto(context);
            context.PSSetConstantBuffer(null, 0);
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
            context.PSSetConstantBuffer(scaleBuffer, 0);
            quad.DrawAuto(context);
            context.PSSetConstantBuffer(null, 0);
            context.SetRenderTarget(null, null);
            context.SetViewport(default);
            context.SetGraphicsPipeline(null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline.Dispose();
                quad.Dispose();
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