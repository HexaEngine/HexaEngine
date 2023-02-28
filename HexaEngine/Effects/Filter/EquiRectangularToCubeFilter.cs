#nullable disable


namespace HexaEngine.Effects.Filter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Graphics.Structs;
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    public class EquiRectangularToCubeFilter : IEffect
    {
        private Cube cube;
        private IGraphicsPipeline pipeline;
        private IBuffer mvpBuffer;
        private ISamplerState sampler;

        public IShaderResourceView Source;
        public RenderTargetViewArray Targets;

        public struct CubeFaceCamera
        {
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        }

        private CubeFaceCamera[] Cameras;
        private bool disposedValue;

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
                Vector3.UnitZ, // -Y
                Vector3.UnitY, // +Z
                Vector3.UnitY, // -Z
            };

            Cameras = new CubeFaceCamera[6];

            for (int i = 0; i < 6; i++)
            {
                Cameras[i].View = Matrix4x4.CreateLookAt(camera, targets[i], upVectors[i]) * Matrix4x4.CreateScale(-1, 1, 1);
                Cameras[i].Projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI * 0.5f, 1.0f, 0.1f, 100.0f);
            }
        }

        public Task Initialize(IGraphicsDevice device, int width, int height)
        {
            cube = new(device);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/equitocube/vs.hlsl",
                PixelShader = "effects/equitocube/ps.hlsl"
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            });

            SetViewPoint(Vector3.Zero);
            mvpBuffer = device.CreateBuffer(new ModelViewProj(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerDescription.AnisotropicWrap);
            return Task.CompletedTask;
        }

        public void Draw(IGraphicsContext context)
        {
            for (int i = 0; i < 6; i++)
            {
                context.Write(mvpBuffer, new ModelViewProj(Matrix4x4.Identity, Cameras[i].View, Cameras[i].Projection));
                context.VSSetConstantBuffer(mvpBuffer, 0);
                context.PSSetSampler(sampler, 0);
                context.PSSetShaderResource(Source, 0);
                Targets.ClearAndSetTarget(context, i);
                cube.DrawAuto(context, pipeline, Targets.Viewport);
            }
        }

        public void BeginResize()
        {
        }

        public void EndResize(int width, int height)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline.Dispose();
                mvpBuffer.Dispose();
                sampler.Dispose();
                disposedValue = true;
            }
        }

        ~EquiRectangularToCubeFilter()
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