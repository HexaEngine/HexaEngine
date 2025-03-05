#nullable disable

namespace HexaEngine.Graphics.Filters
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using System;
    using System.Numerics;

    public struct ModelViewProj
    {
        public Matrix4x4 Model;
        public Matrix4x4 View;
        public Matrix4x4 Projection;

        public ModelViewProj(Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection)
        {
            Model = Matrix4x4.Transpose(model);
            View = Matrix4x4.Transpose(view);
            Projection = Matrix4x4.Transpose(projection);
        }
    }

    public class EquiRectangularToCubeFilter : IFilter
    {
        private Cube cube;
        private IGraphicsPipelineState pipeline;
        private ConstantBuffer<ModelViewProj> mvpBuffer;
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

        public EquiRectangularToCubeFilter(IGraphicsDevice device)
        {
            cube = new(new());
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "filter/equirectangularToCube/vs.hlsl",
                PixelShader = "filter/equirectangularToCube/ps.hlsl",
            }, new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            });

            SetViewPoint(Vector3.Zero);
            mvpBuffer = new(CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerStateDescription.AnisotropicWrap);
            pipeline.Bindings.SetCBV("MatrixBuffer", mvpBuffer);
            pipeline.Bindings.SetSampler("samplerState", sampler);
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

        public void Draw(IGraphicsContext context)
        {
            if (Targets == null)
            {
                return;
            }

            for (int i = 0; i < 6; i++)
            {
                context.Write(mvpBuffer, new ModelViewProj(Matrix4x4.Identity, Cameras[i].View, Cameras[i].Projection));
                pipeline.Bindings.SetSRV("cubemap", Source);
                Targets.ClearAndSetTarget(context, i);
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

            context.Write(mvpBuffer, new ModelViewProj(Matrix4x4.Identity, Cameras[i].View, Cameras[i].Projection));
            context.SetScissorRect(x, y, xsize + x, ysize + y);
            Targets.SetTarget(context, i);
            context.SetViewport(Targets.Viewport);
            pipeline.Bindings.SetSRV("cubemap", Source);
            cube.DrawAuto(context, pipeline);
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}