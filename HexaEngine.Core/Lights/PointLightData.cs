﻿namespace HexaEngine.Core.Lights
{
    using System.Numerics;

    public struct PointLightData
    {
        public Vector4 Color;
        public Vector3 Position;
        public float Padd;

        public PointLightData(PointLight point) : this()
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
            Padd = 0;
        }

        public void Update(PointLight point)
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }
}