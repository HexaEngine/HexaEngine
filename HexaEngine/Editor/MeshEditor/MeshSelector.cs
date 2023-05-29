namespace HexaEngine.Editor.MeshEditor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public class MeshSelector
    {
        private readonly IGraphicsDevice device;
        private readonly ConstantBuffer<CBCamera> camera;
        private readonly IGraphicsPipeline pipeline;
        private Texture texture;
        private readonly IComputePipeline computePipeline;
        private readonly ConstantBuffer<Vector4> mouseBuffer;
        private readonly StructuredUavBuffer<SelectionResult> outputBuffer;

        public MeshSelector(IGraphicsDevice device, ConstantBuffer<CBCamera> camera, int width, int height)
        {
            this.device = device;
            this.camera = camera;
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "forward/selection2/vs.hlsl",
                PixelShader = "forward/selection2/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList,
            });
            texture = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R32G32B32A32Float), DepthStencilDesc.Default);

            computePipeline = device.CreateComputePipeline(new()
            {
                Path = "compute/selection2/shader.hlsl",
            });
            mouseBuffer = new(device, CpuAccessFlags.Write);
            outputBuffer = new(device, 1, true, true);
        }

        public void Resize(int width, int height)
        {
            texture.Dispose();
            texture = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R32G32B32A32Float), DepthStencilDesc.Default);
        }

        public unsafe SelectionResult Select(IGraphicsContext context, MeshSource source, Vector2 position, Mathematics.Viewport viewport)
        {
            mouseBuffer[0] = new(position, viewport.Width, viewport.Height);
            mouseBuffer.Update(context);

            context.ClearDepthStencilView(texture.DepthStencilView, DepthStencilClearFlags.All, 1, 0);
            context.ClearRenderTargetView(texture.RenderTargetView, default);
            context.SetRenderTarget(texture.RenderTargetView, texture.DepthStencilView);
            context.SetViewport(viewport);
            context.SetGraphicsPipeline(pipeline);
            context.VSSetConstantBuffer(camera, 1);
            context.PSSetConstantBuffer(mouseBuffer.Buffer, 0);

            source.Bind(context);
            context.DrawIndexedInstanced(source.IndexCount, 1, 0, 0, 0);

            context.ClearState();
            context.CSSetShaderResource(texture.ShaderResourceView, 0);
            context.CSSetConstantBuffer(mouseBuffer.Buffer, 0);
            void* uav = (void*)outputBuffer.UAV.NativePointer;
            context.CSSetUnorderedAccessViews(&uav, 1);
            computePipeline.Dispatch(context, 1, 1, 1);
            outputBuffer.Read(context);

            return outputBuffer[0];
        }

        public void Release()
        {
            pipeline.Dispose();
            texture.Dispose();
            computePipeline.Dispose();
            mouseBuffer.Dispose();
            outputBuffer.Dispose();
        }
    }
}