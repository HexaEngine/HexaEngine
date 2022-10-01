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

    public class BasicDrawingInstanced
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
        private bool running = true;

        public void Setup(SdlWindow window)
        {
            if (!OperatingSystem.IsWindows())
            {
                Debug.Fail("Only windows is supported");
                return;
            }
            this.window = window;
            window.Closing += (s, e) => { running = false; };
            device = new D3D11GraphicsDevice(window);
            context = device.Context;
            swapChain = device.SwapChain ?? throw new Exception();
            renderer = new(window, device) { NoInternal = true };
        }

        public void DrawSphere()
        {
            Pipeline pipeline = new(device, new()
            {
                VertexShader = "forward/instanced/vs.hlsl",
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
                Topology = PrimitiveTopology.TriangleList
            });
            UVSphere.CreateSphere(out Vertex[] vertices, out int[] indices);
            IBuffer vb = device.CreateBuffer(vertices, BindFlags.VertexBuffer);
            IBuffer ib = device.CreateBuffer(indices, BindFlags.IndexBuffer);
            IBuffer isb = device.CreateBuffer(new Matrix4x4[8000], BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            CameraTransform transform = new() { Position = new Vector3(0, 0, -20) };
            IBuffer cbCam = device.CreateBuffer(new CBCamera(transform), BindFlags.ConstantBuffer);
            IBuffer cbWorld = device.CreateBuffer(new CBWorld(Matrix4x4.Identity), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            Time.Initialize();
            Stopwatch sw = Stopwatch.StartNew();
            List<float> fpss = new();
            Matrix4x4[] matrices = new Matrix4x4[8000];

            while (running)
            {
                renderer.BeginDraw();
                Time.FrameUpdate();

                context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);
                context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.All, 1, 0);
                context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
                context.SetConstantBuffer(cbWorld, ShaderStage.Vertex, 0);
                context.SetConstantBuffer(cbCam, ShaderStage.Vertex, 1);
                context.SetConstantBuffer(cbCam, ShaderStage.Pixel, 1);
                context.SetVertexBuffer(0, vb, Marshal.SizeOf<Vertex>());
                context.SetVertexBuffer(1, isb, Marshal.SizeOf<Matrix4x4>());
                context.SetIndexBuffer(ib, Format.R32UInt, 0);

                int y = 0;

                for (int i = -10; i < 10; i++)
                {
                    for (int j = -10; j < 10; j++)
                    {
                        for (int k = -10; k < 10; k++)
                        {
                            matrices[y++] = Matrix4x4.CreateTranslation(i * 8, j * 8, k * 8) * Matrix4x4.CreateFromYawPitchRoll(Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16);
                        }
                    }
                }

                context.Write(isb, matrices);

                context.Write(cbWorld, new CBWorld(Matrix4x4.Identity));
                pipeline.DrawIndexedInstanced(context, swapChain.Viewport, indices.Length, matrices.Length, 0, 0, 0);

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

        public void DrawSphereFrustumCulling()
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

            CameraTransform transform = new() { Position = new Vector3(0, 0, -20) };
            IBuffer cbCam = device.CreateBuffer(new CBCamera(transform), BindFlags.ConstantBuffer);
            IBuffer cbWorld = device.CreateBuffer(new CBWorld(Matrix4x4.Identity), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            Time.Initialize();
            Stopwatch sw = Stopwatch.StartNew();
            List<float> fpss = new();
            while (Time.CumulativeFrameTime < 10)
            {
                renderer.BeginDraw();
                Time.FrameUpdate();

                context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);
                context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.All, 1, 0);
                context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
                context.SetConstantBuffer(cbWorld, ShaderStage.Vertex, 0);
                context.SetConstantBuffer(cbCam, ShaderStage.Vertex, 1);
                context.SetConstantBuffer(cbCam, ShaderStage.Pixel, 1);
                int x = 0;
                int y = 0;
                for (int i = -10; i < 10; i++)
                {
                    for (int j = -10; j < 10; j++)
                    {
                        for (int k = -10; k < 10; k++)
                        {
                            var trans = Matrix4x4.CreateTranslation(i * 8, j * 8, k * 8) * Matrix4x4.CreateFromYawPitchRoll(Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16);
                            var box = BoundingBox.Transform(sphere.BoundingBox, trans);
                            if (transform.Frustum.Intersects(box))
                            {
                                context.Write(cbWorld, new CBWorld(trans));
                                sphere.DrawAuto(context, pipeline, swapChain.Viewport);
                                x++;
                            }
                            y++;
                        }
                    }
                }

                float fps = (float)(1000 / sw.Elapsed.TotalMilliseconds);
                sw.Restart();
                fpss.Add(fps);
                ImGui.Text($"{fps}fps");

                ImGui.Text($"{x} from {y}");

                renderer.EndDraw();
                swapChain.Present(0);
            }

            avgfpsFrustumCull = fpss.Average();
            minfpsFrustumCull = fpss.Min();
            maxfpsFrustumCull = fpss.Max();
            Debug.WriteLine($"min:{minfpsFrustumCull}, max:{maxfpsFrustumCull}, avg:{avgfpsFrustumCull}");
        }

        public void DrawSphereFrustumCullingMulti()
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

            CameraTransform transform = new() { Position = new Vector3(0, 0, -20) };
            IBuffer cbCam = device.CreateBuffer(new CBCamera(transform), BindFlags.ConstantBuffer);
            IBuffer cbWorld = device.CreateBuffer(new CBWorld(Matrix4x4.Identity), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            Time.Initialize();
            Stopwatch sw = Stopwatch.StartNew();
            List<float> fpss = new();
            ConcurrentQueue<int> drawlist = new();
            Matrix4x4[] matrices = new Matrix4x4[8000];
            while (Time.CumulativeFrameTime < 10)
            {
                renderer.BeginDraw();
                Time.FrameUpdate();

                context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);
                context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.All, 1, 0);
                context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
                context.SetConstantBuffer(cbWorld, ShaderStage.Vertex, 0);
                context.SetConstantBuffer(cbCam, ShaderStage.Vertex, 1);
                context.SetConstantBuffer(cbCam, ShaderStage.Pixel, 1);

                int y = 0;

                for (int i = -10; i < 10; i++)
                {
                    for (int j = -10; j < 10; j++)
                    {
                        for (int k = -10; k < 10; k++)
                        {
                            matrices[y++] = Matrix4x4.CreateTranslation(i * 8, j * 8, k * 8) * Matrix4x4.CreateFromYawPitchRoll(Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16, Time.CumulativeFrameTime.ToRad() * 16);
                        }
                    }
                }

                Parallel.For(0, matrices.Length, i =>
                {
                    var box = BoundingBox.Transform(sphere.BoundingBox, matrices[i]);
                    if (transform.Frustum.Intersects(box))
                    {
                        drawlist.Enqueue(i);
                    }
                });

                int x = drawlist.Count;
                while (drawlist.TryDequeue(out int i))
                {
                    context.Write(cbWorld, new CBWorld(matrices[i]));
                    sphere.DrawAuto(context, pipeline, swapChain.Viewport);
                }

                float fps = (float)(1000 / sw.Elapsed.TotalMilliseconds);
                sw.Restart();
                fpss.Add(fps);
                ImGui.Text($"{fps}fps");

                ImGui.Text($"{x} from {y}");

                renderer.EndDraw();
                swapChain.Present(0);
            }

            avgfpsFrustumCull = fpss.Average();
            minfpsFrustumCull = fpss.Min();
            maxfpsFrustumCull = fpss.Max();
            Debug.WriteLine($"min:{minfpsFrustumCull}, max:{maxfpsFrustumCull}, avg:{avgfpsFrustumCull}");
        }

        public void Teardown()
        {
            float frustumDeltaBaseline = avgfpsFrustumCull / avgfps * 100;

            Debug.WriteLine($"Baseline: min:{minfps}, max:{maxfps}, avg:{avgfps}");
            Debug.WriteLine($"Frustum Cull: min:{minfpsFrustumCull}, max:{maxfpsFrustumCull}, avg:{avgfpsFrustumCull}, compare baseline:{frustumDeltaBaseline}%");
            device.Dispose();
            window.Close();
        }
    }
}