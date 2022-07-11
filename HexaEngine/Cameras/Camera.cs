namespace HexaEngine.Cameras
{
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using System.Numerics;

    [EditorNode("Camera")]
    public class Camera : SceneNode, IView
    {
        public new CameraTransform Transform;
        private bool autoSize = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        public Camera()
        {
            base.Transform = Transform = new();
        }

        [EditorProperty("Type")]
        public ProjectionType ProjectionType { get => Transform.ProjectionType; set => Transform.ProjectionType = value; }

        [EditorProperty(nameof(Fov))]
        public float Fov { get => Transform.Fov; set => Transform.Fov = value; }

        [EditorProperty(nameof(Far))]
        public float Far { get => Transform.Far; set => Transform.Far = value; }

        [EditorProperty(nameof(Near))]
        public float Near { get => Transform.Near; set => Transform.Near = value; }

        [EditorProperty("Auto Size")]
        public bool AutoSize { get => autoSize; set => autoSize = value; }

        [EditorProperty(nameof(Width))]
        public float Width { get => Transform.Width; set => Transform.Width = value; }

        [EditorProperty(nameof(Height))]
        public float Height { get => Transform.Height; set => Transform.Height = value; }

        CameraTransform IView.Transform => Transform;

        public override void Initialize(IGraphicsDevice device)
        {
            device.SwapChain.Resized += Resized;
            base.Initialize(device);
            if (!autoSize) return;
            Transform.Width = device.SwapChain.BackbufferRTV.Viewport.Width;
            Transform.Height = device.SwapChain.BackbufferRTV.Viewport.Height;
        }

        public override void Uninitialize()
        {
            Device.SwapChain.Resized -= Resized;
            base.Uninitialize();
        }

        private void Resized(object sender, ResizedEventArgs e)
        {
            if (!autoSize) return;
            Transform.Width = e.NewWidth;
            Transform.Height = e.NewHeight;
        }

        public static implicit operator CameraTransform(Camera camera) => camera.Transform;
    }

    public struct CameraData
    {
        public CameraData(Vector3 position, Matrix4x4 view, Matrix4x4 proj)
        {
            Position = position;
            reserved = 0;
            View = Matrix4x4.Transpose(view);
            Proj = Matrix4x4.Transpose(proj);
        }

        public Vector3 Position;
        public float reserved;
        public Matrix4x4 View;
        public Matrix4x4 Proj;
    }
}