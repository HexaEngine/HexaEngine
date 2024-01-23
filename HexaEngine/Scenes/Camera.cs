namespace HexaEngine.Scenes
{
    using HexaEngine.Components;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Numerics;

    [EditorGameObject<Camera>("Camera")]
    public class Camera : GameObject
    {
        public new CameraTransform Transform;

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        public Camera()
        {
            base.Transform = Transform = new();
            AddComponentSingleton<SphereSelectionComponent>();
        }

        [EditorProperty("Visible Layers")]
        public DrawLayerCollection VisibleLayers { get; set; } = [];

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

        public override void Uninitialize()
        {
            base.Uninitialize();
        }

        public Matrix4x4 GetCameraProjection(float near)
        {
            switch (ProjectionType)
            {
                case ProjectionType.Perspective:
                    return MathUtil.PerspectiveFovLH(Fov, Width / Height, near, Far);

                case ProjectionType.Orthographic:
                    return MathUtil.OrthoLH(Width, Height, near, Far);

                default:
                    throw new NotSupportedException();
            }
        }

        public Vector4 ProjectPosition(Vector3 position)
        {
            Vector4 posH = Vector4.Transform(new Vector4(position, 1.0f), Transform.Projection);
            posH = new Vector4(new Vector3(posH.X, posH.Y, posH.Z) / posH.W, 1.0f);
            Vector2 screenPos = new Vector2(posH.X, posH.Y) / posH.Z;
            screenPos = screenPos * new Vector2(0.5f, -0.5f) + new Vector2(0.5f);
            return new Vector4(screenPos, posH.Z, posH.W);
        }

        public Vector3 ProjectPosition(Vector2 position, float depth, Viewport viewport)
        {
            throw new NotImplementedException();
            /*
            if (depth == 0 && ProjectionType != ProjectionType.Orthographic)
            {
                return Transform.GlobalPosition;
            }

            Vector2 viewport_size = viewport.Size;

            var cm = GetCameraProjection(depth);

            Vector2 vp_he = cm.get_viewport_half_extents();

            Vector2 point;
            point.X = (position.X / viewport_size.X) * 2.0f - 1.0f;
            point.Y = (1.0f - (position.Y / viewport_size.Y)) * 2.0f - 1.0f;
            point *= vp_he;

            Vector3 p = new(point.X, point.Y, -depth);

            return Vector3.Transform(p, Transform);
            */
        }

        public float DistanceTo(GameObject other)
        {
            return Vector3.Distance(Transform.GlobalPosition, other.Transform.Position);
        }

        public static implicit operator CameraTransform(Camera camera) => camera.Transform;
    }
}