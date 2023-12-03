namespace HexaEngine.AI.Pathfinding
{
    using System;

    public interface IAStarVolume<TState> where TState : notnull, IEquatable<TState>
    {
        public bool IsBlocked(TState state);

        public bool InBounds(TState state);

        public IEnumerable<AStarNode<TState>> GetNeighbors(AStarNode<TState> node);
    }
}