namespace HexaEngine.Scenes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    [EditorGameObject<Camera>("camera")]
    public class Camera : GameObject, IView
    {
        public new CameraTransform Transform;

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        public Camera()
        {
            base.Transform = Transform = new();
        }

        [EditorProperty<ProjectionType>("Type")]
        public ProjectionType ProjectionType { get => Transform.ProjectionType; set => Transform.ProjectionType = value; }

        [EditorProperty(nameof(Fov))]
        public float Fov { get => Transform.Fov; set => Transform.Fov = value; }

        [EditorProperty(nameof(Far))]
        public float Far { get => Transform.Far; set => Transform.Far = value; }

        [EditorProperty(nameof(Near))]
        public float Near { get => Transform.Near; set => Transform.Near = value; }

        [EditorProperty(nameof(Width))]
        public float Width { get => Transform.Width; set => Transform.Width = value; }

        [EditorProperty(nameof(Height))]
        public float Height { get => Transform.Height; set => Transform.Height = value; }

        [EditorProperty(nameof(VisualizeCulling))]
        public bool VisualizeCulling
        {
            get => CameraManager.Culling == this;
            set
            {
                CameraManager.Culling = value ? this : null;
            }
        }

        [EditorButton("Copy from View")]
        public void CopySettings()
        {
            var current = CameraManager.Current;
            if (current == null)
                return;
            Transform.PositionRotation = current.Transform.PositionRotation;
        }

        CameraTransform IView.Transform => Transform;

        public override void Initialize(IGraphicsDevice device)
        {
            base.Initialize(device);
        }

        public override void Uninitialize()
        {
            base.Uninitialize();
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