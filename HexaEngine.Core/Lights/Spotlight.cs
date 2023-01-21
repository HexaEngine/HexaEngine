namespace HexaEngine.Core.Lights
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    [EditorNode<Spotlight>("Spotlight")]
    public class Spotlight : Light
    {
        public new CameraTransform Transform;

        private float strength = 1;
        private float coneAngle;
        private float blend;
        private float falloff = 100;

        public override LightType LightType => LightType.Spot;

        public Spotlight()
        {
            base.Transform = Transform = new();
            OverwriteTransform(Transform);
        }

        [EditorProperty("Strength")]
        public float Strength { get => strength; set => SetAndNotifyWithEqualsTest(ref strength, value); }

        [EditorProperty("Shadow Range")]
        public float ShadowRange
        {
            get => Transform.Far;
            set
            {
                var target = Transform.Far;
                if (SetAndNotifyWithEqualsTest(ref target, value))
                    Transform.Far = target;
            }
        }

        [EditorProperty("Falloff")]
        public float Falloff { get => falloff; set => SetAndNotifyWithEqualsTest(ref falloff, value); }

        [EditorProperty("Cone Angle", 1f, 180f, EditorPropertyMode.Slider)]
        public float ConeAngle { get => coneAngle; set => SetAndNotifyWithEqualsTest(ref coneAngle, value); }

        [EditorProperty("Blend", 0f, 1f, EditorPropertyMode.Slider)]
        public float Blend { get => blend; set => SetAndNotifyWithEqualsTest(ref blend, value); }

        public float GetConeRadius(float z)
        {
            return MathF.Tan((coneAngle / 2).ToRad()) * z;
        }

        public (Vector3, Vector3) GetConeEllipse(float z)
        {
            float r = MathF.Tan((coneAngle / 2).ToRad()) * z;
            Vector3 major = new(r, 0, 0);
            Vector3 minor = new(0, r, 0);
            major = Vector3.Transform(major, Transform.Orientation);
            minor = Vector3.Transform(minor, Transform.Orientation);
            return (major, minor);
        }

        public float GetInnerConeRadius(float z)
        {
            return MathF.Tan((MathUtil.Lerp(0, coneAngle, 1 - blend) / 2).ToRad()) * z;
        }

        public (Vector3, Vector3) GetInnerConeEllipse(float z)
        {
            float r = MathF.Tan((MathUtil.Lerp(0, coneAngle, 1 - blend) / 2).ToRad()) * z;
            Vector3 major = new(r, 0, 0);
            Vector3 minor = new(0, r, 0);
            major = Vector3.Transform(major, Transform.Orientation);
            minor = Vector3.Transform(minor, Transform.Orientation);
            return (major, minor);
        }

        public override bool IntersectFrustum(BoundingBox box)
        {
            return Transform.Frustum.Intersects(box);
        }
    }
}