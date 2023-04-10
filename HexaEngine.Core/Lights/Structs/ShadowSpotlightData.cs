namespace HexaEngine.Core.Lights.Structs
{
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public struct ShadowSpotlightData
    {
        public Matrix4x4 View;
        public Vector4 Color;
        public Vector3 Position;
        public float CutOff;
        public Vector3 Direction;
        public float OuterCutOff;

        public ShadowSpotlightData(Spotlight spotlight)
        {
            View = spotlight.View;
            Color = spotlight.Color * spotlight.Strength;
            Position = spotlight.Transform.GlobalPosition;
            CutOff = MathF.Cos((spotlight.ConeAngle / 2).ToRad());
            Direction = spotlight.Transform.Forward;
            OuterCutOff = MathF.Cos((MathUtil.Lerp(0, spotlight.ConeAngle, 1 - spotlight.Blend) / 2).ToRad());
        }

        public void Update(Spotlight spotlight)
        {
            Color = spotlight.Color * spotlight.Strength;
            Position = spotlight.Transform.GlobalPosition;
            CutOff = MathF.Cos((spotlight.ConeAngle / 2).ToRad());
            Direction = spotlight.Transform.Forward;
            OuterCutOff = MathF.Cos((MathUtil.Lerp(0, spotlight.ConeAngle, 1 - spotlight.Blend) / 2).ToRad());
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }
}