namespace HexaEngine.Core.Lights.Structs
{
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public struct ShadowSpotlightData : IEquatable<ShadowSpotlightData>
    {
        public Matrix4x4 View;
        public SpotlightData Spotlight;

        public ShadowSpotlightData(Spotlight spotlight)
        {
            View = spotlight.View;
            Spotlight.Color = spotlight.Color * spotlight.Strength;
            Spotlight.Position = spotlight.Transform.GlobalPosition;
            Spotlight.CutOff = MathF.Cos((spotlight.ConeAngle / 2).ToRad());
            Spotlight.Direction = spotlight.Transform.Forward;
            Spotlight.OuterCutOff = MathF.Cos((MathUtil.Lerp(0, spotlight.ConeAngle, 1 - spotlight.Blend) / 2).ToRad());
        }

        public void Update(Spotlight spotlight)
        {
            Spotlight.Color = spotlight.Color * spotlight.Strength;
            Spotlight.Position = spotlight.Transform.GlobalPosition;
            Spotlight.CutOff = MathF.Cos((spotlight.ConeAngle / 2).ToRad());
            Spotlight.Direction = spotlight.Transform.Forward;
            Spotlight.OuterCutOff = MathF.Cos((MathUtil.Lerp(0, spotlight.ConeAngle, 1 - spotlight.Blend) / 2).ToRad());
        }

        public override string ToString()
        {
            return Spotlight.Color.ToString();
        }

        public override bool Equals(object? obj)
        {
            return obj is ShadowSpotlightData data && Equals(data);
        }

        public bool Equals(ShadowSpotlightData other)
        {
            return View.Equals(other.View) &&
                   Spotlight.Equals(other.Spotlight);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(View, Spotlight);
        }

        public static bool operator ==(ShadowSpotlightData left, ShadowSpotlightData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ShadowSpotlightData left, ShadowSpotlightData right)
        {
            return !(left == right);
        }
    }
}