namespace HexaEngine.Scenes
{
    using HexaEngine.Components;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Numerics;

    public enum SensorFit
    {
        Auto = 0,
        Horizontal,
        Vertical
    }

    public enum LensUnit
    {
        FieldOfView,
        Millimeters,
    }

    public enum CameraMode
    {
        NonPhysical,
        Physical,
    }

    [EditorGameObject<Camera>("Camera")]
    public class Camera : GameObject
    {
        private float focalLength = 50;
        private float sensorSize = 36;
        private CameraTransform transform = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        public Camera()
        {
            OverwriteTransform(transform);
            focalLength = FovToFocalLength(Transform.Fov, sensorSize);
            AddComponentSingleton<SphereSelectionComponent>();
        }

        public new CameraTransform Transform
        {
            get => transform;
            set
            {
                transform = value;
                OverwriteTransform(value);
            }
        }

        [EditorProperty("Visible Layers")]
        public DrawLayerCollection VisibleLayers { get; set; } = [];

        [EditorCategory("Lens")]
        [EditorProperty<ProjectionType>("Type")]
        public ProjectionType ProjectionType { get => Transform.ProjectionType; set => Transform.ProjectionType = value; }

        [EditorCategory("Lens")]
        [EditorPropertyCondition<Camera>(nameof(IsLensUnitFov))]
        [EditorProperty("Field of view", 0f, float.Pi - 0.0174532925f, EditorPropertyMode.SliderAngle)]
        public float Fov
        {
            get => Transform.Fov;
            set
            {
                Transform.Fov = value;
                focalLength = FovToFocalLength(value, sensorSize);
            }
        }

        [EditorCategory("Lens")]
        [EditorPropertyCondition<Camera>(nameof(IsLensUnitMM))]
        [EditorProperty("Focal Length")]
        public float FocalLength
        {
            get => focalLength;
            set
            {
                Transform.Fov = FocalLengthToFov(value, sensorSize);
                focalLength = value;
            }
        }

        [EditorCategory("Lens")]
        [EditorProperty<LensUnit>("Lens Unit")]
        public LensUnit LensUnit { get; set; } = LensUnit.Millimeters;

        [EditorCategory("Lens")]
        [EditorProperty("Far")]
        public float Far { get => Transform.Far; set => Transform.Far = value; }

        [EditorCategory("Lens")]
        [EditorProperty("Near")]
        public float Near { get => Transform.Near; set => Transform.Near = value; }

        public float Width { get => Transform.Width; set => Transform.Width = value; }

        public float Height { get => Transform.Height; set => Transform.Height = value; }

        [EditorCategory("Camera")]
        [EditorProperty<SensorFit>("Sensor Fit")]
        public SensorFit SensorFit { get; set; } = SensorFit.Auto;

        [EditorCategory("Camera")]
        [EditorProperty("Sensor Size")]
        public float SensorSize
        {
            get => sensorSize;
            set
            {
                sensorSize = value;
                Transform.Fov = FocalLengthToFov(focalLength, value);
            }
        }

        [EditorCategory("Debug")]
        [EditorProperty("Visualize Culling")]
        public bool VisualizeCulling
        {
            get => CameraManager.Culling == this;
            set
            {
                CameraManager.Culling = value ? this : null;
            }
        }

        public bool AutoFocus { get; set; }

        public float FocusDistance { get; set; }

        public float ApertureFStop { get; set; }

        public int ApertureBlades { get; set; }

        public float ApertureRotation { get; set; }

        public float ApertureRatio { get; set; }

        [EditorCategory("Camera")]
        [EditorButton("Copy from View")]
        public void CopySettings()
        {
            var current = CameraManager.Current;
            if (current == null)
                return;
            Transform.PositionRotation = current.Transform.PositionRotation;
        }

        public static float FocalLengthToFov(float focalLength, float sensorSize)
        {
            return 2 * MathF.Atan(sensorSize / (2 * focalLength));
        }

        public static float FovToFocalLength(float fov, float sensorSize)
        {
            return sensorSize / (2 * MathF.Tan(fov / 2));
        }

        public static bool IsLensUnitFov(Camera camera)
        {
            return camera.LensUnit == LensUnit.FieldOfView;
        }

        public static bool IsLensUnitMM(Camera camera)
        {
            return camera.LensUnit == LensUnit.Millimeters;
        }

        public override void Initialize()
        {
            OverwriteTransform(Transform);
            base.Initialize();
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