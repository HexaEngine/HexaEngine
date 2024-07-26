namespace HexaEngine.Meshes
{
    using HexaEngine.Core;
    using Hexa.NET.Mathematics;
    using HexaEngine.Scenes;
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

        public uint Frame;
        public float CumulativeTime;
        public float DeltaTime;
        public float GameTime;

        public CBCamera(Camera camera, Vector2 screenDim)
        {
            var transform = camera.Transform;
            transform.Width = screenDim.X;
            transform.Height = screenDim.Y;
            Proj = Matrix4x4.Transpose(transform.Projection);
            View = Matrix4x4.Transpose(transform.View);
            ProjInv = Matrix4x4.Transpose(transform.ProjectionInv);
            ViewInv = Matrix4x4.Transpose(transform.ViewInv);
            ViewProj = Matrix4x4.Transpose(transform.ViewProjection);
            ViewProjInv = Matrix4x4.Transpose(transform.ViewProjectionInv);

            Far = camera.Far;
            Near = camera.Near;
            ScreenDim = screenDim;

            Frame = (uint)Time.Frame;
            CumulativeTime = Time.CumulativeFrameTime;
            DeltaTime = Time.Delta;
            GameTime = Time.GameTime;
        }

        public CBCamera(Camera camera, Vector2 screenDim, Matrix4x4 last)
        {
            var transform = camera.Transform;
            transform.Width = screenDim.X;
            transform.Height = screenDim.Y;
            Proj = Matrix4x4.Transpose(transform.Projection);
            View = Matrix4x4.Transpose(transform.View);
            ProjInv = Matrix4x4.Transpose(transform.ProjectionInv);
            ViewInv = Matrix4x4.Transpose(transform.ViewInv);
            ViewProj = Matrix4x4.Transpose(transform.ViewProjection);
            ViewProjInv = Matrix4x4.Transpose(transform.ViewProjectionInv);
            PrevViewProj = Matrix4x4.Transpose(last);

            Far = camera.Far;
            Near = camera.Near;
            ScreenDim = screenDim;

            Frame = (uint)Time.Frame;
            CumulativeTime = Time.CumulativeFrameTime;
            DeltaTime = Time.Delta;
            GameTime = Time.GameTime;
        }

        public CBCamera(Camera camera, Vector2 screenDim, CBCamera last)
        {
            var transform = camera.Transform;
            transform.Width = screenDim.X;
            transform.Height = screenDim.Y;
            Proj = Matrix4x4.Transpose(transform.Projection);
            View = Matrix4x4.Transpose(transform.View);
            ProjInv = Matrix4x4.Transpose(transform.ProjectionInv);
            ViewInv = Matrix4x4.Transpose(transform.ViewInv);
            ViewProj = Matrix4x4.Transpose(transform.ViewProjection);
            ViewProjInv = Matrix4x4.Transpose(transform.ViewProjectionInv);
            PrevViewProj = last.ViewProj;

            Far = camera.Far;
            Near = camera.Near;
            ScreenDim = screenDim;

            Frame = (uint)Time.Frame;
            CumulativeTime = Time.CumulativeFrameTime;
            DeltaTime = Time.Delta;
            GameTime = Time.GameTime;
        }

        public CBCamera(CameraTransform camera, Vector2 screenDim)
        {
            var transform = camera;
            transform.Width = screenDim.X;
            transform.Height = screenDim.Y;
            Proj = Matrix4x4.Transpose(transform.Projection);
            View = Matrix4x4.Transpose(transform.View);
            ProjInv = Matrix4x4.Transpose(transform.ProjectionInv);
            ViewInv = Matrix4x4.Transpose(transform.ViewInv);
            ViewProj = Matrix4x4.Transpose(transform.ViewProjection);
            ViewProjInv = Matrix4x4.Transpose(transform.ViewProjectionInv);

            Far = camera.Far;
            Near = camera.Near;
            ScreenDim = screenDim;

            Frame = (uint)Time.Frame;
            CumulativeTime = Time.CumulativeFrameTime;
            DeltaTime = Time.Delta;
            GameTime = Time.GameTime;
        }
    }
}