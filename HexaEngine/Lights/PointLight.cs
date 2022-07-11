namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using System;

    [EditorNode("Point Light")]
    public class PointLight : Light
    {
        /*
        private Matrix4x4 forwardView;
        private Matrix4x4 backwardView;
        private Matrix4x4 upView;
        private Matrix4x4 downView;
        private Matrix4x4 leftView;
        private Matrix4x4 rightView;
        private Matrix4x4 projection;
        private BoundingFrustum forwardFrustum;
        private BoundingFrustum backwardFrustum;
        private BoundingFrustum upFrustum;
        private BoundingFrustum downFrustum;
        private BoundingFrustum leftFrustum;
        private BoundingFrustum rightFrustum;

        public Matrix4x4 ForwardView => forwardView;

        public Matrix4x4 BackwardView => backwardView;

        public Matrix4x4 UpView => upView;

        public Matrix4x4 DownView => downView;

        public Matrix4x4 LeftView => leftView;

        public Matrix4x4 RightView => rightView;

        public BoundingFrustum ForwardFrustum => forwardFrustum;

        public BoundingFrustum BackwardFrustum => backwardFrustum;

        public BoundingFrustum UpFrustum => upFrustum;

        public BoundingFrustum DownFrustum => downFrustum;

        public BoundingFrustum LeftFrustum => leftFrustum;

        public BoundingFrustum RightFrustum => rightFrustum;
              */
        public override LightType Type => LightType.Point;

        public override void BindDepth(IGraphicsContext context, int startSlot)
        {
            throw new NotImplementedException();
        }

        public override void ClearDepth(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public override void Render(IGraphicsContext context, Viewport viewport, IView view, IView scene, int indexCount)
        {
            throw new System.NotImplementedException();
        }

        public override void RenderDepth(IGraphicsContext context, Action<IGraphicsContext, Viewport, IView> callback)
        {
            throw new NotImplementedException();
        }
    }
}