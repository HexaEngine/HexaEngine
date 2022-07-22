namespace HexaEngine.Tests
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.D3D11;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Rendering.ConstantBuffers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    public class BasicDrawing
    {
        private SdlWindow window;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private ISwapChain swapChain;

        [SetUp]
        public void Setup()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Fail("Only windows is supported");
                return;
            }
            window = new();
            window.Show();
            device = new D3D11GraphicsDevice(window);
            context = device.Context;
            swapChain = device.SwapChain ?? throw new Exception();
        }

        [Test]
        public void DrawSphere()
        {
            Pipeline pipeline = new(device, new()
            {
                VertexShader = "forward/basic/vs.hlsl",
                PixelShader = "forward/basic/ps.hlsl",
            }, new()
            {
                Blend = BlendDescription.Opaque,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList
            });
            UVSphere sphere = new(device);

            CameraTransform transform = new() { Position = new Vector3(0, 0, -10) };
            IBuffer cbCam = device.CreateBuffer(new CBCamera(transform), BindFlags.ConstantBuffer);
            IBuffer cbWorld = device.CreateBuffer(new CBWorld(Matrix4x4.Identity), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            Time.Initialize();
            while (Time.CumulativeFrameTime < 5)
            {
                Time.FrameUpdate();
                var transform1 = Matrix4x4.CreateTranslation(1 * MathF.Cos((Time.CumulativeFrameTime * 80).ToRad()), 1 * MathF.Sin((Time.CumulativeFrameTime * 80).ToRad()), 0) * Matrix4x4.CreateRotationX((Time.CumulativeFrameTime * 80).ToRad());
                context.Write(cbWorld, new CBWorld(Matrix4x4.Transpose(transform1)));
                context.SetConstantBuffer(cbWorld, ShaderStage.Vertex, 0);
                context.SetConstantBuffer(cbCam, ShaderStage.Vertex, 1);
                context.SetConstantBuffer(cbCam, ShaderStage.Pixel, 1);
                context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);
                context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.All, 1, 0);
                context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
                sphere.DrawAuto(context, pipeline, swapChain.Viewport);
                swapChain.Present(1);
            }
        }

        [TearDown]
        public void Teardown()
        {
            device.Dispose();
            window.Close();
        }
    }
}