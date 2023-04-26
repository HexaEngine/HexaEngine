namespace HexaEngine.Core.Lights.Structs
{
    using HexaEngine.Core.Lights.Types;
    using System;

    public struct ShadowPointLightData : IEquatable<ShadowPointLightData>
    {
        public PointLightData Data;
        public float Far;

        public ShadowPointLightData(PointLight point) : this()
        {
            Data.Color = point.Color * point.Strength;
            Data.Position = point.Transform.GlobalPosition;
            Far = point.ShadowRange;
        }

        public void Update(PointLight point)
        {
            Data.Color = point.Color * point.Strength;
            Data.Position = point.Transform.GlobalPosition;
            Far = point.ShadowRange;
        }

        public override string ToString()
        {
            return Data.Color.ToString();
        }

        public override bool Equals(object? obj)
        {
            return obj is ShadowPointLightData data && Equals(data);
        }

        public bool Equals(ShadowPointLightData other)
        {
            return Data.Equals(other.Data) &&
                   Far == other.Far;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Data, Far);
        }

        public static bool operator ==(ShadowPointLightData left, ShadowPointLightData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ShadowPointLightData left, ShadowPointLightData right)
        {
            return !(left == right);
        }
    }
}