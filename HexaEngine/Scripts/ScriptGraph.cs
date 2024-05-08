namespace HexaEngine.Scripts
{
    using HexaEngine.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class ScriptGraph
    {
        private readonly List<ScriptNode> nodes = [];
        private readonly List<ScriptNode> sorted = [];
        private readonly Dictionary<Type, ScriptNode> scriptToNode = [];
        private readonly ScriptTypeRegistry scriptTypeRegistry = new();
        private readonly TopologicalSorter<ScriptNode> sorter = new();

        public ScriptGraph()
        {
        }

        public void AddNode(Type script)
        {
            ScriptNode node = new(script, scriptTypeRegistry);
            nodes.Add(node);
            scriptToNode.Add(script, node);
            scriptTypeRegistry.Add(script, node);
        }

        public bool RemoveNode(Type script)
        {
            if (!scriptToNode.TryGetValue(script, out var node))
            {
                return false;
            }

            scriptTypeRegistry.Remove(node);
            scriptToNode.Remove(script);
            nodes.Remove(node);
            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Reset();
            }

            scriptTypeRegistry.Clear();
            nodes.Clear();
            scriptToNode.Clear();
        }

        public List<List<ScriptNode>> GroupNodesForParallelExecution()
        {
            var depthGroups = nodes.GroupBy(node => (node.Depth, node.RunInParallel)).OrderBy(group => group.Key.Depth);
            var parallelNodeLists = depthGroups.Select(group => group.ToList()).ToList();
            return parallelNodeLists;
        }

        private void CalculateDepths()
        {
            var visited = new HashSet<ScriptNode>();
            foreach (var node in nodes)
            {
                CalculateDepth(node, visited);
            }
        }

        private static void CalculateDepth(ScriptNode node, HashSet<ScriptNode> visited)
        {
            if (visited.Contains(node))
                return;

            visited.Add(node);

            if (node.Dependencies.Count == 0)
            {
                node.Depth = 0;
            }
            else
            {
                foreach (var dependency in node.Dependencies)
                {
                    CalculateDepth(dependency, visited);
                    node.Depth = Math.Max(node.Depth, dependency.Depth + 1);
                }
            }
        }

        public void Build()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                ScriptNode node = nodes[i];

                node.Builder.Build(nodes);
            }

            sorted.Clear();

            sorter.TopologicalSort(nodes, sorted);

            CalculateDepths();
        }
    }
}