namespace HexaEngine.Core.Lights.Structs
{
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public struct SpotlightData : IEquatable<SpotlightData>
    {
        public Vector4 Color;
        public Vector3 Position;
        public float CutOff;
        public Vector3 Direction;
        public float OuterCutOff;

        public SpotlightData(Spotlight spotlight)
        {
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

        public override bool Equals(object? obj)
        {
            return obj is SpotlightData data && Equals(data);
        }

        public bool Equals(SpotlightData other)
        {
            return Color.Equals(other.Color) &&
                   Position.Equals(other.Position) &&
                   CutOff == other.CutOff &&
                   Direction.Equals(other.Direction) &&
                   OuterCutOff == other.OuterCutOff;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Color, Position, CutOff, Direction, OuterCutOff);
        }

        public static bool operator ==(SpotlightData left, SpotlightData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SpotlightData left, SpotlightData right)
        {
            return !(left == right);
        }
    }
}