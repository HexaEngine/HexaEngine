namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Meshes;
    using System.Numerics;

    public class SpriteRenderer : IDisposable
    {
        private readonly IGraphicsPipelineState pipeline;
        private readonly ConstantBuffer<Constants> constantBuffer;

        private struct Constants
        {
            public Vector2 ScreenSize;
            public Vector2 AltasSize;
            public Matrix4x4 World;
        }

        public SpriteRenderer(IGraphicsDevice device)
        {
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "deferred/sprite/vs.hlsl",
                PixelShader = "deferred/sprite/ps.hlsl",
            }, new()
            {
                Topology = PrimitiveTopology.TriangleStrip
            });
            constantBuffer = new(CpuAccessFlags.Write);
            pipeline.Bindings.SetCBV("constants", constantBuffer);
        }

        public void Draw(IGraphicsContext context, SpriteBatch batch, SpriteAtlas atlas, Matrix4x4 transform)
        {
            if (!pipeline.IsValid || !pipeline.IsInitialized)
            {
                return;
            }
            batch.Update(context);
            Constants constants = default;
            constants.AltasSize = atlas.Size;
            constants.ScreenSize = Application.MainWindow.RenderViewport.Size;
            constants.World = Matrix4x4.Transpose(transform);

            constantBuffer.Update(context, constants);

            context.SetGraphicsPipelineState(pipeline);
            pipeline.Bindings.SetSRV("spritebuffer", batch.Buffer.SRV);
            pipeline.Bindings.SetSRV("atlasTex", atlas.SRV);
            pipeline.Bindings.SetSampler("samplerState", atlas.SamplerState);

            context.DrawInstanced(4, batch.Count, 0, 0);

            context.SetGraphicsPipelineState(null);
        }

        public void Dispose()
        {
            pipeline.Dispose();
            constantBuffer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}