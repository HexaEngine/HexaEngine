namespace HexaEngine.Filters
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Graphics.Structs;
    using System;
    using System.Numerics;

    /// <summary>
    /// Pre filter for the env map used in the brfd pipeline.
    /// </summary>
    public class PreFilter : IFilter
    {
        private readonly Cube cube;
        private readonly IGraphicsPipeline pipeline;
        private readonly IBuffer mvpBuffer;
        private readonly IBuffer rghbuffer;
        private readonly ISamplerState sampler;

        public RenderTargetViewArray? Targets;
        public IShaderResourceView? Source;

        public struct CubeFaceCamera
        {
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        }

        public struct RoughnessBuffer
        {
            public float Roughness;
            public Vector3 Padd;
        }

        private CubeFaceCamera[] Cameras;

        public float Roughness;
        private bool disposedValue;

        public PreFilter(IGraphicsDevice device)
        {
            Cameras = new CubeFaceCamera[6];
            cube = new(device);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "filter/prefilter/vs.hlsl",
                PixelShader = "filter/prefilter/ps.hlsl"
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = new(CullMode.None, FillMode.Solid) { ScissorEnable = true },
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            });

            SetViewPoint(Vector3.Zero);
            mvpBuffer = device.CreateBuffer(new ViewProj(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            rghbuffer = device.CreateBuffer(new RoughnessBuffer(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerDescription.AnisotropicWrap);
        }

        public void SetViewPoint(Vector3 camera)
        {
            // The TextureCube Texture2D assumes the
            // following order of faces.

            // The LookAt targets for view matrices
            var targets = new[] {
                camera + Vector3.UnitX, // +X
                camera - Vector3.UnitX, // -X
                camera + Vector3.UnitY, // +Y
                camera - Vector3.UnitY, // -Y
                camera + Vector3.UnitZ, // +Z
                camera - Vector3.UnitZ  // -Z
            };

            var upVectors = new[] {
                Vector3.UnitY, // +X
                Vector3.UnitY, // -X
                -Vector3.UnitZ,// +Y
                Vector3.UnitZ,// -Y
                Vector3.UnitY, // +Z
                Vector3.UnitY, // -Z
            };

            for (int i = 0; i < 6; i++)
            {
                Cameras[i].View = Matrix4x4.CreateLookAt(camera, targets[i], upVectors[i]) * Matrix4x4.CreateScale(-1, 1, 1);
                Cameras[i].Projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI * 0.5f, 1.0f, 0.1f, 100.0f);
            }
        }

        public void Draw(IGraphicsContext context)
        {
            if (Targets == null)
            {
                return;
            }

            context.Write(rghbuffer, new RoughnessBuffer() { Roughness = Roughness });
            for (int i = 0; i < 6; i++)
            {
                context.Write(mvpBuffer, new ViewProj(Cameras[i].View, Cameras[i].Projection));
                Targets.ClearAndSetTarget(context, i);
                context.VSSetConstantBuffer(mvpBuffer, 0);
                context.PSSetConstantBuffer(rghbuffer, 0);
                context.PSSetShaderResource(Source, 0);
                context.PSSetSampler(sampler, 0);
                context.SetViewport(Targets.Viewport);
                cube.DrawAuto(context, pipeline);
            }
        }

        public void DrawSlice(IGraphicsContext context, int i, int x, int y, int xsize, int ysize)
        {
            if (Targets == null)
            {
                return;
            }

            context.Write(rghbuffer, new RoughnessBuffer() { Roughness = Roughness });
            context.Write(mvpBuffer, new ViewProj(Cameras[i].View, Cameras[i].Projection));
            context.SetScissorRect(x, y, xsize + x, ysize + y);
            Targets.SetTarget(context, i);
            context.SetViewport(Targets.Viewport);
            context.VSSetConstantBuffer(mvpBuffer, 0);
            context.PSSetConstantBuffer(rghbuffer, 0);
            context.PSSetShaderResource(Source, 0);
            context.PSSetSampler(sampler, 0);
            cube.DrawAuto(context, pipeline);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                cube.Dispose();
                pipeline.Dispose();
                mvpBuffer.Dispose();
                rghbuffer.Dispose();
                sampler.Dispose();
                disposedValue = true;
            }
        }

        ~PreFilter()
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