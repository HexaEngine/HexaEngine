namespace HexaEngine.Core.Scenes
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public struct CBCamera
    {
        public Matrix4x4 View;
        public Matrix4x4 Proj;
        public Matrix4x4 ViewInv;
        public Matrix4x4 ProjInv;
        public Matrix4x4 ViewProj;
        public Matrix4x4 ViewProjInv;
        public Matrix4x4 PrevViewProj;
        public float Far;
        public float Near;
        public Vector2 ScreenDim;

        public CBCamera(Camera camera, Vector2 screenDim)
        {
            Proj = Matrix4x4.Transpose(camera.Transform.Projection);
            View = Matrix4x4.Transpose(camera.Transform.View);
            ProjInv = Matrix4x4.Transpose(camera.Transform.ProjectionInv);
            ViewInv = Matrix4x4.Transpose(camera.Transform.ViewInv);
            ViewProj = Matrix4x4.Transpose(camera.Transform.ViewProjection);
            ViewProjInv = Matrix4x4.Transpose(camera.Transform.ViewProjectionInv);
            PrevViewProj = Matrix4x4.Transpose(camera.Transform.PrevViewProjection);
            Far = camera.Far;
            Near = camera.Near;
            ScreenDim = screenDim;
        }

        public CBCamera(Camera camera, Vector2 screenDim, Matrix4x4 last)
        {
            Proj = Matrix4x4.Transpose(camera.Transform.Projection);
            View = Matrix4x4.Transpose(camera.Transform.View);
            ProjInv = Matrix4x4.Transpose(camera.Transform.ProjectionInv);
            ViewInv = Matrix4x4.Transpose(camera.Transform.ViewInv);
            ViewProj = Matrix4x4.Transpose(camera.Transform.ViewProjection);
            ViewProjInv = Matrix4x4.Transpose(camera.Transform.ViewProjectionInv);
            PrevViewProj = Matrix4x4.Transpose(last);
            Far = camera.Far;
            Near = camera.Near;
            ScreenDim = screenDim;
        }

        public CBCamera(Camera camera, Vector2 screenDim, CBCamera last)
        {
            Proj = Matrix4x4.Transpose(camera.Transform.Projection);
            View = Matrix4x4.Transpose(camera.Transform.View);
            ProjInv = Matrix4x4.Transpose(camera.Transform.ProjectionInv);
            ViewInv = Matrix4x4.Transpose(camera.Transform.ViewInv);
            ViewProj = Matrix4x4.Transpose(camera.Transform.ViewProjection);
            ViewProjInv = Matrix4x4.Transpose(camera.Transform.ViewProjectionInv);
            PrevViewProj = last.ViewProj;
            Far = camera.Far;
            Near = camera.Near;
            ScreenDim = screenDim;
        }

        public CBCamera(CameraTransform camera, Vector2 screenDim)
        {
            Proj = Matrix4x4.Transpose(camera.Projection);
            View = Matrix4x4.Transpose(camera.View);
            ProjInv = Matrix4x4.Transpose(camera.ProjectionInv);
            ViewInv = Matrix4x4.Transpose(camera.ViewInv);
            ViewProj = Matrix4x4.Transpose(camera.ViewProjection);
            ViewProjInv = Matrix4x4.Transpose(camera.ViewProjectionInv);
            PrevViewProj = Matrix4x4.Transpose(camera.PrevViewProjection);
            Far = camera.Far;
            Near = camera.Near;
            ScreenDim = screenDim;
        }
    }
}