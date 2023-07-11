namespace HexaEngine.Core.Graphics.Structs
{
    using HexaEngine.Core.Graphics;
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
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

        public ModelViewProj(IView view, Matrix4x4 model) : this(model, view.Transform.View, view.Transform.Projection)
        {
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ViewProj
    {
        public Matrix4x4 View;
        public Matrix4x4 Projection;

        public ViewProj(Matrix4x4 view, Matrix4x4 projection)
        {
            View = Matrix4x4.Transpose(view);
            Projection = Matrix4x4.Transpose(projection);
        }

        public ViewProj(IView view) : this(view.Transform.View, view.Transform.Projection)
        {
        }
    }
}