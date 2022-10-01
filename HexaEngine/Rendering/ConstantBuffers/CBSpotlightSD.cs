namespace HexaEngine.Rendering.ConstantBuffers
{
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;
    using MathUtil = Mathematics.MathUtil;

    public struct CBSpotlightSD
    {
        public Matrix4x4 View;
        public Matrix4x4 Proj;
        public Vector4 Color;
        public Vector3 Position;
        public float CutOff;
        public Vector3 Direction;
        public float OuterCutOff;

        public CBSpotlightSD(Spotlight spotlight)
        {
            View = Matrix4x4.Transpose(spotlight.Transform.View);
            Proj = Matrix4x4.Transpose(spotlight.Transform.Projection);
            Color = spotlight.Color * spotlight.Strength;
            Position = spotlight.Transform.GlobalPosition;
            CutOff = MathF.Cos((spotlight.ConeAngle / 2).ToRad());
            Direction = spotlight.Transform.Forward;
            OuterCutOff = MathF.Cos((MathUtil.Lerp(0, spotlight.ConeAngle, 1 - spotlight.Blend) / 2).ToRad());
        }
    }
}