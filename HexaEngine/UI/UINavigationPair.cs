namespace HexaEngine.UI
{
    using System.Collections.Generic;
    using System.Numerics;

    public struct UINavigationPair : IEquatable<UINavigationPair>
    {
        public IUINavigationElement First;
        public IUINavigationElement Second;
        public Vector2 Direction;
        public float Weight;

        public UINavigationPair(IUINavigationElement first, IUINavigationElement second, Vector2 direction, float weight)
        {
            First = first;
            Second = second;
            Direction = direction;
            Weight = weight;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is UINavigationPair pair && Equals(pair);
        }

        public readonly bool Equals(UINavigationPair other)
        {
            return EqualityComparer<IUINavigationElement>.Default.Equals(First, other.First) &&
                   EqualityComparer<IUINavigationElement>.Default.Equals(Second, other.Second);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(First, Second);
        }

        public static bool operator ==(UINavigationPair left, UINavigationPair right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UINavigationPair left, UINavigationPair right)
        {
            return !(left == right);
        }

        public readonly bool HasComponent(IUINavigationElement component)
        {
            return component == First || component == Second;
        }

        public readonly IUINavigationElement GetOther(IUINavigationElement component)
        {
            if (component == First)
                return Second;
            return First;
        }
    }
}