namespace HexaEngine.Rendering
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Resources;
    using System.Numerics;

    public class SpriteRenderer : IDisposable
    {
        private readonly IGraphicsPipeline pipeline;
        private readonly ConstantBuffer<Constants> constantBuffer;

        private readonly ResourceRef<IBuffer> camera;

        private struct Constants
        {
            public Vector2 ScreenSize;
            public Vector2 AltasSize;
            public Matrix4x4 World;
        }

        public SpriteRenderer(IGraphicsDevice device)
        {
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "deferred/sprite/vs.hlsl",
                PixelShader = "deferred/sprite/ps.hlsl"
            }, new GraphicsPipelineState()
            {
                Topology = PrimitiveTopology.TriangleStrip
            });
            constantBuffer = new(device, CpuAccessFlags.Write);
            camera = ResourceManager2.Shared.GetBuffer("CBCamera");
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

            context.SetGraphicsPipeline(pipeline);
            context.VSSetShaderResource(0, batch.Buffer.SRV);
            context.VSSetConstantBuffer(0, constantBuffer);
            context.VSSetConstantBuffer(1, camera.Value);

            context.PSSetShaderResource(0, atlas.SRV);
            context.PSSetSampler(0, atlas.SamplerState);

            context.DrawInstanced(4, batch.Count, 0, 0);

            context.PSSetSampler(0, null);
            context.PSSetShaderResource(0, null);

            context.VSSetConstantBuffer(1, null);
            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetConstantBuffer(1, null);
            context.SetGraphicsPipeline(null);
        }

        public void Dispose()
        {
            pipeline.Dispose();
            constantBuffer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}