namespace HexaEngine.Core.Lights.Structs
{
    using HexaEngine.Core.Lights.Types;
    using System;
    using System.Numerics;

    public struct DirectionalLightData : IEquatable<DirectionalLightData>
    {
        public Vector4 Color;
        public Vector3 Direction;

        public DirectionalLightData(DirectionalLight light)
        {
            Color = light.Color;
            Direction = light.Transform.Forward;
        }

        public void Update(DirectionalLight light)
        {
            Color = light.Color;
            Direction = light.Transform.Forward;
        }

        public override string ToString()
        {
            return Color.ToString();
        }

        public override bool Equals(object? obj)
        {
            return obj is DirectionalLightData data && Equals(data);
        }

        public bool Equals(DirectionalLightData other)
        {
            return Color.Equals(other.Color) &&
                   Direction.Equals(other.Direction);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Color, Direction);
        }

        public static bool operator ==(DirectionalLightData left, DirectionalLightData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DirectionalLightData left, DirectionalLightData right)
        {
            return !(left == right);
        }
    }
}