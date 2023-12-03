namespace HexaEngine.AI.Pathfinding
{
    using System;

    public class AStar<TState, TVolume> where TState : notnull, IEquatable<TState> where TVolume : IAStarVolume<TState>
    {
        private readonly Func<AStarNode<TState>, AStarNode<TState>, double> heuristic;
        private readonly TVolume volume;

        public AStar(Func<AStarNode<TState>, AStarNode<TState>, double> heuristic, TVolume volume)
        {
            this.heuristic = heuristic;
            this.volume = volume;
        }

        public List<AStarNode<TState>> ReconstructPath(AStarNode<TState>? node)
        {
            List<AStarNode<TState>> path = [];

            while (node is not null)
            {
                path.Add(node);
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }

        public unsafe List<AStarNode<TState>>? FindPath(TState start, TState goal)
        {
            if (!volume.InBounds(start) || !volume.InBounds(goal))
            {
                throw new ArgumentException("");
            }

            List<AStarNode<TState>>? path = null;

            AStarNode<TState> startNode = new(start);
            AStarNode<TState> goalNode = new(goal);

            List<AStarNode<TState>> openList = new();
            HashSet<AStarNode<TState>> closedList = new();

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                AStarNode<TState> currentNode = openList[0];

                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].F < currentNode.F)
                    {
                        currentNode = openList[i];
                    }
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (currentNode == goalNode)
                {
                    path = ReconstructPath(currentNode);
                    break;
                }

                var neighbors = volume.GetNeighbors(currentNode);

                foreach (var neighbor in neighbors)
                {
                    if (closedList.Contains(neighbor) || volume.IsBlocked(neighbor.State))
                    {
                        continue;
                    }

                    double tentativeG = currentNode.G + 1;

                    if (!openList.Contains(neighbor) || tentativeG < neighbor.G)
                    {
                        neighbor.Parent = currentNode;
                        neighbor.G = tentativeG;
                        neighbor.H = heuristic(neighbor, goalNode);

                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }

            return path;
        }
    }
}