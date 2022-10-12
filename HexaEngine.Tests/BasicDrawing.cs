namespace HexaEngine.Tests
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.D3D11;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.ConstantBuffers;
    using ImGuiNET;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    public class BasicDrawing
    {
        private SdlWindow window;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private ISwapChain swapChain;
        private ImGuiRenderer renderer;
        private float avgfps;
        private float minfps;
        private float maxfps;
        private float avgfpsFrustumCull;
        private float minfpsFrustumCull;
        private float maxfpsFrustumCull;
        private float avgfpsFrustumCullMulti;
        private float minfpsFrustumCullMulti;
        private float maxfpsFrustumCullMulti;
        private float avgfpsInstanced;
        private float minfpsInstanced;
        private float maxfpsInstanced;

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
            device = new D3D11GraphicsDevice(new DXGIAdapter(), window);
            context = device.Context;
            swapChain = device.SwapChain ?? throw new Exception();
            renderer = new(window, device);
        }

        [Test]
        public void DrawSphere()
        {
            Pipeline pipeline = new(device, new()
            {
                VertexShader = "forward/basic/vs.hlsl",
                PixelShader = "forward/basic/ps.hlsl",
            }, new PipelineState()
            {
                Blend = BlendDescription.Opaque,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList
            });
            UVSphere sphere = new(device);

            CameraTransform transform = new() { Position = new Vector3(0, 0, 0) };
            IBuffer cbCam = device.CreateBuffer(new CBCamera(transform), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            IBuffer cbWorld = device.CreateBuffer(new CBWorld(Matrix4x4.Identity), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            Time.Initialize();
            Stopwatch sw = Stopwatch.StartNew();
            List<float> fpss = new();
            Matrix4x4[] matrices = new Matrix4x4[64000];
            int x = 0;
            for (int i = -20; i < 20; i++)
            {
                for (int j = -20; j < 20; j++)
                {
                    for (int k = -20; k < 20; k++)
                    {
                        matrices[x++] = Matrix4x4.CreateTranslation(i * 8, j * 8, k * 8);
                    }
                }
            }
            while (Time.CumulativeFrameTime < 10)
            {
                renderer.BeginDraw();
                Time.FrameUpdate();
                transform.Orientation = Quaternion.CreateFromYawPitchRoll(Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16);
                context.Write(cbCam, new CBCamera(transform));
                context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);
                context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.All, 1, 0);
                context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
                context.SetConstantBuffer(cbWorld, ShaderStage.Vertex, 0);
                context.SetConstantBuffer(cbCam, ShaderStage.Vertex, 1);
                context.SetConstantBuffer(cbCam, ShaderStage.Pixel, 1);
                for (int i = 0; i < matrices.Length; i++)
                {
                    context.Write(cbWorld, new CBWorld(matrices[i]));
                    sphere.DrawAuto(context, pipeline, swapChain.Viewport);
                }

                float fps = (float)(1000 / sw.Elapsed.TotalMilliseconds);
                sw.Restart();
                fpss.Add(fps);
                ImGui.Text($"{fps}fps");

                renderer.EndDraw();
                swapChain.Present(0);
            }

            avgfps = fpss.Average();
            minfps = fpss.Min();
            maxfps = fpss.Max();
            Debug.WriteLine($"min:{minfps}, max:{maxfps}, avg:{avgfps}");
        }

        [Test]
        public void DrawSphereFrustumCulling()
        {
            Pipeline pipeline = new(device, new()
            {
                VertexShader = "forward/basic/vs.hlsl",
                PixelShader = "forward/basic/ps.hlsl",
            }, new PipelineState()
            {
                Blend = BlendDescription.Opaque,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList
            });
            UVSphere sphere = new(device);

            CameraTransform transform = new() { Position = new Vector3(0, 0, 0) };
            IBuffer cbCam = device.CreateBuffer(new CBCamera(transform), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            IBuffer cbWorld = device.CreateBuffer(new CBWorld(Matrix4x4.Identity), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            Time.Initialize();
            Stopwatch sw = Stopwatch.StartNew();
            List<float> fpss = new();
            Matrix4x4[] matrices = new Matrix4x4[64000];
            int x = 0;
            for (int i = -20; i < 20; i++)
            {
                for (int j = -20; j < 20; j++)
                {
                    for (int k = -20; k < 20; k++)
                    {
                        matrices[x++] = Matrix4x4.CreateTranslation(i * 8, j * 8, k * 8);
                    }
                }
            }
            while (Time.CumulativeFrameTime < 10)
            {
                renderer.BeginDraw();
                Time.FrameUpdate();

                transform.Orientation = Quaternion.CreateFromYawPitchRoll(Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16);
                context.Write(cbCam, new CBCamera(transform));
                context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);
                context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.All, 1, 0);
                context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
                context.SetConstantBuffer(cbWorld, ShaderStage.Vertex, 0);
                context.SetConstantBuffer(cbCam, ShaderStage.Vertex, 1);
                context.SetConstantBuffer(cbCam, ShaderStage.Pixel, 1);

                int y = 0;
                for (int i = 0; i < matrices.Length; i++)
                {
                    var trans = matrices[i];
                    var box = BoundingBox.Transform(sphere.BoundingBox, trans);
                    if (transform.Frustum.Intersects(box))
                    {
                        context.Write(cbWorld, new CBWorld(trans));
                        sphere.DrawAuto(context, pipeline, swapChain.Viewport);
                        y++;
                    }
                }

                float fps = (float)(1000 / sw.Elapsed.TotalMilliseconds);
                sw.Restart();
                fpss.Add(fps);
                ImGui.Text($"{fps}fps");

                ImGui.Text($"{y} from {matrices.Length}");

                renderer.EndDraw();
                swapChain.Present(0);
            }

            avgfpsFrustumCull = fpss.Average();
            minfpsFrustumCull = fpss.Min();
            maxfpsFrustumCull = fpss.Max();
            Debug.WriteLine($"min:{minfpsFrustumCull}, max:{maxfpsFrustumCull}, avg:{avgfpsFrustumCull}");
        }

        [Test]
        public void DrawSphereFrustumCullingMulti()
        {
            Pipeline pipeline = new(device, new()
            {
                VertexShader = "forward/basic/vs.hlsl",
                PixelShader = "forward/basic/ps.hlsl",
            }, new PipelineState()
            {
                Blend = BlendDescription.Opaque,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList
            });
            UVSphere sphere = new(device);

            CameraTransform transform = new() { Position = new Vector3(0, 0, 0) };
            IBuffer cbCam = device.CreateBuffer(new CBCamera(transform), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            IBuffer cbWorld = device.CreateBuffer(new CBWorld(Matrix4x4.Identity), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            Time.Initialize();
            Stopwatch sw = Stopwatch.StartNew();
            List<float> fpss = new();
            ConcurrentQueue<int> drawlist = new();
            Matrix4x4[] matrices = new Matrix4x4[64000];
            int x = 0;
            for (int i = -20; i < 20; i++)
            {
                for (int j = -20; j < 20; j++)
                {
                    for (int k = -20; k < 20; k++)
                    {
                        matrices[x++] = Matrix4x4.CreateTranslation(i * 8, j * 8, k * 8);
                    }
                }
            }
            while (Time.CumulativeFrameTime < 10)
            {
                renderer.BeginDraw();
                Time.FrameUpdate();

                transform.Orientation = Quaternion.CreateFromYawPitchRoll(Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16);
                context.Write(cbCam, new CBCamera(transform));
                context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);
                context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.All, 1, 0);
                context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
                context.SetConstantBuffer(cbWorld, ShaderStage.Vertex, 0);
                context.SetConstantBuffer(cbCam, ShaderStage.Vertex, 1);
                context.SetConstantBuffer(cbCam, ShaderStage.Pixel, 1);

                Parallel.For(0, matrices.Length, i =>
                {
                    var box = BoundingBox.Transform(sphere.BoundingBox, matrices[i]);
                    if (transform.Frustum.Intersects(box))
                    {
                        drawlist.Enqueue(i);
                    }
                });

                int y = drawlist.Count;
                while (drawlist.TryDequeue(out int i))
                {
                    context.Write(cbWorld, new CBWorld(matrices[i]));
                    sphere.DrawAuto(context, pipeline, swapChain.Viewport);
                }

                float fps = (float)(1000 / sw.Elapsed.TotalMilliseconds);
                sw.Restart();
                fpss.Add(fps);
                ImGui.Text($"{fps}fps");

                ImGui.Text($"{y} from {matrices.Length}");

                renderer.EndDraw();
                swapChain.Present(0);
            }

            avgfpsFrustumCullMulti = fpss.Average();
            minfpsFrustumCullMulti = fpss.Min();
            maxfpsFrustumCullMulti = fpss.Max();
            Debug.WriteLine($"min:{minfpsFrustumCullMulti}, max:{maxfpsFrustumCullMulti}, avg:{avgfpsFrustumCullMulti}");
        }

        [Test]
        public void DrawSphereInstanced()
        {
            ImGuiConsole.Redirect = true;
            ShaderCache.DisableCache = true;
            Pipeline pipeline = new(device, new()
            {
                VertexShader = "forward/instanced/vs.hlsl",
                HullShader = "forward/instanced/hs.hlsl",
                DomainShader = "forward/instanced/ds.hlsl",
                PixelShader = "forward/instanced/ps.hlsl",
            }, new InputElementDescription[]
            {
                new("POSITION", 0, Format.RGB32Float, 0),
                new("TEXCOORD", 0, Format.RGB32Float, 0),
                new("NORMAL", 0, Format.RGB32Float, 0),
                new("TANGENT", 0, Format.RGB32Float, 0),
                new("INSTANCED_MATS", 0, Format.RGBA32Float, 0, 1, InputClassification.PerInstanceData, 1),
                new("INSTANCED_MATS", 1, Format.RGBA32Float, 16, 1, InputClassification.PerInstanceData, 1),
                new("INSTANCED_MATS", 2, Format.RGBA32Float, 32, 1, InputClassification.PerInstanceData, 1),
                new("INSTANCED_MATS", 3, Format.RGBA32Float, 48, 1, InputClassification.PerInstanceData, 1),
            }, new()
            {
                Blend = BlendDescription.Opaque,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints
            });
            UVSphere.CreateSphere(out Vertex[] vertices, out int[] indices);
            IBuffer vb = device.CreateBuffer(vertices, BindFlags.VertexBuffer);
            IBuffer ib = device.CreateBuffer(indices, BindFlags.IndexBuffer);
            IBuffer isb = device.CreateBuffer(new Matrix4x4[64000], BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            CameraTransform transform = new() { Position = new Vector3(0, 0, 0) };
            IBuffer cbCam = device.CreateBuffer(new CBCamera(transform), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            Time.Initialize();
            Stopwatch sw = Stopwatch.StartNew();
            List<float> fpss = new();
            Matrix4x4[] matrices = new Matrix4x4[64000];
            int y = 0;

            for (int i = -20; i < 20; i++)
            {
                for (int j = -20; j < 20; j++)
                {
                    for (int k = -20; k < 20; k++)
                    {
                        matrices[y++] = Matrix4x4.CreateTranslation(i * 8, j * 8, k * 8);
                    }
                }
            }
            context.Write(isb, matrices);
            while (Time.CumulativeFrameTime < 10)
            {
                Time.FrameUpdate();

                transform.Orientation = Quaternion.CreateFromYawPitchRoll(Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16);
                context.Write(cbCam, new CBCamera(transform));

                context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);
                context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.All, 1, 0);
                context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
                context.SetConstantBuffer(cbCam, ShaderStage.Vertex, 1);
                context.SetConstantBuffer(cbCam, ShaderStage.Domain, 1);
                context.SetConstantBuffer(cbCam, ShaderStage.Pixel, 1);
                context.SetVertexBuffer(0, vb, Marshal.SizeOf<Vertex>());
                context.SetVertexBuffer(1, isb, Marshal.SizeOf<Matrix4x4>());
                context.SetIndexBuffer(ib, Format.R32UInt, 0);

                pipeline.DrawIndexedInstanced(context, swapChain.Viewport, indices.Length, matrices.Length, 0, 0, 0);

                float fps = (float)(1000 / sw.Elapsed.TotalMilliseconds);
                sw.Restart();
                fpss.Add(fps);

                swapChain.Present(0);
            }

            avgfpsInstanced = fpss.Average();
            minfpsInstanced = fpss.Min();
            maxfpsInstanced = fpss.Max();
            Debug.WriteLine($"min:{minfpsInstanced}, max:{maxfpsInstanced}, avg:{avgfpsInstanced}");
        }

        [TearDown]
        public void Teardown()
        {
            float frustumDeltaBaseline = avgfpsFrustumCull / avgfps * 100;
            float frustumMultiDeltaBaseline = avgfpsFrustumCullMulti / avgfps * 100;
            float instancedDeltaBaseline = avgfpsInstanced / avgfps * 100;

            Debug.WriteLine($"Baseline: min:{minfps}, max:{maxfps}, avg:{avgfps}");
            Debug.WriteLine($"Frustum Cull: min:{minfpsFrustumCull}, max:{maxfpsFrustumCull}, avg:{avgfpsFrustumCull}, compare baseline:{frustumDeltaBaseline}%");
            Debug.WriteLine($"Frustum Cull Multi: min:{minfpsFrustumCullMulti}, max:{maxfpsFrustumCullMulti}, avg:{avgfpsFrustumCullMulti}, compare baseline:{frustumMultiDeltaBaseline}%");
            Debug.WriteLine($"Instanced: min:{minfpsInstanced}, max:{maxfpsInstanced}, avg:{avgfpsInstanced}, compare baseline:{instancedDeltaBaseline}%");
            device.Dispose();
            window.Close();
        }
    }

    [SetUpFixture]
    public class SetupTrace
    {
        [OneTimeSetUp]
        public void StartTest()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        [OneTimeTearDown]
        public void EndTest()
        {
            Trace.Flush();
        }
    }
}