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
        public float Far;
        public float Near;
        public Vector2 Padd;

        public CBCamera(Camera camera)
        {
            Proj = Matrix4x4.Transpose(camera.Transform.Projection);
            View = Matrix4x4.Transpose(camera.Transform.View);
            ProjInv = Matrix4x4.Transpose(camera.Transform.ProjectionInv);
            ViewInv = Matrix4x4.Transpose(camera.Transform.ViewInv);
            Far = camera.Far;
            Near = camera.Near;
            Padd = default;
        }

        public CBCamera(CameraTransform camera)
        {
            Proj = Matrix4x4.Transpose(camera.Projection);
            View = Matrix4x4.Transpose(camera.View);
            ProjInv = Matrix4x4.Transpose(camera.ProjectionInv);
            ViewInv = Matrix4x4.Transpose(camera.ViewInv);
            Far = camera.Far;
            Near = camera.Near;
            Padd = default;
        }
    }
}