namespace HexaEngine.Core.Lights.Structs
{
    using HexaEngine.Core.Lights.Types;
    using System;
    using System.Numerics;

    public struct PointLightData : IEquatable<PointLightData>
    {
        public Vector4 Color;
        public Vector3 Position;

        public PointLightData(PointLight point) : this()
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
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

        public override bool Equals(object? obj)
        {
            return obj is PointLightData data && Equals(data);
        }

        public bool Equals(PointLightData other)
        {
            return Color.Equals(other.Color) &&
                   Position.Equals(other.Position);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Color, Position);
        }

        public static bool operator ==(PointLightData left, PointLightData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PointLightData left, PointLightData right)
        {
            return !(left == right);
        }
    }
}