namespace HexaEngine.Editor.ImagePainter.Filters
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using System;
    using System.Numerics;

    public class EquiRectangularToCubeFilter
    {
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

        private readonly Cube cube;
        private readonly IGraphicsPipelineState pipeline;
        private readonly ConstantBuffer<ModelViewProj> mvpBuffer;
        private readonly ISamplerState sampler;

        public IShaderResourceView? Source;

        public struct CubeFaceCamera
        {
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        }

        private readonly CubeFaceCamera[] Cameras = new CubeFaceCamera[6];
        private bool disposedValue;

        public EquiRectangularToCubeFilter(IGraphicsDevice device)
        {
            cube = new(new());
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.ImagePainter:shaders/filter/equirectangularToCube/vs.hlsl",
                PixelShader = "HexaEngine.ImagePainter:shaders/filter/equirectangularToCube/ps.hlsl",
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
            Vector3[] targets = [
                camera + Vector3.UnitX, // +X
                camera - Vector3.UnitX, // -X
                camera + Vector3.UnitY, // +Y
                camera - Vector3.UnitY, // -Y
                camera + Vector3.UnitZ, // +Z
                camera - Vector3.UnitZ  // -Z
            ];

            Vector3[] upVectors = [
                Vector3.UnitY, // +X
                Vector3.UnitY, // -X
                -Vector3.UnitZ,// +Y
                Vector3.UnitZ, // -Y
                Vector3.UnitY, // +Z
                Vector3.UnitY, // -Z
            ];

            for (int i = 0; i < 6; i++)
            {
                Cameras[i].View = Matrix4x4.CreateLookAt(camera, targets[i], upVectors[i]) * Matrix4x4.CreateScale(-1, 1, 1);
                Cameras[i].Projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI * 0.5f, 1.0f, 0.1f, 100.0f);
            }
        }

        public void Draw(IGraphicsContext context, IRenderTargetView[] rtvs, Viewport viewport)
        {
            for (int i = 0; i < 6; i++)
            {
                var rtv = rtvs[i];
                context.Write(mvpBuffer, new ModelViewProj(Matrix4x4.Identity, Cameras[i].View, Cameras[i].Projection));
                pipeline.Bindings.SetSRV("cubemap", Source);
                context.ClearRenderTargetView(rtv, default);
                context.SetRenderTarget(rtv, null);
                context.SetViewport(viewport);
                cube.DrawAuto(context, pipeline);
            }
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