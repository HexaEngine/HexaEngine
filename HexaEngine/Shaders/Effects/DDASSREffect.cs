namespace HexaEngine.Shaders.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Objects.Primitives;
    using System.Numerics;

    public class DDASSREffect : Effect
    {
        private readonly IBuffer mvp;

        private struct ViewProj2
        {
            public Matrix4x4 View;
            public Matrix4x4 ViewInv;
            public Matrix4x4 Projection;
            public Matrix4x4 ProjectionInv;

            public ViewProj2(Matrix4x4 view, Matrix4x4 viewInv, Matrix4x4 projection, Matrix4x4 projectionInv)
            {
                View = Matrix4x4.Transpose(view);
                ViewInv = Matrix4x4.Transpose(viewInv);
                Projection = Matrix4x4.Transpose(projection);
                ProjectionInv = Matrix4x4.Transpose(projectionInv);
            }

            public ViewProj2(Matrix4x4 view, Matrix4x4 projection)
            {
                Matrix4x4.Invert(view, out Matrix4x4 viewInv);
                Matrix4x4.Invert(projection, out Matrix4x4 projectionInv);
                View = Matrix4x4.Transpose(view);
                ViewInv = Matrix4x4.Transpose(viewInv);
                Projection = Matrix4x4.Transpose(projection);
                ProjectionInv = Matrix4x4.Transpose(projectionInv);
            }

            public ViewProj2(IView view) : this(view.Transform.View, view.Transform.Projection)
            {
            }
        }

        public DDASSREffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/ddassr/vs.hlsl",
            PixelShader = "effects/ddassr/ps.hlsl",
        })
        {
            mvp = CreateConstantBuffer<ViewProj2>(ShaderStage.Pixel, 0);
            Mesh = new Quad(device);
        }

        public override void Draw(IGraphicsContext context, IView view)
        {
            context.Write(mvp, new ViewProj2(view));
            DrawAuto(context, Target.Viewport, null);
        }
    }
}