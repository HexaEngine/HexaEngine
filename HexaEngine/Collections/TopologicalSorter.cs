namespace HexaEngine.Collections
{
    public interface INode
    {
        public List<INode> Dependencies { get; }
    }

    public class TopologicalSorter<T> where T : INode
    {
        private readonly List<T> sortedList = new();
        private readonly HashSet<T> visitedNodes = new();
        private readonly HashSet<T> recursionStack = new();
        private readonly Stack<T> cycleStack = new();

        public List<T> TopologicalSort(List<T> nodes)
        {
            sortedList.Clear();
            visitedNodes.Clear();
            recursionStack.Clear();
            cycleStack.Clear();

            for (int i = 0; i < nodes.Count; i++)
            {
                T node = nodes[i];
                if (!visitedNodes.Contains(node))
                {
                    if (!TopologicalSortRecursive(node, nodes))
                    {
                        // A cycle is detected
                        throw new Exception("The graph contains a cycle: " + GetCyclePath());
                    }
                }
            }

            return sortedList;
        }

        public List<T> TopologicalSort(List<T> nodes, List<T> sortedList)
        {
            visitedNodes.Clear();
            recursionStack.Clear();
            cycleStack.Clear();

            for (int i = 0; i < nodes.Count; i++)
            {
                T node = nodes[i];
                if (!visitedNodes.Contains(node))
                {
                    if (!TopologicalSortRecursive(node, nodes, sortedList))
                    {
                        // A cycle is detected
                        throw new Exception("The graph contains a cycle: " + GetCyclePath());
                    }
                }
            }

            return sortedList;
        }

        private bool TopologicalSortRecursive(T node, List<T> nodes)
        {
            visitedNodes.Add(node);
            recursionStack.Add(node);
            cycleStack.Push(node);

            for (int i = 0; i < node.Dependencies.Count; i++)
            {
                T dependency = (T)node.Dependencies[i];
                if (!visitedNodes.Contains(dependency))
                {
                    if (!TopologicalSortRecursive(dependency, nodes))
                    {
                        return false;
                    }
                }
                else if (recursionStack.Contains(dependency))
                {
                    cycleStack.Push(dependency);
                    // A cycle is detected
                    return false;
                }
            }

            sortedList.Add(node);
            recursionStack.Remove(node);
            cycleStack.Pop();

            return true;
        }

        private bool TopologicalSortRecursive(T node, List<T> nodes, List<T> sortedList)
        {
            visitedNodes.Add(node);
            recursionStack.Add(node);
            cycleStack.Push(node);

            for (int i = 0; i < node.Dependencies.Count; i++)
            {
                T dependency = (T)node.Dependencies[i];
                if (!visitedNodes.Contains(dependency))
                {
                    if (!TopologicalSortRecursive(dependency, nodes))
                    {
                        return false;
                    }
                }
                else if (recursionStack.Contains(dependency))
                {
                    cycleStack.Push(dependency);
                    // A cycle is detected
                    return false;
                }
            }

            sortedList.Add(node);
            recursionStack.Remove(node);
            cycleStack.Pop();

            return true;
        }

        private string GetCyclePath()
        {
            if (cycleStack.Count == 0)
            {
                return string.Empty;
            }

            var cyclePath = cycleStack.Reverse().Select(node => node.ToString());
            return string.Join(" -> ", cyclePath);
        }
    }
}