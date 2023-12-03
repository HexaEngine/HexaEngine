namespace HexaEngine.AI.Pathfinding
{
    using System;

    public class AStarNode<TState> where TState : notnull, IEquatable<TState>
    {
        public readonly TState State;

        public double G;
        public double H;

        public double F => G + H;

        public AStarNode<TState>? Parent;

        public AStarNode(TState state)
        {
            State = state;
            G = 0;
            H = 0;
            Parent = null;
        }

        public override int GetHashCode()
        {
            return State.GetHashCode();
        }

        public bool Equals(AStarNode<TState> other)
        {
            return other.State.Equals(State);
        }

        public static bool operator ==(AStarNode<TState> left, AStarNode<TState> right)
        {
            return left.State.Equals(right.State);
        }

        public static bool operator !=(AStarNode<TState> left, AStarNode<TState> right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is AStarNode<TState> node && node.State.Equals(State);
        }
    }
}