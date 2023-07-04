namespace HexaEngine.Core.Animations
{
    using System;

    public struct AnimatorCondition : IEquatable<AnimatorCondition>
    {
        public AnimatorConditionMode Mode;
        public string Parameter;
        public float Threshold;

        public override readonly bool Equals(object? obj)
        {
            return obj is AnimatorCondition condition && Equals(condition);
        }

        public readonly bool Equals(AnimatorCondition other)
        {
            return Mode == other.Mode &&
                   Parameter == other.Parameter &&
                   Threshold == other.Threshold;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Mode, Parameter, Threshold);
        }

        public static bool operator ==(AnimatorCondition left, AnimatorCondition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AnimatorCondition left, AnimatorCondition right)
        {
            return !(left == right);
        }
    }
}