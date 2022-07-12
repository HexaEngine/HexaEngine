namespace HexaEngine.Lights
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    [EditorNode("Spotlight")]
    public class Spotlight : Light
    {
        public new CameraTransform Transform;
        private float coneAngle;
        private float blend;

        public override LightType Type => LightType.Spot;

        public Spotlight()
        {
            base.Transform = Transform = new();
        }

        [EditorProperty("Strength")]
        public float Strength { get; set; } = 1000;

        [EditorProperty("Cone Angle", EditorPropertyMode.Slider, 1f, 180f)]
        public float ConeAngle { get => coneAngle; set => coneAngle = value; }

        [EditorProperty("Blend", EditorPropertyMode.Slider, 0f, 1f)]
        public float Blend { get => blend; set => blend = value; }

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
    }
}