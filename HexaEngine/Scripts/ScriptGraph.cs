namespace HexaEngine.Scripts
{
    using HexaEngine.Collections;
    using System.Collections.Generic;

    public class ScriptGraph
    {
        private readonly List<ScriptNode> nodes = new();
        private readonly Dictionary<ScriptComponent, ScriptNode> scriptToNode = new();
        private readonly ScriptNode root;
        private readonly ScriptTypeRegistry scriptTypeRegistry = new();
        private readonly TopologicalSorter<ScriptNode> sorter = new();

        public ScriptGraph()
        {
            root = new ScriptNode(typeof(ScriptRoot), ScriptFlags.Awake | ScriptFlags.Destroy | ScriptFlags.Update | ScriptFlags.FixedUpdate, scriptTypeRegistry);
            nodes.Add(root);
        }

        public void AddNode(ScriptComponent script)
        {
            ScriptNode node = new(script, scriptTypeRegistry);
            nodes.Add(node);
            scriptToNode.Add(script, node);
            scriptTypeRegistry.Add(script, node);
        }

        public bool RemoveNode(ScriptComponent script)
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
            root.Reset();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Reset();
            }

            scriptTypeRegistry.Clear();
            nodes.Clear();
            scriptToNode.Clear();
            nodes.Add(root);
        }

        public void Build(IList<ScriptComponent> scriptsSorted, ScriptFlags stage)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                ScriptNode node = nodes[i];
                node.Builder.Dependencies.Add(root.ScriptType); // prevent nodes from being culled away by topological sort.
                node.Builder.Build(nodes, stage);
            }
        }
    }
}